using System;
using System.Security.Cryptography.X509Certificates;

namespace KS.Fiks.IO.Client.Configuration
{
    public class FiksIOConfigurationBuilder
    {
        private FiksIOConfiguration _fiksIoConfiguration;
        private AmqpConfiguration _amqpConfiguration;
        private IntegrasjonConfiguration _integrasjonConfiguration;
        private KontoConfiguration _kontoConfiguration;
        private AsiceSigningConfiguration _asiceSigningConfiguration = null;
        private bool ampqKeepAlive = false;
        private string amqpApplicationName = string.Empty;
        private ushort amqpPrefetchCount = 10;
        private string maskinportenIssuer = string.Empty;
        private X509Certificate2 maskinportenCertificate;

        public static FiksIOConfigurationBuilder Init()
        {
            return new FiksIOConfigurationBuilder();
        }

        public FiksIOConfiguration BuildTestConfiguration()
        {
            ValidateConfigurations();

            return new FiksIOConfiguration(
                amqpConfiguration: new AmqpConfiguration(AmqpConfiguration.TestHost, applicationName: amqpApplicationName, prefetchCount: amqpPrefetchCount, keepAlive: ampqKeepAlive),
                apiConfiguration: ApiConfiguration.CreateTestConfiguration(),
                asiceSigningConfiguration: _asiceSigningConfiguration,
                integrasjonConfiguration: _integrasjonConfiguration,
                kontoConfiguration: _kontoConfiguration,
                maskinportenConfiguration: FiksIOConfiguration.CreateMaskinportenTestConfig(maskinportenIssuer, maskinportenCertificate));
        }

        public FiksIOConfiguration BuildProdConfiguration()
        {
            ValidateConfigurations();

            return new FiksIOConfiguration(
                amqpConfiguration: new AmqpConfiguration(AmqpConfiguration.ProdHost, applicationName: amqpApplicationName, prefetchCount: amqpPrefetchCount, keepAlive: ampqKeepAlive),
                apiConfiguration: ApiConfiguration.CreateProdConfiguration(),
                asiceSigningConfiguration: _asiceSigningConfiguration,
                integrasjonConfiguration: _integrasjonConfiguration,
                kontoConfiguration: _kontoConfiguration,
                maskinportenConfiguration: FiksIOConfiguration.CreateMaskinportenTestConfig(maskinportenIssuer, maskinportenCertificate));
        }

        public FiksIOConfigurationBuilder WithMaskinportenConfiguration(X509Certificate2 certificate, string issuer)
        {
            maskinportenIssuer = issuer;
            maskinportenCertificate = certificate;
            return this;
        }

        public FiksIOConfigurationBuilder WithAsiceSigningConfiguration(string certificatePath, string certificatePrivateKeyPath)
        {
            _asiceSigningConfiguration = new AsiceSigningConfiguration(certificatePath, certificatePrivateKeyPath);
            return this;
        }

        public FiksIOConfigurationBuilder WithFiksIntegrasjonConfiguration(Guid fiksIntegrasjonId, string fiksIntegrasjonPassword)
        {
            _integrasjonConfiguration = new IntegrasjonConfiguration(fiksIntegrasjonId, fiksIntegrasjonPassword);
            return this;
        }

        public FiksIOConfigurationBuilder WithFiksKontoConfiguration(Guid fiksKontoId, string fiksPrivateKey) 
        {
            _kontoConfiguration = new KontoConfiguration(fiksKontoId, fiksPrivateKey);
            return this;
        }

        public FiksIOConfigurationBuilder WithAmqpConfiguration(string applicationName, ushort prefetchCount, bool keepAlive = false)
        {
            ampqKeepAlive = keepAlive;
            amqpApplicationName = applicationName;
            amqpPrefetchCount = prefetchCount;
            return this;
        }

        private void ValidateConfigurations()
        {
            if (string.IsNullOrEmpty(maskinportenIssuer) || maskinportenCertificate == null)
            {
                throw new ArgumentException(
                    "MaskinportenConfiguration missing. Have you called the WithMaskinportenConfiguration( ... ) in this builder?");
            }

            if (_integrasjonConfiguration == null)
            {
                throw new ArgumentException(
                    "FiksIntegrasjonConfiguration missing. Have you called the WithFiksIntegrasjonConfiguration( ... ) in this builder?");
            }

            if (_kontoConfiguration == null)
            {
                throw new ArgumentException(
                    "FiksKontoConfiguration missing. Have you called the WithFiksKontoConfiguration( ... ) in this builder?");
            }
        }
    }
}