using System;
using System.Threading.Tasks;
using RabbitMQ.Client.Events;

namespace KS.Fiks.IO.Client.Amqp
{
    public interface IAmqpWatcher
    {
        Task HandleConnectionBlocked(object sender, ConnectionBlockedEventArgs connectionBlockedEventArgs);

        Task HandleConnectionShutdown(object sender, ShutdownEventArgs eventArgs);

        Task HandleConnectionUnblocked(object sender, AsyncEventArgs asyncEventArgs);
    }
}