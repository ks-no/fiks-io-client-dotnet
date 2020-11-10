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

        private readonly IModel channel;

        private readonly IAmqpConsumerFactory amqpConsumerFactory;

        private readonly IConnectionFactory connectionFactory;

        private readonly KontoConfiguration kontoConfiguration;

        private readonly IMaskinportenClient maskinportenClient;

        private readonly SslOption sslOption;

        private IAmqpReceiveConsumer receiveConsumer;

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
            this.sslOption = amqpConfiguration.SslOption ?? new SslOption();
            this.maskinportenClient = maskinportenClient;
            this.kontoConfiguration = kontoConfiguration;
            this.connectionFactory = connectionFactory ?? new ConnectionFactory();
            SetupConnectionFactory(integrasjonConfiguration);
            this.channel = ConnectToChannel(amqpConfiguration);
            this.amqpConsumerFactory = consumerFactory ?? new AmqpConsumerFactory(sendHandler, dokumentlagerHandler, this.kontoConfiguration);
        }

        public void AddMessageReceivedHandler(
            EventHandler<MottattMeldingArgs> receivedEvent,
            EventHandler<ConsumerEventArgs> cancelledEvent)
        {
            if (this.receiveConsumer == null)
            {
                this.receiveConsumer = this.amqpConsumerFactory.CreateReceiveConsumer(this.channel);
            }

            this.receiveConsumer.Received += receivedEvent;

            this.receiveConsumer.ConsumerCancelled += cancelledEvent;

            this.channel.BasicConsume(this.receiveConsumer, GetQueueName());
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
                this.channel?.Dispose();
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
                var endpoint = new AmqpTcpEndpoint(configuration.Host, configuration.Port, this.sslOption);
                return this.connectionFactory.CreateConnection(new List<AmqpTcpEndpoint> {endpoint});
            }
            catch (Exception ex)
            {
                throw new FiksIOAmqpConnectionFailedException($"Unable to create connection. Host: {configuration.Host}; Port: {configuration.Port}; UserName:{this.connectionFactory.UserName}; SslOption.Enabled: {this.sslOption?.Enabled};SslOption.ServerName: {this.sslOption?.ServerName}", ex);
            }
        }

        private void SetupConnectionFactory(IntegrasjonConfiguration integrasjonConfiguration)
        {
            try
            {
                var maskinportenToken = this.maskinportenClient.GetAccessToken(integrasjonConfiguration.Scope).Result;
                this.connectionFactory.UserName = integrasjonConfiguration.IntegrasjonId.ToString();
                this.connectionFactory.Password = $"{integrasjonConfiguration.IntegrasjonPassord} {maskinportenToken.Token}";
            }
            catch (AggregateException ex)
            {
                throw new FiksIOAmqpSetupFailedException("Unable to setup connection factory.", ex);
            }
        }

        private string GetQueueName()
        {
            return $"{QueuePrefix}{this.kontoConfiguration.KontoId}";
        }
    }
}