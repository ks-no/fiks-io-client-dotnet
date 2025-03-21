using System;
using System.Threading.Tasks;
using KS.Fiks.IO.Client.Models;
using RabbitMQ.Client.Events;

namespace KS.Fiks.IO.Client.Amqp
{
    public interface IAmqpHandler : IAsyncDisposable
    {
        Task AddMessageReceivedHandlerAsync(
            Func<MottattMeldingArgs, Task> receivedEvent,
            Func<ConsumerEventArgs, Task> cancelledEvent);

        Task<bool> IsOpenAsync();
    }
}