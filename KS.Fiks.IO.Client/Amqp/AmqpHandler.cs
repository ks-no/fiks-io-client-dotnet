using System;
using System.Collections.Generic;
using System.Threading;
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
        private static ILogger<AmqpHandler> _logger;

        private const string QueuePrefix = "fiksio.konto.";

        private IConnectionFactory _connectionFactory;

        private IMaskinportenClient _maskinportenClient;

        private IntegrasjonConfiguration _integrasjonConfiguration;

        private IConnection _connection;

        private readonly IAmqpConsumerFactory _amqpConsumerFactory;

        private readonly KontoConfiguration _kontoConfiguration;

        private readonly SslOption _sslOption;

        private readonly Timer _ensureAmqpConnectionIsOpenTimer;

        private IModel _channel;

        private IAmqpReceiveConsumer _receiveConsumer;

        private ICredentialsRefresher _credentialsRefresher;

        private ICredentialsProvider _credentialsProvider;

        private AmqpHandler(
            IMaskinportenClient maskinportenClient,
            ISendHandler sendHandler,
            IDokumentlagerHandler dokumentlagerHandler,
            AmqpConfiguration amqpConfiguration,
            IntegrasjonConfiguration integrasjonConfiguration,
            KontoConfiguration kontoConfiguration,
            ILoggerFactory loggerFactory = null,
            IConnectionFactory connectionFactory = null,
            IAmqpConsumerFactory consumerFactory = null)
        {
            _credentialsRefresher = new TimerBasedCredentialRefresher();
            _credentialsProvider = new MaskinportenCredentialsProvider(maskinportenClient, integrasjonConfiguration);
            _sslOption = amqpConfiguration.SslOption ?? new SslOption();
            _maskinportenClient = maskinportenClient;
            _integrasjonConfiguration = integrasjonConfiguration;
            _kontoConfiguration = kontoConfiguration;
            _connectionFactory = connectionFactory ?? new ConnectionFactory
            {
                CredentialsRefresher = _credentialsRefresher,
                CredentialsProvider = _credentialsProvider
            };

            _amqpConsumerFactory = consumerFactory ?? new AmqpConsumerFactory(sendHandler, dokumentlagerHandler, _kontoConfiguration);

            if (loggerFactory != null)
            {
                _logger = loggerFactory.CreateLogger<AmqpHandler>();
            }
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
            IAmqpConsumerFactory consumerFactory = null)
        {
            var amqpHandler = new AmqpHandler(maskinportenClient, sendHandler, dokumentlagerHandler, amqpConfiguration, integrasjonConfiguration, kontoConfiguration, loggerFactory, connectionFactory, consumerFactory);
            await amqpHandler.Connect(integrasjonConfiguration, amqpConfiguration).ConfigureAwait(false);

             _logger?.LogDebug("AmqpHandler CreateAsync done");
            return amqpHandler;
        }

        public void AddMessageReceivedHandler(
            EventHandler<MottattMeldingArgs> receivedEvent,
            EventHandler<ConsumerEventArgs> cancelledEvent)
        {
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
            if (disposing)
            {
                _channel.Dispose();
                _connection.Dispose();
                _ensureAmqpConnectionIsOpenTimer?.Dispose();
            }
        }

        private async Task Connect(IntegrasjonConfiguration integrasjonConfiguration, AmqpConfiguration amqpConfiguration)
        {
            _connection = CreateConnection(amqpConfiguration);
            _channel = ConnectToChannel(amqpConfiguration);

            // Handle events for debugging
            _connection.ConnectionShutdown += HandleConnectionShutdown;
            _connection.ConnectionBlocked += HandleConnectionBlocked;
            _connection.ConnectionUnblocked += HandleConnectionUnblocked;
        }

        private void HandleConnectionBlocked(object sender, EventArgs e)
        {
            _logger?.LogDebug("RabbitMQ Connection ConnectionBlocked event has been triggered");
        }

        private void HandleConnectionUnblocked(object sender, EventArgs e)
        {
            _logger?.LogDebug("RabbitMQ Connection ConnectionUnblocked event has been triggered");
        }

        private void HandleConnectionShutdown(object sender, EventArgs shutdownEventArgs)
        {
            _logger?.LogDebug($"RabbitMQ Connection ConnectionShutdown event has been triggered");
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