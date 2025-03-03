using System;
using System.Threading;
using System.Threading.Tasks;
using KS.Fiks.IO.Client.Configuration;
using KS.Fiks.IO.Send.Client.Configuration;
using Ks.Fiks.Maskinporten.Client;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace KS.Fiks.IO.Client.Amqp
{
    public class MaskinportenCredentialsProvider : ICredentialsProvider
    {
        private const int TokenRetrievalTimeout = 5000;
        private static ILogger<MaskinportenCredentialsProvider> _logger;
        private readonly IMaskinportenClient _maskinportenClient;
        private readonly IntegrasjonConfiguration _integrasjonConfiguration;
        private ReaderWriterLock _lock = new ReaderWriterLock();
        private MaskinportenToken _maskinportenToken;
        private ICredentialsProvider _credentialsProviderImplementation;

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
            var token = await CheckState().ConfigureAwait(false);
            var password = $"{_integrasjonConfiguration.IntegrasjonPassord} {token.Token}";

            return new Credentials(Name, UserName, password, ValidUntil);
        }

        public string Name { get; }

        public string UserName => _integrasjonConfiguration.IntegrasjonId.ToString();


        public TimeSpan? ValidUntil { get; }

        public void Refresh()
        {
            _logger.LogDebug("Refresh start");
            RetrieveToken();
        }

        private async Task<MaskinportenToken> CheckState()
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

            return await RetrieveToken().ConfigureAwait(false);
        }

        private Task<MaskinportenToken> RetrieveToken()
        {
            _lock.AcquireWriterLock(TokenRetrievalTimeout);
            try
            {
                return RequestOrRenewToken();
            }
            finally
            {
                _lock.ReleaseReaderLock();
            }
        }

        private async Task<MaskinportenToken> RequestOrRenewToken()
        {
            try
            {
                _maskinportenToken = await _maskinportenClient
                    .GetAccessToken(_integrasjonConfiguration.Scope)
                    .ConfigureAwait(false);
                return _maskinportenToken;
            }
            catch (Exception ex)
            {
                throw new Exception("Token retrieval failed", ex);
            }
        }
    }
}