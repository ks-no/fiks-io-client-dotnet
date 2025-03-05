using System.Threading.Tasks;
using RabbitMQ.Client.Events;

namespace KS.Fiks.IO.Client.Amqp
{
    public interface IAmqpWatcher
    {
        Task HandleConnectionBlocked(object sender, ConnectionBlockedEventArgs connectionBlockedEventArgs);

        Task HandleConnectionShutdown(object sender, ShutdownEventArgs eventArgs);

        Task HandleConnectionUnblocked(object sender, AsyncEventArgs asyncEventArgs);

        Task HandleConnectionRecoveryError(object sender, ConnectionRecoveryErrorEventArgs eventArgs);

        Task HandleRecoverySucceeded(object sender, AsyncEventArgs eventArgs);

        Task HandleRecoveringConsumer(object sender, RecoveringConsumerEventArgs recoveringConsumerEvent);

        Task HandleChannelShutdown(object sender, ShutdownEventArgs eventArgs);

        Task HandleBasicChannelCancel(string consumerTag);

        Task HandleBasicChannelCancelOk(string consumerTag);

        Task HandleBasicChannelConsumeOk(string consumerTag);
    }
}