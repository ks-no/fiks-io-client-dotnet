using System;
using KS.Fiks.IO.Client.Exceptions;
using KS.Fiks.IO.Client.Models;
using RabbitMQ.Client;

namespace KS.Fiks.IO.Client.Amqp
{
    public class AmqpHandler : IAmqpHandler
    {
        private readonly IModel _channel;

        private readonly IAmqpConsumerFactory _amqpConsumerFactory;

        private IAmqpReceiveConsumer _receiveConsumer;


        public AmqpHandler(IConnectionFactory connectionFactory, IAmqpConsumerFactory consumerFactory = null)
        {
            _channel = ConnectToChannel(connectionFactory);
            _amqpConsumerFactory = consumerFactory ?? new AmqpConsumerFactory();
        }

        public void AddReceivedListener(EventHandler<MessageReceivedArgs> receivedEvent)
        {
            if (_receiveConsumer == null)
            {
                _receiveConsumer = _amqpConsumerFactory.CreateReceiveConsumer(_channel);
            }

            _receiveConsumer.Received += receivedEvent;

            // _channel.BasicConsume(_receiveConsumer, "queue");
        }

        private static IModel ConnectToChannel(IConnectionFactory connectionFactory)
        {
            var connection = CreateConnection(connectionFactory);
            try
            {
                return connection.CreateModel();
            }
            catch (Exception ex)
            {
                throw new AmqpConnectionFailedException("Unable to connect to channel", ex);
            }
        }

        private static IConnection CreateConnection(IConnectionFactory connectionFactory)
        {
            try
            {
                return connectionFactory.CreateConnection();
            }
            catch (Exception ex)
            {
                throw new AmqpConnectionFailedException("Unable to create connection", ex);
            }
        }
    }
}