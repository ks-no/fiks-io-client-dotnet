using System;
using KS.Fiks.IO.Client.Models;
using RabbitMQ.Client.Events;

namespace KS.Fiks.IO.Client.Amqp
{
    public interface IAmqpHandler : IDisposable
    {
        void AddMessageReceivedHandler(
            EventHandler<MottattMeldingArgs> receivedEvent,
            EventHandler<ConsumerEventArgs> cancelledEvent);

        bool IsOpen();
    }
}