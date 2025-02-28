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

        public async Task HandleConnectionBlocked(object sender, ConnectionBlockedEventArgs connectionBlockedEventArgs)
        {
            _logger?.LogDebug($"RabbitMQ Connection Blocked: {connectionBlockedEventArgs.Reason}");
            await Task.CompletedTask.ConfigureAwait(false);
        }

        public async Task HandleConnectionUnblocked(object sender, AsyncEventArgs asyncEventArgs)
        {
            _logger?.LogDebug("RabbitMQ Connection Unblocked");
            await Task.CompletedTask.ConfigureAwait(false);
        }

        public async Task HandleConnectionShutdown(object sender, ShutdownEventArgs eventArgs)
        {
            _logger?.LogDebug($"RabbitMQ Connection Shutdown: {eventArgs.ReplyText}");
            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}