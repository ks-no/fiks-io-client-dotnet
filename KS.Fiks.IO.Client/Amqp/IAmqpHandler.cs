using System;
using KS.Fiks.IO.Client.Models;
using RabbitMQ.Client.Events;

namespace KS.Fiks.IO.Client.Amqp
{
    public interface IAmqpHandler
    {
        void AddReceivedListener(EventHandler<MessageReceivedArgs> receivedEvent);

        void AddReceivedListener(
            EventHandler<MessageReceivedArgs> receivedEvent,
            EventHandler<ConsumerEventArgs> cancelledEvent);
    }
}