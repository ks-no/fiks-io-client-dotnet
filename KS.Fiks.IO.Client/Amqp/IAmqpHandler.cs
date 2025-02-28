using System;
using System.Threading.Tasks;
using KS.Fiks.IO.Client.Models;
using RabbitMQ.Client.Events;

namespace KS.Fiks.IO.Client.Amqp
{
    public interface IAmqpHandler : IDisposable
    {
        void AddMessageReceivedHandler(
            EventHandler<MottattMeldingArgs> receivedEvent,
            EventHandler<ConsumerEventArgs> cancelledEvent);

        Task AddMessageReceivedHandlerAsync(
            Func<MottattMeldingArgs, Task> receivedEvent,
            Func<ConsumerEventArgs, Task> cancelledEvent);

        bool IsOpen();

        Task<bool> IsOpenAsync();
    }
}