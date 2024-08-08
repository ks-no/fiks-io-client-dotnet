using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Threading.Tasks;
using KS.Fiks.IO.Client.Configuration;
using KS.Fiks.IO.Client.Dokumentlager;
using KS.Fiks.IO.Client.Exceptions;
using KS.Fiks.IO.Client.Models;
using KS.Fiks.IO.Client.Send;
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
        private IModel _channel;
        private IAmqpReceiveConsumer _receiveConsumer;
        private EventHandler<MottattMeldingArgs> _receivedEvent;
        private EventHandler<ConsumerEventArgs> _cancelledEvent;

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
            await amqpHandler.Connect(amqpConfiguration).ConfigureAwait(false);

             _logger?.LogDebug("AmqpHandler CreateAsync done");
            return amqpHandler;
        }

        public void AddMessageReceivedHandler(
            EventHandler<MottattMeldingArgs> receivedEvent,
            EventHandler<ConsumerEventArgs> cancelledEvent)
        {
            _cancelledEvent = cancelledEvent;
            _receivedEvent = receivedEvent;

            if (_receiveConsumer == null)
            {
                _receiveConsumer = _amqpConsumerFactory.CreateReceiveConsumer(_channel);
            }

            _receiveConsumer.Received += receivedEvent;
            _receiveConsumer.ConsumerCancelled += cancelledEvent;

            _channel.BasicConsume(_receiveConsumer, GetQueueName());
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public bool IsOpen()
        {
            _logger?.LogDebug($"IsOpen status _channel: {_channel} and _connection: {_connection}");
            return _channel != null && _channel.IsOpen && _connection != null && _connection.IsOpen;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            // Unsubscribe consumer events
            if (_receivedEvent != null)
            {
                _receiveConsumer.Received -= _receivedEvent;
            }

            if (_cancelledEvent != null)
            {
                _receiveConsumer.ConsumerCancelled -= _cancelledEvent;
            }

            // Unsubscribe events for logging of RabbitMQ events
            _connection.ConnectionShutdown -= _amqpWatcher.HandleConnectionShutdown;
            _connection.ConnectionBlocked -= _amqpWatcher.HandleConnectionBlocked;
            _connection.ConnectionUnblocked -= _amqpWatcher.HandleConnectionUnblocked;

            _channel.Dispose();
            _connection.Dispose();
        }

        private Task Connect(AmqpConfiguration amqpConfiguration)
        {
            _connection = CreateConnection(amqpConfiguration);
            _channel = ConnectToChannel(amqpConfiguration);

            // Handle events for logging of RabbitMQ events
            _connection.ConnectionShutdown += _amqpWatcher.HandleConnectionShutdown;
            _connection.ConnectionBlocked += _amqpWatcher.HandleConnectionBlocked;
            _connection.ConnectionUnblocked += _amqpWatcher.HandleConnectionUnblocked;

            return Task.CompletedTask;
        }

        private IModel ConnectToChannel(AmqpConfiguration configuration)
        {
            try
            {
                var channel = _connection.CreateModel();
                channel.BasicQos(0, configuration.PrefetchCount, false);
                return channel;
            }
            catch (Exception ex)
            {
                throw new FiksIOAmqpConnectionFailedException("Unable to connect to channel", ex);
            }
        }

        private IConnection CreateConnection(AmqpConfiguration configuration)
        {
            try
            {
                var endpoint = new AmqpTcpEndpoint(configuration.Host, configuration.Port, _sslOption);
                var connection = _connectionFactory.CreateConnection(new List<AmqpTcpEndpoint> { endpoint }, configuration.ApplicationName);
                return connection;
            }
            catch (Exception ex)
            {
                throw new FiksIOAmqpConnectionFailedException($"Unable to create connection. Host: {configuration.Host}; Port: {configuration.Port}; UserName:{_connectionFactory.UserName}; SslOption.Enabled: {_sslOption?.Enabled};SslOption.ServerName: {_sslOption?.ServerName}", ex);
            }
        }

        private string GetQueueName()
        {
            return $"{QueuePrefix}{_kontoConfiguration.KontoId}";
        }
    }
}