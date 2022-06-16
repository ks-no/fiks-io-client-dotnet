using System;
using System.Collections.Generic;
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

        private readonly IConnection _connection;

        private readonly IModel _channel;

        private readonly IAmqpConsumerFactory _amqpConsumerFactory;

        private readonly IConnectionFactory _connectionFactory;

        private readonly KontoConfiguration _kontoConfiguration;

        private readonly IMaskinportenClient _maskinportenClient;

        private readonly SslOption _sslOption;

        private IAmqpReceiveConsumer _receiveConsumer;

        internal AmqpHandler(
            IMaskinportenClient maskinportenClient,
            ISendHandler sendHandler,
            IDokumentlagerHandler dokumentlagerHandler,
            AmqpConfiguration amqpConfiguration,
            IntegrasjonConfiguration integrasjonConfiguration,
            KontoConfiguration kontoConfiguration,
            IConnectionFactory connectionFactory = null,
            IAmqpConsumerFactory consumerFactory = null)
        {
            _sslOption = amqpConfiguration.SslOption ?? new SslOption();
            _maskinportenClient = maskinportenClient;
            _kontoConfiguration = kontoConfiguration;
            _connectionFactory = connectionFactory ?? new ConnectionFactory();
            SetupConnectionFactory(integrasjonConfiguration);
            _connection = CreateConnection(amqpConfiguration);
            _channel = ConnectToChannel(amqpConfiguration);
            _amqpConsumerFactory = consumerFactory ?? new AmqpConsumerFactory(sendHandler, dokumentlagerHandler, _kontoConfiguration);
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

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _channel.Dispose();
                _connection.Dispose();
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
                return _connectionFactory.CreateConnection(new List<AmqpTcpEndpoint> {endpoint}, configuration.ApplicationName);
            }
            catch (Exception ex)
            {
                throw new FiksIOAmqpConnectionFailedException($"Unable to create connection. Host: {configuration.Host}; Port: {configuration.Port}; UserName:{_connectionFactory.UserName}; SslOption.Enabled: {_sslOption?.Enabled};SslOption.ServerName: {_sslOption?.ServerName}", ex);
            }
        }

        private void SetupConnectionFactory(IntegrasjonConfiguration integrasjonConfiguration)
        {
            try
            {
                var maskinportenToken = _maskinportenClient.GetAccessToken(integrasjonConfiguration.Scope).Result;
                _connectionFactory.UserName = integrasjonConfiguration.IntegrasjonId.ToString();
                _connectionFactory.Password = $"{integrasjonConfiguration.IntegrasjonPassord} {maskinportenToken.Token}";
            }
            catch (AggregateException ex)
            {
                throw new FiksIOAmqpSetupFailedException("Unable to setup connection factory.", ex);
            }
        }

        private string GetQueueName()
        {
            return $"{QueuePrefix}{_kontoConfiguration.KontoId}";
        }
    }
}