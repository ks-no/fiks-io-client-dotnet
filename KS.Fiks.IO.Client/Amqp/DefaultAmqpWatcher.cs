using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Events;

namespace KS.Fiks.IO.Client.Amqp
{
    internal class DefaultAmqpWatcher : IAmqpWatcher
    {
        private readonly ILogger<DefaultAmqpWatcher> _logger;

        public DefaultAmqpWatcher(ILoggerFactory loggerFactory = null)
        {
            if (loggerFactory != null)
            {
                _logger = loggerFactory.CreateLogger<DefaultAmqpWatcher>();
            }
        }

        // Connection events
        public Task HandleConnectionBlocked(object sender, ConnectionBlockedEventArgs connectionBlockedEventArgs)
        {
            _logger?.LogWarning($"RabbitMQ Connection Blocked: {connectionBlockedEventArgs.Reason}");
            return Task.CompletedTask;
        }

        public Task HandleConnectionUnblocked(object sender, AsyncEventArgs asyncEventArgs)
        {
            _logger?.LogInformation("RabbitMQ Connection Unblocked");
            return Task.CompletedTask;
        }

        public Task HandleConnectionShutdown(object sender, ShutdownEventArgs eventArgs)
        {
            _logger?.LogWarning($"RabbitMQ Connection Shutdown: {eventArgs.ReplyText}");
            return Task.CompletedTask;
        }

        public Task HandleConnectionRecoveryError(object sender, ConnectionRecoveryErrorEventArgs eventArgs)
        {
            _logger?.LogError(eventArgs.Exception, "RabbitMQ Connection Recovery Failed");
            return Task.CompletedTask;
        }

        public Task HandleRecoverySucceeded(object sender, AsyncEventArgs eventArgs)
        {
            _logger?.LogInformation("RabbitMQ Connection Recovery Succeeded");
            return Task.CompletedTask;
        }

        public Task HandleRecoveringConsumer(object sender, RecoveringConsumerEventArgs recoveringConsumerEvent)
        {
            _logger?.LogInformation($"RabbitMQ Recovering Consumer: {recoveringConsumerEvent.ConsumerTag}");
            return Task.CompletedTask;
        }

        // Consumer events
        public Task HandleChannelShutdown(object sender, ShutdownEventArgs eventArgs)
        {
            _logger?.LogWarning($"RabbitMQ Channel Shutdown: {eventArgs.ReplyText}");
            return Task.CompletedTask;
        }

        public Task HandleBasicChannelCancel(string consumerTag)
        {
            _logger?.LogWarning($"RabbitMQ Consumer Cancelled: {consumerTag}");
            return Task.CompletedTask;
        }

        public Task HandleBasicChannelCancelOk(string consumerTag)
        {
            _logger?.LogInformation($"RabbitMQ Consumer Cancellation Acknowledged: {consumerTag}");
            return Task.CompletedTask;
        }

        public Task HandleBasicChannelConsumeOk(string consumerTag)
        {
            _logger?.LogInformation($"RabbitMQ Consumer Successfully Started: {consumerTag}");
            return Task.CompletedTask;
        }
    }
}