using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KS.Fiks.IO.Client.Configuration;
using KS.Fiks.IO.Client.Exceptions;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace KS.Fiks.IO.Client.Amqp
{
    public class AmqpConnectionManager
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly SslOption? _sslOption;
        private readonly SemaphoreSlim _tokenBucket;
        private readonly int _bucketSize;
        private readonly TimeSpan _tokenFillRate;
        private readonly ILogger _logger;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public AmqpConnectionManager(
            IConnectionFactory connectionFactory,
            AmqpConfiguration amqpConfiguration,
            ILoggerFactory loggerFactory = null)
        {
            _connectionFactory = connectionFactory;
            _sslOption = amqpConfiguration.SslOption;
            _bucketSize = amqpConfiguration.RateLimitConfiguration.BucketSize;
            _tokenFillRate = amqpConfiguration.RateLimitConfiguration.TokenRefillInterval;
            _tokenBucket = new SemaphoreSlim(_bucketSize, _bucketSize);
            _logger = loggerFactory?.CreateLogger("AmqpConnectionManager");

            if (!string.IsNullOrEmpty(amqpConfiguration.Vhost))
            {
                _connectionFactory.VirtualHost = amqpConfiguration.Vhost;
                _logger?.LogInformation("Set VirtualHost to {Vhost}", amqpConfiguration.Vhost);
            }

            StartTokenRefill();
        }

        private void StartTokenRefill()
        {
            _logger?.LogInformation("Amqp rate limiting configured: BucketSize={BucketSize}, TokenRefillInterval={RefillInterval}", _bucketSize, _tokenFillRate);
            Task.Run(
                async () =>
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    await Task.Delay(_tokenFillRate, _cancellationTokenSource.Token).ConfigureAwait(false);
                    if (_tokenBucket.CurrentCount >= _bucketSize)
                    {
                        continue;
                    }

                    _tokenBucket.Release();
                    _logger?.LogDebug("Token refilled. Current token count: {CurrentCount}/{BucketSize}", _tokenBucket.CurrentCount, _bucketSize);
                }
            }, _cancellationTokenSource.Token);
        }

        public void StopTokenRefill()
        {
            _cancellationTokenSource.Cancel();
            _logger?.LogInformation("Token refill process stopped");
        }

        public async Task<IConnection> CreateConnectionAsync(AmqpConfiguration configuration)
        {
            try
            {
                _logger?.LogInformation("Waiting for token to create connection...");

                if (_tokenBucket.CurrentCount == 0)
                {
                    _logger?.LogWarning("Too many connection attempts. Rate limiting active.");
                }

                await _tokenBucket.WaitAsync().ConfigureAwait(false);
                _logger?.LogDebug("Token acquired, proceeding to create connection");

                var endpoint = new AmqpTcpEndpoint(configuration.Host, configuration.Port, _sslOption);
                var connection = await _connectionFactory
                    .CreateConnectionAsync(new List<AmqpTcpEndpoint> { endpoint }, configuration.ApplicationName)
                    .ConfigureAwait(false);
                _logger?.LogInformation("Successfully created connection to {Host}:{Port}", configuration.Host, configuration.Port);
                return connection;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to create connection to {Host}:{Port}", configuration.Host, configuration.Port);
                throw new FiksIOAmqpConnectionFailedException($"Failed to create connection to {configuration.Host}:{configuration.Port}", ex);
            }
        }
    }
}