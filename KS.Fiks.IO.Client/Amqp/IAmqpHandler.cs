using System;
using KS.Fiks.IO.Client.Models;
using RabbitMQ.Client.Events;

namespace KS.Fiks.IO.Client.Amqp
{
    internal interface IAmqpHandler : IDisposable
    {
        void AddMessageReceivedHandler(
            EventHandler<MessageReceivedArgs> receivedEvent,
            EventHandler<ConsumerEventArgs> cancelledEvent);
    }
}