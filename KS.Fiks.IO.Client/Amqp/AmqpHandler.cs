using System;
using System.Collections.Generic;
using System.Net.Security;
using KS.Fiks.IO.Client.Configuration;
using KS.Fiks.IO.Client.Exceptions;
using KS.Fiks.IO.Client.Models;
using Ks.Fiks.Maskinporten.Client;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace KS.Fiks.IO.Client.Amqp
{
    internal class AmqpHandler : IAmqpHandler
    {
        private readonly IModel _channel;

        private readonly IAmqpConsumerFactory _amqpConsumerFactory;

        private readonly IConnectionFactory _connectionFactory;

        private readonly string _accountId;

        private readonly IMaskinportenClient _maskinportenClient;

        private IAmqpReceiveConsumer _receiveConsumer;

        internal AmqpHandler(
            IMaskinportenClient maskinportenClient,
            AmqpConfiguration amqpConfiguration,
            FiksIntegrationConfiguration integrationConfiguration,
            string accountId,
            IConnectionFactory connectionFactory = null,
            IAmqpConsumerFactory consumerFactory = null)
        {
            _maskinportenClient = maskinportenClient;
            _accountId = accountId;
            _connectionFactory = connectionFactory ?? new ConnectionFactory();
            SetupConnectionFactory(integrationConfiguration);
            _channel = ConnectToChannel(amqpConfiguration);
            _amqpConsumerFactory = consumerFactory ?? new AmqpConsumerFactory();
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

            _channel.BasicConsume(_receiveConsumer, _accountId);
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
            var sslOptions = new SslOption();
            sslOptions.Enabled = true;
            sslOptions.ServerName = "ubergenkom.no";
            sslOptions.AcceptablePolicyErrors = SslPolicyErrors.RemoteCertificateNotAvailable;
            sslOptions.CertificateValidationCallback = (sender, certificate, chain, errors) => true;

            try
            {
                var endpoint = new AmqpTcpEndpoint(configuration.Host, configuration.Port, sslOptions);
                return _connectionFactory.CreateConnection(new List<AmqpTcpEndpoint> {endpoint});
            }
            catch (Exception ex)
            {
                throw new FiksIOAmqpConnectionFailedException($"Unable to create connection. Host: {configuration.Host}; Port: {configuration.Port}; UserName:{_connectionFactory.UserName}; Password:{_connectionFactory.Password}", ex);
            }
        }

        private void SetupConnectionFactory(FiksIntegrationConfiguration integrationConfiguration)
        {
            var maskinportenToken = _maskinportenClient.GetAccessToken(integrationConfiguration.Scope).Result;
            _connectionFactory.UserName = integrationConfiguration.IntegrastionId.ToString();
            _connectionFactory.Password = $"{integrationConfiguration.IntegrationPassword} {maskinportenToken.Token}";
        }
    }
}