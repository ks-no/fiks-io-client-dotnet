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
        private readonly AmqpConnectionManager _connectionManager;
        private readonly IAmqpWatcher _amqpWatcher;
        private IConnection _connection;
        private IChannel _channel;
        private IAmqpReceiveConsumer _receiveConsumer;
        private Func<MottattMeldingArgs, Task> _receivedEvent;
        private Func<ConsumerEventArgs, Task> _cancelledEvent;
        private int _disposed;

        internal void SetConnection(IConnection connection)
        {
            _connection = connection;
        }

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
            if (loggerFactory != null)
            {
                _logger = loggerFactory.CreateLogger<AmqpHandler>();
            }

            _sslOption = amqpConfiguration.SslOption ?? new SslOption();
            _kontoConfiguration = kontoConfiguration;
            _connectionManager = new AmqpConnectionManager(
                connectionFactory ?? new ConnectionFactory
                {
                    CredentialsProvider = new MaskinportenCredentialsProvider(
                        "TokenCredentialsForMaskinporten", maskinportenClient, integrasjonConfiguration, loggerFactory)
                },
                amqpConfiguration,
                loggerFactory);

            _amqpWatcher = amqpWatcher ?? new DefaultAmqpWatcher(loggerFactory);

            _amqpConsumerFactory = consumerFactory ?? new AmqpConsumerFactory(
                sendHandler, dokumentlagerHandler, _amqpWatcher, _kontoConfiguration);
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
            var handler = new AmqpHandler(maskinportenClient, sendHandler, dokumentlagerHandler,
                amqpConfiguration, integrasjonConfiguration, kontoConfiguration,
                loggerFactory, connectionFactory, consumerFactory, amqpWatcher);

            await handler.ConnectAsync(amqpConfiguration).ConfigureAwait(false);

            _logger?.LogDebug("AmqpHandler CreateAsync done");
            return handler;
        }

        public async Task AddMessageReceivedHandlerAsync(
            Func<MottattMeldingArgs, Task> receivedEvent,
            Func<ConsumerEventArgs, Task> cancelledEvent)
        {
            _receivedEvent = receivedEvent;
            _cancelledEvent = cancelledEvent;

            if (_receiveConsumer == null)
            {
                _receiveConsumer = _amqpConsumerFactory.CreateReceiveConsumer(_channel);
            }

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
            return Task.FromResult(_channel?.IsOpen == true && _connection?.IsOpen == true);
        }

        public async ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 1)
            {
                return;
            }

            RunSafe(
                () =>
            {
                UnsubscribeConsumerEvents();
                SafelyUnsubscribeConnectionEvents();
            }, "Error during event unsubscription in DisposeAsync");

            await DisposeSafeAsync(_channel, "channel").ConfigureAwait(false);
            await DisposeSafeAsync(_connection, "connection").ConfigureAwait(false);
        }

        private void SafelyUnsubscribeConnectionEvents()
        {
            if (_connection == null || !_connection.IsOpen)
            {
                _logger?.LogDebug("Connection is closed, skipping event unsubscription");
                return;
            }

            RunSafe(UnsubscribeConnectionEvents, "Error unsubscribing connection events");
        }

        private async Task ConnectAsync(AmqpConfiguration config)
        {
            _connection = await CreateConnectionAsync(config).ConfigureAwait(false);
            _channel = await ConnectToChannelAsync(config).ConfigureAwait(false);

            SubscribeConnectionEvents();
        }

        private void SubscribeConnectionEvents()
        {
            _connection.ConnectionShutdownAsync += _amqpWatcher.HandleConnectionShutdown;
            _connection.ConnectionBlockedAsync += _amqpWatcher.HandleConnectionBlocked;
            _connection.ConnectionUnblockedAsync += _amqpWatcher.HandleConnectionUnblocked;
            _connection.RecoverySucceededAsync += _amqpWatcher.HandleRecoverySucceeded;
            _connection.RecoveringConsumerAsync += _amqpWatcher.HandleRecoveringConsumer;
            _connection.ConnectionRecoveryErrorAsync += _amqpWatcher.HandleConnectionRecoveryError;
        }

        private void UnsubscribeConnectionEvents()
        {
            _connection.ConnectionShutdownAsync -= _amqpWatcher.HandleConnectionShutdown;
            _connection.ConnectionBlockedAsync -= _amqpWatcher.HandleConnectionBlocked;
            _connection.ConnectionUnblockedAsync -= _amqpWatcher.HandleConnectionUnblocked;
            _connection.RecoverySucceededAsync -= _amqpWatcher.HandleRecoverySucceeded;
            _connection.RecoveringConsumerAsync -= _amqpWatcher.HandleRecoveringConsumer;
            _connection.ConnectionRecoveryErrorAsync -= _amqpWatcher.HandleConnectionRecoveryError;
        }

        private void UnsubscribeConsumerEvents()
        {
            if (_receivedEvent != null)
            {
                _receiveConsumer.ReceivedAsync -= _receivedEvent;
            }

            if (_cancelledEvent != null)
            {
                _receiveConsumer.ConsumerCancelledAsync -= _cancelledEvent;
            }
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
            return await _connectionManager.CreateConnectionAsync(configuration).ConfigureAwait(false);
        }

        private string GetQueueName()
        {
            return $"{QueuePrefix}{_kontoConfiguration.KontoId}";
        }

        private void RunSafe(Action action, string warningMessage)
        {
            try
            {
                action();
            }
            catch (ObjectDisposedException ex)
            {
                _logger?.LogDebug(ex, $"{warningMessage} (disposed)");
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, warningMessage);
            }
        }

        private async Task DisposeSafeAsync<T>(T disposable, string name)
            where T : class, IAsyncDisposable
        {
            if (disposable == null)
            {
                return;
            }

            try
            {
                await disposable.DisposeAsync().ConfigureAwait(false);
            }
            catch (ObjectDisposedException ex)
            {
                _logger?.LogDebug(ex, $"{name} already disposed.");
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, $"Error disposing {name} in DisposeAsync");
            }
        }
    }
}