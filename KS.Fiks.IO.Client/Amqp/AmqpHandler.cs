using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KS.Fiks.IO.Client.Configuration;
using KS.Fiks.IO.Client.Dokumentlager;
using KS.Fiks.IO.Client.Exceptions;
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


            if (loggerFactory != null)
            {
                _logger = loggerFactory.CreateLogger<AmqpHandler>();
            }

            _amqpWatcher = amqpWatcher ?? new DefaultAmqpWatcher(loggerFactory);

            _amqpConsumerFactory = consumerFactory ?? new AmqpConsumerFactory(sendHandler, dokumentlagerHandler, _amqpWatcher, _kontoConfiguration);
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

        public Task<bool> IsOpenAsync()
        {
            return Task.FromResult(_channel is { IsOpen: true } && _connection is { IsOpen: true });
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                if (_connection != null)
                {
                    UnsubscribeConnectionEvents();
                }

                if (_channel != null)
                {
                    await _channel.DisposeAsync().ConfigureAwait(false);
                }

                if (_connection != null)
                {
                    await _connection.DisposeAsync().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error disposing AmqpHandler resources.");
            }
        }

        private async Task ConnectAsync(AmqpConfiguration amqpConfiguration)
        {
            try
            {
                _connection = await CreateConnectionAsync(amqpConfiguration).ConfigureAwait(false);
                _channel = await ConnectToChannelAsync(amqpConfiguration).ConfigureAwait(false);

                SubscribeConnectionEvents();

                _logger?.LogDebug("Connected to AMQP.");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to connect to AMQP. Cleaning up resources...");

                if (_connection == null)
                {
                    throw new FiksIOAmqpConnectionFailedException("Failed to connect to AMQP.", ex);
                }

                await _connection.DisposeAsync().ConfigureAwait(false);
                _connection = null;

                throw new FiksIOAmqpConnectionFailedException("Failed to connect to AMQP.", ex);
            }
        }

        private void SubscribeConnectionEvents()
            {
                _logger?.LogDebug("Subscribing to connection events.");
                _connection.ConnectionShutdownAsync += _amqpWatcher.HandleConnectionShutdown;
                _connection.ConnectionBlockedAsync += _amqpWatcher.HandleConnectionBlocked;
                _connection.ConnectionUnblockedAsync += _amqpWatcher.HandleConnectionUnblocked;
                _connection.RecoverySucceededAsync += _amqpWatcher.HandleRecoverySucceeded;
                _connection.RecoveringConsumerAsync += _amqpWatcher.HandleRecoveringConsumer;
                _connection.ConnectionRecoveryErrorAsync += _amqpWatcher.HandleConnectionRecoveryError;
            }

        private void UnsubscribeConnectionEvents()
        {
            _logger?.LogDebug("Unsubscribing from connection events.");
            _connection.ConnectionShutdownAsync -= _amqpWatcher.HandleConnectionShutdown;
            _connection.ConnectionBlockedAsync -= _amqpWatcher.HandleConnectionBlocked;
            _connection.ConnectionUnblockedAsync -= _amqpWatcher.HandleConnectionUnblocked;
            _connection.RecoverySucceededAsync -= _amqpWatcher.HandleRecoverySucceeded;
            _connection.RecoveringConsumerAsync -= _amqpWatcher.HandleRecoveringConsumer;
            _connection.ConnectionRecoveryErrorAsync -= _amqpWatcher.HandleConnectionRecoveryError;
        }

        private async Task<IChannel> ConnectToChannelAsync(AmqpConfiguration configuration)
        {
            try
            {
                var channel = await _connection.CreateChannelAsync().ConfigureAwait(false);
                await channel.BasicQosAsync(0, configuration.PrefetchCount, false).ConfigureAwait(false);
                return channel;
            }
            catch (Exception ex)
            {
                throw new FiksIOAmqpConnectionFailedException("Unable to connect to channel", ex);
            }
        }

        private async Task<IConnection> CreateConnectionAsync(AmqpConfiguration configuration)
        {
            try
            {
                var endpoint = new AmqpTcpEndpoint(configuration.Host, configuration.Port, _sslOption);
                return await _connectionFactory
                    .CreateConnectionAsync(new List<AmqpTcpEndpoint> { endpoint }, configuration.ApplicationName)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new FiksIOAmqpConnectionFailedException(
                    $"Unable to create connection. Host: {configuration.Host}; Port: {configuration.Port}; UserName:{_connectionFactory.UserName}; SslOption.Enabled: {_sslOption?.Enabled};SslOption.ServerName: {_sslOption?.ServerName}",
                    ex);
            }
        }

        private string GetQueueName()
        {
            return $"{QueuePrefix}{_kontoConfiguration.KontoId}";
        }
    }
}