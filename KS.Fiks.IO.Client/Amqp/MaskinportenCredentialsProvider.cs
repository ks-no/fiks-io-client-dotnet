using System;
using System.Threading;
using KS.Fiks.IO.Client.Configuration;
using Ks.Fiks.Maskinporten.Client;
using RabbitMQ.Client;
using RabbitMQ.Client.OAuth2;

namespace KS.Fiks.IO.Client.Amqp
{
    public class Token : IToken
    {
        private readonly JsonToken _source;
        private readonly DateTime _lastTokenRenewal;

        public Token(JsonToken json)
        {
            this._source = json;
            this._lastTokenRenewal = DateTime.Now;
        }

        public string AccessToken
        {
            get
            {
                return _source.access_token;
            }
        }

        public string RefreshToken
        {
            get
            {
                return _source.refresh_token;
            }
        }

        public TimeSpan ExpiresIn
        {
            get
            {
                return TimeSpan.FromSeconds(_source.expires_in);
            }
        }

        bool IToken.hasExpired
        {
            get
            {
                TimeSpan age = DateTime.Now - _lastTokenRenewal;
                return age > ExpiresIn;
            }
        }
    }
    
    public class MaskinportenCredentialsProvider : ICredentialsProvider
    {
        const int TOKEN_RETRIEVAL_TIMEOUT = 5000;
        private ReaderWriterLock _lock = new ReaderWriterLock();
        private IMaskinportenClient _maskinportenClient;
        private IntegrasjonConfiguration _integrasjonConfiguration;
        private MaskinportenToken _maskinportenToken;

        public MaskinportenCredentialsProvider(IMaskinportenClient maskinportenClient, IntegrasjonConfiguration integrasjonConfiguration)
        {
            _maskinportenClient = maskinportenClient;
            _integrasjonConfiguration = integrasjonConfiguration;
        }

        public string Name { get; }

        public string UserName => _integrasjonConfiguration.IntegrasjonId.ToString();

        public string Password => $"{_integrasjonConfiguration.IntegrasjonPassord} {checkState().Token}";

        public TimeSpan? ValidUntil
        {
            get
            {
                var t = checkState();
                if (t is null)
                {
                    return null;
                }

                return TimeSpan.FromSeconds(t.ExpiresIn);
            }
        }

        public void Refresh()
        {
            retrieveToken();
        }

        private MaskinportenToken checkState()
        {
            _lock.AcquireReaderLock(TOKEN_RETRIEVAL_TIMEOUT);
            try
            {
                if (_maskinportenToken != null)
                {
                    return _maskinportenToken;
                }
            }
            finally
            {
                _lock.ReleaseReaderLock();
            }

            return retrieveToken();
        }

        private MaskinportenToken retrieveToken()
        {
            _lock.AcquireWriterLock(TOKEN_RETRIEVAL_TIMEOUT);
            try
            {
                return requestOrRenewToken();
            }
            finally
            {
                _lock.ReleaseReaderLock();
            }
        }

        private MaskinportenToken requestOrRenewToken()
        {
            return _maskinportenClient.GetAccessToken(_integrasjonConfiguration.Scope).Result;
        }
    }
}