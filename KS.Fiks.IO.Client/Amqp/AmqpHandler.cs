using System;
using System.Collections.Generic;
using KS.Fiks.IO.Client.Configuration;
using KS.Fiks.IO.Client.Exceptions;
using KS.Fiks.IO.Client.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace KS.Fiks.IO.Client.Amqp
{
    internal class AmqpHandler : IAmqpHandler
    {
        private readonly IModel _channel;

        private readonly IAmqpConsumerFactory _amqpConsumerFactory;

        private readonly string _accountId;

        private IAmqpReceiveConsumer _receiveConsumer;

        internal AmqpHandler(
            AmqpConfiguration configuration,
            string accountId,
            IConnectionFactory connectionFactory = null,
            IAmqpConsumerFactory consumerFactory = null)
        {
            _accountId = accountId;
            _channel = ConnectToChannel(
                connectionFactory ?? new ConnectionFactory(),
                configuration);
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

        private IModel ConnectToChannel(IConnectionFactory connectionFactory, AmqpConfiguration configuration)
        {
            var connection = CreateConnection(connectionFactory, configuration);
            try
            {
                return connection.CreateModel();
            }
            catch (Exception ex)
            {
                throw new FiksIOAmqpConnectionFailedException("Unable to connect to channel", ex);
            }
        }

        private IConnection CreateConnection(IConnectionFactory connectionFactory, AmqpConfiguration configuration)
        {
            try
            {
                var endpoint = new AmqpTcpEndpoint(configuration.Host, configuration.Port);
                Console.WriteLine($"Trying to connect to {endpoint} (hostName: {endpoint.HostName}; port:{endpoint.Port};");
                return connectionFactory.CreateConnection(new List<AmqpTcpEndpoint> {endpoint});
            }
            catch (Exception ex)
            {
                throw new FiksIOAmqpConnectionFailedException("Unable to create connection", ex);
            }
        }
    }
}