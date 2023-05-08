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
            _sslOption = amqpConfiguration.SslOption ?? new SslOption();
            _maskinportenClient = maskinportenClient;
            _integrasjonConfiguration = integrasjonConfiguration;
            _kontoConfiguration = kontoConfiguration;
            _connectionFactory = connectionFactory ?? new ConnectionFactory();
            _amqpConsumerFactory = consumerFactory ?? new AmqpConsumerFactory(sendHandler, dokumentlagerHandler, _kontoConfiguration);

            if (amqpConfiguration.KeepAlive)
            {
                _ensureAmqpConnectionIsOpenTimer = new Timer(Callback, null, amqpConfiguration.KeepAliveHealthCheckInterval, amqpConfiguration.KeepAliveHealthCheckInterval);
            }

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
            await amqpHandler.SetupConnectionAndConnect(integrasjonConfiguration, amqpConfiguration).ConfigureAwait(false);

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

        private async Task SetupConnectionAndConnect(IntegrasjonConfiguration integrasjonConfiguration, AmqpConfiguration amqpConfiguration)
        {
            await SetupConnectionFactory(integrasjonConfiguration).ConfigureAwait(false);
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

        private async void Callback(object o)
        {
            await RefreshTokenIfNotOpen().ConfigureAwait(false);
        }

        private async Task RefreshTokenIfNotOpen()
        {
            _logger?.LogDebug("AmqpHandler RefreshTokenIfNotOpen start");
            if (!IsOpen())
            {
                _logger?.LogDebug("AmqpHandler RefreshTokenIfNotOpen - Connection according to IsOpen is not open and will try to fetch and update with new token");

                try
                {
                    await RefreshMaskinportenToken(_integrasjonConfiguration).ConfigureAwait(false);
                    _logger?.LogDebug("AmqpHandler EnsureAmqpConnectionIsOpen - Connection reconnected");
                }
                catch (Exception e)
                {
                    _logger?.LogWarning($"AmqpHandler RefreshTokenIfNotOpen - Something went wrong trying to refresh token. Error message: {e.Message}", e);
                }
            }
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

        private async Task SetupConnectionFactory(IntegrasjonConfiguration integrasjonConfiguration)
        {
            try
            {
                var maskinportenToken = await _maskinportenClient.GetAccessToken(integrasjonConfiguration.Scope).ConfigureAwait(false);
                _connectionFactory.UserName = integrasjonConfiguration.IntegrasjonId.ToString();
                _connectionFactory.Password = $"{integrasjonConfiguration.IntegrasjonPassord} {maskinportenToken.Token}";
            }
            catch (Exception ex)
            {
                _logger?.LogError("AmqpHandler - Unable to setup connection factory");
                throw new FiksIOAmqpSetupFailedException("Unable to setup connection factory.", ex);
            }
        }

        private async Task RefreshMaskinportenToken(IntegrasjonConfiguration integrasjonConfiguration)
        {
            try
            {
                var maskinportenToken = await _maskinportenClient.GetAccessToken(integrasjonConfiguration.Scope).ConfigureAwait(false);
                _connectionFactory.Password = $"{integrasjonConfiguration.IntegrasjonPassord} {maskinportenToken.Token}";
            }
            catch (Exception ex)
            {
                _logger?.LogError("AmqpHandler - Unable to refresh with latest maskinporten token");
                throw new FiksIOAmqpSetupFailedException("Unable to refresh with latest maskinporten token.", ex);
            }
        }

        private string GetQueueName()
        {
            return $"{QueuePrefix}{_kontoConfiguration.KontoId}";
        }
    }
}