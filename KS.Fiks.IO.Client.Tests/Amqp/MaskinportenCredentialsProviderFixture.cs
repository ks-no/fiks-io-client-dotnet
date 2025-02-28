using System;
using System.Threading.Tasks;
using KS.Fiks.IO.Client.Amqp;
using KS.Fiks.IO.Send.Client.Configuration;
using Ks.Fiks.Maskinporten.Client;
using Microsoft.Extensions.Logging;
using Moq;

namespace KS.Fiks.IO.Client.Tests.Amqp
{
    public class MaskinportenCredentialsProviderFixture
    {
        private string _firstToken = "firsttesttoken";
        private string _newToken = "newValidToken";
        private Guid _integrationId = Guid.NewGuid();
        private string _integrationPassword = "defaultPassword";
        private int _expiresIn = 100;
        private bool _shouldThrowException;
        private ILoggerFactory _loggerFactory;
        private bool _shouldTimeout;

        public Guid IntegrationId => _integrationId;

        public string IntegrationPassword => _integrationPassword;

        public Mock<IMaskinportenClient> MaskinportenClientMock { get; } = new Mock<IMaskinportenClient>();

        internal MaskinportenCredentialsProvider CreateSut()
        {
            var conf = new IntegrasjonConfiguration(_integrationId, IntegrationPassword);
            SetupMocks();

            return new MaskinportenCredentialsProvider(
                "Test",
                MaskinportenClientMock.Object,
                conf,
                _loggerFactory);
        }

        private void SetupMocks()
        {
            if (_shouldThrowException)
            {
                MaskinportenClientMock
                    .Setup(_ => _.GetAccessToken(It.IsAny<string>()))
                    .ThrowsAsync(new Exception("Token retrieval failed"));
            }
            else if (_shouldTimeout)
            {
                MaskinportenClientMock
                    .Setup(_ => _.GetAccessToken(It.IsAny<string>()))
                    .Returns(async () =>
                    {
                        await Task.Delay(6000);
                        return new MaskinportenToken(_firstToken, _expiresIn);
                    });
            }
            else
            {
                MaskinportenClientMock
                    .SetupSequence(_ => _.GetAccessToken(It.IsAny<string>()))
                    .ReturnsAsync(new MaskinportenToken(_firstToken, _expiresIn)) 
                    .ReturnsAsync(new MaskinportenToken(_newToken, 100));
            }
        }

        public MaskinportenCredentialsProviderFixture WithMaskinportenToken(string token, int expiresIn = 100)
        {
            _firstToken = token;
            _expiresIn = expiresIn;
            return this;
        }

        public MaskinportenCredentialsProviderFixture WithNewToken(string token)
        {
            _newToken = token;
            return this;
        }

        public MaskinportenCredentialsProviderFixture WithIntegrationPassword(string password)
        {
            _integrationPassword = password;
            return this;
        }

        public MaskinportenCredentialsProviderFixture WithTokenExpiresIn(int expiresIn)
        {
            _expiresIn = expiresIn;
            return this;
        }

        public MaskinportenCredentialsProviderFixture WithTokenRetrievalException()
        {
            _shouldThrowException = true;
            return this;
        }

        public MaskinportenCredentialsProviderFixture WithTokenRetrievalTimeout()
        {
            _shouldTimeout = true;
            return this;
        }

        public MaskinportenCredentialsProviderFixture WithLoggerFactory(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
            return this;
        }
    }
}