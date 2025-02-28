using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KS.Fiks.IO.Client.Configuration;
using KS.Fiks.IO.Client.Dokumentlager;
using KS.Fiks.IO.Client.Models;
using KS.Fiks.IO.Client.Send;
using KS.Fiks.IO.Send.Client.Configuration;
using Ks.Fiks.Maskinporten.Client;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace KS.Fiks.IO.Client.Amqp
{
    internal class AmqpHandler : IAmqpHandler
    {
        private const string QueuePrefix = "fiksio.konto.";
        private static ILogger<AmqpHandler> _logger;
        private readonly IAmqpConsumerFactory _amqpConsumerFactory;
        private readonly KontoConfiguration _kontoConfiguration;
        private readonly SslOption _sslOption;
        private readonly IConnectionFactory _connectionFactory;
        private readonly IAmqpWatcher _amqpWatcher;
        private IConnection _connection;
        private IChannel _channel;
        private IAmqpReceiveConsumer _receiveConsumer;

        private AmqpHandler(
            IMaskinportenClient maskinportenClient,
            ISendHandler sendHandler,
            IDokumentlagerHandler dokumentlagerHandler,
            AmqpConfiguration amqpConfiguration,
            IntegrasjonConfiguration integrasjonConfiguration,
            KontoConfiguration kontoConfiguration,
            ILoggerFactory loggerFactory = null,
            IConnectionFactory connectionFactory = null,
            IAmqpConsumerFactory consumerFactory = null,
            IAmqpWatcher amqpWatcher = null)
        {
            _sslOption = amqpConfiguration.SslOption ?? new SslOption();
            _kontoConfiguration = kontoConfiguration;
            _connectionFactory = connectionFactory ?? new ConnectionFactory
            {
                CredentialsProvider = new MaskinportenCredentialsProvider("TokenCredentialsForMaskinporten", maskinportenClient, integrasjonConfiguration, loggerFactory)
            };

            if (!string.IsNullOrEmpty(amqpConfiguration.Vhost))
            {
                _connectionFactory.VirtualHost = amqpConfiguration.Vhost;
            }

            _amqpConsumerFactory = consumerFactory ?? new AmqpConsumerFactory(sendHandler, dokumentlagerHandler, _kontoConfiguration);

            if (loggerFactory != null)
            {
                _logger = loggerFactory.CreateLogger<AmqpHandler>();
            }

            _amqpWatcher = amqpWatcher ?? new DefaultAmqpWatcher(loggerFactory);
        }

        public static async Task<IAmqpHandler> CreateAsync(
            IMaskinportenClient maskinportenClient,
            ISendHandler sendHandler,
            IDokumentlagerHandler dokumentlagerHandler,
            AmqpConfiguration amqpConfiguration,
            IntegrasjonConfiguration integrasjonConfiguration,
            KontoConfiguration kontoConfiguration,
            ILoggerFactory loggerFactory = null,
            IConnectionFactory connectionFactory = null,
            IAmqpConsumerFactory consumerFactory = null,
            IAmqpWatcher amqpWatcher = null)
        {
            var amqpHandler = new AmqpHandler(maskinportenClient, sendHandler, dokumentlagerHandler, amqpConfiguration, integrasjonConfiguration, kontoConfiguration, loggerFactory, connectionFactory, consumerFactory, amqpWatcher);
            await amqpHandler.ConnectAsync(amqpConfiguration).ConfigureAwait(false);

            _logger?.LogDebug("AmqpHandler CreateAsync done");
            return amqpHandler;
        }

        public void AddMessageReceivedHandler(
            EventHandler<MottattMeldingArgs> receivedEvent,
            EventHandler<ConsumerEventArgs> cancelledEvent)
        {
            AddMessageReceivedHandlerAsync(AsyncReceived, AsyncCancelled)
                .GetAwaiter().GetResult();
            return;

            Task AsyncCancelled(ConsumerEventArgs args)
            {
                cancelledEvent?.Invoke(this, args);
                return Task.CompletedTask;
            }

            Task AsyncReceived(MottattMeldingArgs args)
            {
                receivedEvent?.Invoke(this, args);
                return Task.CompletedTask;
            }
        }

        public async Task AddMessageReceivedHandlerAsync(
            Func<MottattMeldingArgs, Task> receivedEvent,
            Func<ConsumerEventArgs, Task> cancelledEvent)
        {
            _receiveConsumer ??= _amqpConsumerFactory.CreateReceiveConsumer(_channel);

            _receiveConsumer.ReceivedAsync += receivedEvent;
            _receiveConsumer.ConsumerCancelledAsync += cancelledEvent;

            await _channel.BasicConsumeAsync(
                queue: GetQueueName(),
                autoAck: false,
                consumer: _receiveConsumer,
                cancellationToken: CancellationToken.None).ConfigureAwait(false);
        }

        public bool IsOpen()
        {
            return _channel is { IsOpen: true } && _connection is { IsOpen: true };
        }

        public Task<bool> IsOpenAsync()
        {
            return Task.FromResult(IsOpen());
        }

        public void Dispose()
        {
            DisposeAsync().GetAwaiter().GetResult();
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            if (_connection is { IsOpen: true })
            {
                _connection.ConnectionShutdownAsync -= _amqpWatcher.HandleConnectionShutdown;
                _connection.ConnectionBlockedAsync -= _amqpWatcher.HandleConnectionBlocked;
                _connection.ConnectionUnblockedAsync -= _amqpWatcher.HandleConnectionUnblocked;

                await _channel.DisposeAsync().ConfigureAwait(false);
                await _connection.DisposeAsync().ConfigureAwait(false);
            }
        }

        private async Task ConnectAsync(AmqpConfiguration amqpConfiguration)
        {
            _connection = await CreateConnectionAsync(amqpConfiguration).ConfigureAwait(false);
            _channel = await ConnectToChannelAsync(amqpConfiguration).ConfigureAwait(false);
        }

        private async Task<IChannel> ConnectToChannelAsync(AmqpConfiguration configuration)
        {
            var channel = await _connection.CreateChannelAsync().ConfigureAwait(false);
            await channel.BasicQosAsync(0, configuration.PrefetchCount, false).ConfigureAwait(false);
            return channel;
        }

        private async Task<IConnection> CreateConnectionAsync(AmqpConfiguration configuration)
        {
            var endpoint = new AmqpTcpEndpoint(configuration.Host, configuration.Port, _sslOption);
            return await _connectionFactory.CreateConnectionAsync(new List<AmqpTcpEndpoint> { endpoint }, configuration.ApplicationName).ConfigureAwait(false);
        }

        private string GetQueueName()
        {
            return $"{QueuePrefix}{_kontoConfiguration.KontoId}";
        }
    }
}