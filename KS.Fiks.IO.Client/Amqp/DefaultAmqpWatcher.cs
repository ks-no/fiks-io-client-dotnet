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

        public Task HandleConnectionBlocked(object sender, ConnectionBlockedEventArgs connectionBlockedEventArgs)
        {
            _logger?.LogDebug($"RabbitMQ Connection Blocked: {connectionBlockedEventArgs.Reason}");
            return Task.CompletedTask;
        }

        public Task HandleConnectionUnblocked(object sender, AsyncEventArgs asyncEventArgs)
        {
            _logger?.LogDebug("RabbitMQ Connection Unblocked");
            return Task.CompletedTask;
        }

        public Task HandleConnectionShutdown(object sender, ShutdownEventArgs eventArgs)
        {
            _logger?.LogDebug($"RabbitMQ Connection Shutdown: {eventArgs.ReplyText}");
            return Task.CompletedTask;
        }
    }
}