using System;
using System.Threading;
using KS.Fiks.IO.Client.Configuration;
using Ks.Fiks.Maskinporten.Client;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace KS.Fiks.IO.Client.Amqp
{
    public class MaskinportenCredentialsProvider : ICredentialsProvider
    {
        private static ILogger<MaskinportenCredentialsProvider> _logger;
        private const int TokenRetrievalTimeout = 5000;
        private ReaderWriterLock _lock = new ReaderWriterLock();
        private readonly IMaskinportenClient _maskinportenClient;
        private readonly IntegrasjonConfiguration _integrasjonConfiguration;
        private MaskinportenToken _maskinportenToken;
        private const int ValidUntilBufferInSeconds = 10;

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

        public TimeSpan? ValidUntil { get; private set; }

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
                if (_maskinportenToken != null && DateTime.Now.TimeOfDay < ValidUntil)
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
             _maskinportenToken = _maskinportenClient.GetAccessToken(_integrasjonConfiguration.Scope).Result;
             ValidUntil = DateTime.Now.TimeOfDay.Add(TimeSpan.FromSeconds(_maskinportenToken.ExpiresIn - ValidUntilBufferInSeconds));
            return _maskinportenToken;
        }
    }
}