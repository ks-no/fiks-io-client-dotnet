using System;
using System.Threading;
using System.Threading.Tasks;
using KS.Fiks.IO.Client.Configuration;
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

        public string Name { get; }

        public string UserName => _integrasjonConfiguration.IntegrasjonId.ToString();

        public string Password => $"{_integrasjonConfiguration.IntegrasjonPassord} {CheckState().Token}";

        public TimeSpan? ValidUntil { get; }

        public void Refresh()
        {
            _logger.LogDebug("Refresh start");
            RetrieveToken();
        }

        private MaskinportenToken CheckState()
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

            return RetrieveToken();
        }

        private MaskinportenToken RetrieveToken()
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

        private MaskinportenToken RequestOrRenewToken()
        {
            var getAccessTokenTask = Task.Run(() => _maskinportenClient.GetAccessToken(_integrasjonConfiguration.Scope));
            getAccessTokenTask.Wait();
            _maskinportenToken = getAccessTokenTask.Result;
            return _maskinportenToken;
        }
    }
}