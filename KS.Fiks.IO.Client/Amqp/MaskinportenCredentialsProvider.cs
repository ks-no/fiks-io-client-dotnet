using System;
using System.Threading;
using System.Threading.Tasks;
using KS.Fiks.IO.Send.Client.Configuration;
using Ks.Fiks.Maskinporten.Client;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace KS.Fiks.IO.Client.Amqp
{
    public sealed class MaskinportenCredentialsProvider : ICredentialsProvider, IDisposable
    {
        private static readonly TimeSpan TokenRetrievalTimeout = TimeSpan.FromMilliseconds(5000); // 5 seconds timeout
        private readonly IMaskinportenClient _maskinportenClient;
        private readonly IntegrasjonConfiguration _integrasjonConfiguration;
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private readonly ILogger<MaskinportenCredentialsProvider> _logger;
        private MaskinportenToken _maskinportenToken;
        private bool _disposed;

        public MaskinportenCredentialsProvider(
            string name,
            IMaskinportenClient maskinportenClient,
            IntegrasjonConfiguration integrasjonConfiguration,
            ILoggerFactory loggerFactory = null)
        {
            Name = name;
            _maskinportenClient = maskinportenClient;
            _integrasjonConfiguration = integrasjonConfiguration;
            _logger = loggerFactory?.CreateLogger<MaskinportenCredentialsProvider>();
        }

        public string Name { get; }

        public string UserName => _integrasjonConfiguration.IntegrasjonId.ToString();

        public async Task<Credentials> GetCredentialsAsync(CancellationToken cancellationToken = default)
        {
            var token = await CheckStateAsync(cancellationToken).ConfigureAwait(false);
            var password = $"{_integrasjonConfiguration.IntegrasjonPassord} {token.Token}";
            return new Credentials(Name, UserName, password, null);
        }

        public async Task RefreshAsync(CancellationToken cancellationToken = default)
        {
            _logger?.LogDebug("Refresh start");
            await RetrieveTokenAsync(cancellationToken).ConfigureAwait(false);
        }

        private async Task<MaskinportenToken> CheckStateAsync(CancellationToken cancellationToken)
        {
            if (_lock.TryEnterUpgradeableReadLock(TokenRetrievalTimeout))
            {
                try
                {
                    if (_maskinportenToken != null && !_maskinportenToken.IsExpiring())
                    {
                        return _maskinportenToken;
                    }
                }
                finally
                {
                    _lock.ExitUpgradeableReadLock();
                }

                _logger?.LogDebug("Refreshing token due to expiration or absence.");
                return await RetrieveTokenAsync(cancellationToken).ConfigureAwait(false);
            }
            else
            {
                _logger?.LogError("Failed to acquire upgradeable read lock within timeout.");
                throw new TimeoutException("Unable to acquire upgradeable read lock within timeout.");
            }
        }

        private async Task<MaskinportenToken> RetrieveTokenAsync(CancellationToken cancellationToken)
        {
            if (_lock.TryEnterWriteLock(TokenRetrievalTimeout))
            {
                try
                {
                    _logger?.LogDebug("Requesting or renewing Maskinporten token");
                    _maskinportenToken = await _maskinportenClient
                        .GetAccessToken(_integrasjonConfiguration.Scope)
                        .ConfigureAwait(false);
                    return _maskinportenToken;
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
            else
            {
                _logger?.LogError("Failed to acquire write lock within timeout.");
                throw new TimeoutException("Unable to acquire write lock within timeout.");
            }
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _lock?.Dispose();
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~MaskinportenCredentialsProvider()
        {
            Dispose(false);
        }
    }
}