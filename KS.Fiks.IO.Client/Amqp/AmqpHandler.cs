using System;
using System.Collections.Generic;
using System.Net.Security;
using KS.Fiks.IO.Client.Configuration;
using KS.Fiks.IO.Client.Dokumentlager;
using KS.Fiks.IO.Client.Exceptions;
using KS.Fiks.IO.Client.Models;
using KS.Fiks.IO.Client.Send;
using Ks.Fiks.Maskinporten.Client;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace KS.Fiks.IO.Client.Amqp
{
    internal class AmqpHandler : IAmqpHandler
    {
        private const string QueuePrefix = "fiksio.konto.";

        private readonly IModel _channel;

        private readonly IAmqpConsumerFactory _amqpConsumerFactory;

        private readonly IConnectionFactory _connectionFactory;

        private readonly AccountConfiguration _accountConfiguration;

        private readonly IMaskinportenClient _maskinportenClient;

        private readonly IDokumentlagerHandler _dokumentlagerHandler;

        private IAmqpReceiveConsumer _receiveConsumer;

        internal AmqpHandler(
            IMaskinportenClient maskinportenClient,
            ISendHandler sendHandler,
            IDokumentlagerHandler dokumentlagerHandler,
            AmqpConfiguration amqpConfiguration,
            IntegrationConfiguration integrationConfiguration,
            AccountConfiguration accountConfiguration,
            IConnectionFactory connectionFactory = null,
            IAmqpConsumerFactory consumerFactory = null)
        {
            _maskinportenClient = maskinportenClient;
            _dokumentlagerHandler = dokumentlagerHandler;
            _accountConfiguration = accountConfiguration;
            _connectionFactory = connectionFactory ?? new ConnectionFactory();
            SetupConnectionFactory(integrationConfiguration);
            _channel = ConnectToChannel(amqpConfiguration);
            _amqpConsumerFactory = consumerFactory ?? new AmqpConsumerFactory(sendHandler, _accountConfiguration);
        }

        public void AddMessageReceivedHandler(
            EventHandler<MessageReceivedArgs> receivedEvent,
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

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _channel?.Dispose();
            }
        }

        private IModel ConnectToChannel(AmqpConfiguration configuration)
        {
            var connection = CreateConnection(configuration);
            try
            {
                return connection.CreateModel();
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
                var endpoint = new AmqpTcpEndpoint(configuration.Host, configuration.Port, GetSslOptions());
                return _connectionFactory.CreateConnection(new List<AmqpTcpEndpoint> {endpoint});
            }
            catch (Exception ex)
            {
                throw new FiksIOAmqpConnectionFailedException($"Unable to create connection. Host: {configuration.Host}; Port: {configuration.Port}; UserName:{_connectionFactory.UserName};", ex);
            }
        }

        private void SetupConnectionFactory(IntegrationConfiguration integrationConfiguration)
        {
            try
            {
                var maskinportenToken = _maskinportenClient.GetAccessToken(integrationConfiguration.Scope).Result;
                _connectionFactory.UserName = integrationConfiguration.IntegrationId.ToString();
                _connectionFactory.Password = $"{integrationConfiguration.IntegrationPassword} {maskinportenToken.Token}";
            }
            catch (AggregateException ex)
            {
                throw new FiksIOAmqpSetupFailedException("Unable to setup connection factory.", ex);
            }
        }

        private SslOption GetSslOptions()
        {
            return new SslOption
            {
                Enabled = true,
                ServerName = "ubergenkom.no",
                AcceptablePolicyErrors = SslPolicyErrors.RemoteCertificateNotAvailable,
                CertificateValidationCallback = (sender, certificate, chain, errors) => true
            };
        }

        private string GetQueueName()
        {
            return $"{QueuePrefix}{_accountConfiguration.AccountId}";
        }
    }
}