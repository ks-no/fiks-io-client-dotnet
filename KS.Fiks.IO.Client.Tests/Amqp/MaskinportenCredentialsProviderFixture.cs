using System;
using KS.Fiks.IO.Client.Amqp;
using KS.Fiks.IO.Client.Configuration;
using KS.Fiks.IO.Send.Client.Configuration;
using Ks.Fiks.Maskinporten.Client;
using Moq;

namespace KS.Fiks.IO.Client.Tests.Amqp
{
    public class MaskinportenCredentialsProviderFixture
    {
        private bool _connectionFactoryShouldThrow;
        private bool _connectionShouldThrow;
        private string _firstToken = "firsttesttoken";
        private Guid _integrationId = Guid.NewGuid();
        private string _integrationPassword = "defaultPassword";
        private int _expiresIn = 100;

        internal Mock<IMaskinportenClient> MaskinportenClientMock { get; } = new Mock<IMaskinportenClient>();

        internal MaskinportenCredentialsProvider CreateSut()
        {
            var conf = new IntegrasjonConfiguration(_integrationId, _integrationPassword);
            SetupMocks();
            var credentialsProvider = new MaskinportenCredentialsProvider("Test", MaskinportenClientMock.Object, conf);

            return credentialsProvider;
        }

        private void SetupMocks()
        {
            MaskinportenClientMock.Setup(_ => _.GetAccessToken(It.IsAny<string>()))
                                  .ReturnsAsync(new MaskinportenToken(_firstToken, _expiresIn));
        }

        private IntegrasjonConfiguration CreateIntegrationConfiguration()
        {
            return new IntegrasjonConfiguration(_integrationId, _integrationPassword);
        }

        public MaskinportenCredentialsProviderFixture WithMaskinportenToken(string token)
        {
            _firstToken = token;
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
    }
}