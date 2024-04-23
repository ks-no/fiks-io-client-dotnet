namespace KS.Fiks.IO.Client.Amqp
{
    using System;
    using Microsoft.Extensions.Logging;

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

        public void HandleConnectionBlocked(object sender, EventArgs e)
        {
            _logger?.LogDebug("RabbitMQ Connection ConnectionBlocked event has been triggered");
        }

        public void HandleConnectionUnblocked(object sender, EventArgs e)
        {
            _logger?.LogDebug("RabbitMQ Connection ConnectionUnblocked event has been triggered");
        }

        public void HandleConnectionShutdown(object sender, EventArgs shutdownEventArgs)
        {
            _logger?.LogDebug($"RabbitMQ Connection ConnectionShutdown event has been triggered");
        }
    }
}
