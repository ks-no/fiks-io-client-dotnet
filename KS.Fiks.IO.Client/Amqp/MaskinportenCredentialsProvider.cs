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
        private const int TokenRetrievalTimeout = 5000; // 5 seconds timeout
        private static ILogger<MaskinportenCredentialsProvider> _logger;
        private readonly IMaskinportenClient _maskinportenClient;
        private readonly IntegrasjonConfiguration _integrasjonConfiguration;
        private readonly ReaderWriterLock _lock = new ReaderWriterLock();
        private MaskinportenToken _maskinportenToken;
        private bool _disposed;

        public MaskinportenCredentialsProvider(
            string name,
            IMaskinportenClient maskinportenClient,
            IntegrasjonConfiguration integrasjonConfiguration,
            ILoggerFactory loggerFactory = null)
        {
            Name = name;
            _maskinportenClient = maskinportenClient ?? throw new ArgumentNullException(nameof(maskinportenClient));
            _integrasjonConfiguration = integrasjonConfiguration ?? throw new ArgumentNullException(nameof(integrasjonConfiguration));
            _logger = loggerFactory?.CreateLogger<MaskinportenCredentialsProvider>();
        }

        public async Task<Credentials> GetCredentialsAsync(CancellationToken cancellationToken = default)
        {
            _logger?.LogDebug("Retrieving credentials asynchronously");

            var token = await RetrieveTokenAsync(cancellationToken).ConfigureAwait(false);
            var password = $"{_integrasjonConfiguration.IntegrasjonPassord} {token.Token}";

            return new Credentials(Name, UserName, password, ValidUntil);
        }

        public string Name { get; }

        public string UserName => _integrasjonConfiguration.IntegrasjonId.ToString();

        public string Password => $"{_integrasjonConfiguration.IntegrasjonPassord} {CheckState().Token}";

        private TimeSpan? ValidUntil => _maskinportenToken != null
            ? _maskinportenToken.IsExpiring()
                ? TimeSpan.Zero
                : (TimeSpan?)(RequestNewTokenAfterTime - DateTime.UtcNow)
            : null;

        public void Refresh()
        {
            _logger?.LogDebug("Refresh start");
            _ = RetrieveTokenAsync(CancellationToken.None).GetAwaiter().GetResult();
        }

        private MaskinportenToken CheckState()
        {
            try
            {
                _lock.AcquireReaderLock(TokenRetrievalTimeout);
                try
                {
                    if (_maskinportenToken != null && !_maskinportenToken.IsExpiring())
                    {
                        return _maskinportenToken;
                    }
                }
                finally
                {
                    _lock.ReleaseReaderLock();
                }
            }
            catch (ApplicationException ex)
            {
                _logger?.LogError(ex, "Timeout while acquiring read lock");
            }

            return RequestOrRenewToken();
        }

        private async Task<MaskinportenToken> RetrieveTokenAsync(CancellationToken cancellationToken)
        {
            try
            {
                _lock.AcquireWriterLock(TokenRetrievalTimeout);
                try
                {
                    _logger?.LogDebug("Requesting or renewing Maskinporten token asynchronously");
                    _maskinportenToken = await _maskinportenClient
                        .GetAccessToken(_integrasjonConfiguration.Scope)
                        .ConfigureAwait(false);
                    return _maskinportenToken;
                }
                finally
                {
                    _lock.ReleaseWriterLock();
                }
            }
            catch (ApplicationException ex)
            {
                _logger?.LogError(ex, "Timeout while acquiring write lock");
                throw;
            }
        }

        private MaskinportenToken RequestOrRenewToken()
        {
            try
            {
                _lock.AcquireWriterLock(TokenRetrievalTimeout);
                try
                {
                    _logger?.LogDebug("Requesting or renewing Maskinporten token synchronously");
                    _maskinportenToken = _maskinportenClient
                        .GetAccessToken(_integrasjonConfiguration.Scope)
                        .GetAwaiter()
                        .GetResult();
                    return _maskinportenToken;
                }
                finally
                {
                    _lock.ReleaseWriterLock();
                }
            }
            catch (ApplicationException ex)
            {
                _logger?.LogError(ex, "Timeout while acquiring write lock");
                throw;
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
                // Dispose managed resources if needed
                _lock.ReleaseLock();
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

        private DateTime RequestNewTokenAfterTime =>
            _maskinportenToken != null && !_maskinportenToken.IsExpiring()
                ? DateTime.UtcNow.AddSeconds(30)
                : DateTime.UtcNow;
    }
}