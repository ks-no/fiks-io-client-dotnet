using System;
using System.Threading;
using System.Threading.Tasks;
using KS.Fiks.IO.Send.Client.Configuration;
using Ks.Fiks.Maskinporten.Client;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace KS.Fiks.IO.Client.Amqp
{
    public class MaskinportenCredentialsProvider : ICredentialsProvider
    {
        private const int TokenRetrievalTimeout = 5;
        private static ILogger<MaskinportenCredentialsProvider> _logger;
        private readonly IMaskinportenClient _maskinportenClient;
        private readonly IntegrasjonConfiguration _integrasjonConfiguration;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private MaskinportenToken _maskinportenToken;

        public MaskinportenCredentialsProvider(string name, IMaskinportenClient maskinportenClient, IntegrasjonConfiguration integrasjonConfiguration, ILoggerFactory loggerFactory = null)
        {
            Name = name;
            _maskinportenClient = maskinportenClient;
            _integrasjonConfiguration = integrasjonConfiguration;
            if (loggerFactory != null)
            {
                _logger = loggerFactory.CreateLogger<MaskinportenCredentialsProvider>();
            }
        }

        public async Task<Credentials> GetCredentialsAsync(CancellationToken cancellationToken = default)
        {
            var token = await CheckStateAsync(cancellationToken).ConfigureAwait(false);
            var password = $"{_integrasjonConfiguration.IntegrasjonPassord} {token.Token}";

            return new Credentials(Name, UserName, password, ValidUntil);
        }

        public string Name { get; }

        private string UserName => _integrasjonConfiguration.IntegrasjonId.ToString();

        public TimeSpan? ValidUntil { get; }

        public async Task RefreshAsync()
        {
            _logger?.LogDebug("Refreshing token...");
            await RetrieveToken().ConfigureAwait(false);
        }

        private async Task<MaskinportenToken> CheckStateAsync(CancellationToken cancellationToken)
        {
            await _lock.WaitAsync(TimeSpan.FromSeconds(TokenRetrievalTimeout), cancellationToken).ConfigureAwait(false);
            try
            {
                if (_maskinportenToken != null && !_maskinportenToken.IsExpiring())
                {
                    return _maskinportenToken;
                }

                return await RetrieveToken().ConfigureAwait(false);
            }
            finally
            {
                _lock.Release();
            }
        }

        private async Task<MaskinportenToken> RetrieveToken()
        {
            try
            {
                _logger?.LogInformation("Requesting new Maskinporten token...");
                _maskinportenToken = await _maskinportenClient
                    .GetAccessToken(_integrasjonConfiguration.Scope)
                    .ConfigureAwait(false);

                _logger?.LogInformation("Successfully retrieved new Maskinporten token.");
                return _maskinportenToken;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to retrieve Maskinporten token.");
                throw;
            }
        }
    }
}