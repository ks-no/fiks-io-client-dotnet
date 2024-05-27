using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Ks.Fiks.Maskinporten.Client;

namespace KS.Fiks.IO.Client.Configuration
{
    public class FiksIOConfigurationBuilder
    {
        private FiksIOConfiguration _fiksIoConfiguration;
        private AmqpConfiguration _amqpConfiguration;
        private IntegrasjonConfiguration _integrasjonConfiguration;
        private KontoConfiguration _kontoConfiguration;
        private AsiceSigningConfiguration _asiceSigningConfiguration;
        private MaskinportenClientConfiguration _maskinportenClientConfiguration;
        private ApiConfiguration _apiConfiguration;
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
                amqpConfiguration: new AmqpConfiguration(AmqpConfiguration.TestHost, applicationName: amqpApplicationName, prefetchCount: amqpPrefetchCount),
                apiConfiguration: ApiConfiguration.CreateTestConfiguration(),
                asiceSigningConfiguration: _asiceSigningConfiguration,
                integrasjonConfiguration: _integrasjonConfiguration,
                kontoConfiguration: _kontoConfiguration,
                maskinportenConfiguration: FiksIOConfiguration.CreateMaskinportenTestConfig(maskinportenIssuer, maskinportenCertificate));
        }

        public FiksIOConfiguration BuildDevConfiguration(string amqpHost, string apiHost)
        {
            ValidateConfigurations();

            return new FiksIOConfiguration(
                amqpConfiguration: new AmqpConfiguration(amqpHost, applicationName: amqpApplicationName, prefetchCount: amqpPrefetchCount),
                apiConfiguration: ApiConfiguration.CreateDevConfiguration(apiHost),
                asiceSigningConfiguration: _asiceSigningConfiguration,
                integrasjonConfiguration: _integrasjonConfiguration,
                kontoConfiguration: _kontoConfiguration,
                maskinportenConfiguration: FiksIOConfiguration.CreateMaskinportenTestConfig(maskinportenIssuer, maskinportenCertificate));
        }

        public FiksIOConfiguration BuildProdConfiguration()
        {
            ValidateConfigurations();

            return new FiksIOConfiguration(
                amqpConfiguration: new AmqpConfiguration(AmqpConfiguration.ProdHost, applicationName: amqpApplicationName, prefetchCount: amqpPrefetchCount),
                apiConfiguration: ApiConfiguration.CreateProdConfiguration(),
                asiceSigningConfiguration: _asiceSigningConfiguration,
                integrasjonConfiguration: _integrasjonConfiguration,
                kontoConfiguration: _kontoConfiguration,
                maskinportenConfiguration: FiksIOConfiguration.CreateMaskinportenProdConfig(maskinportenIssuer, maskinportenCertificate));
        }

        public FiksIOConfigurationBuilder WithMaskinportenConfiguration(X509Certificate2 certificate, string issuer)
        {
            maskinportenIssuer = issuer;
            maskinportenCertificate = certificate;
            return this;
        }

        /*
         * AsiceSigning with public/private key pair
         */
        public FiksIOConfigurationBuilder WithAsiceSigningConfiguration(string publicKeyPath, string privateKeyPath)
        {
            _asiceSigningConfiguration = new AsiceSigningConfiguration(publicKeyPath, privateKeyPath);
            return this;
        }

        /*
         * AsiceSigning with a X509Certificate2 that must contain a matching privatekey. This can be the same as used for Maskinporten
         */
        public FiksIOConfigurationBuilder WithAsiceSigningConfiguration(X509Certificate2 x509Certificate2)
        {
            _asiceSigningConfiguration = new AsiceSigningConfiguration(x509Certificate2);
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

        public FiksIOConfigurationBuilder WithFiksKontoConfiguration(Guid fiksKontoId, IEnumerable<string> fiksPrivateKeys)
        {
            _kontoConfiguration = new KontoConfiguration(fiksKontoId, fiksPrivateKeys);
            return this;
        }

        public FiksIOConfigurationBuilder WithAmqpConfiguration(string applicationName, ushort prefetchCount)
        {
            amqpApplicationName = applicationName;
            amqpPrefetchCount = prefetchCount;
            return this;
        }

        public FiksIOConfigurationBuilder WithApiConfiguration(string hostName, int hostPort)
        {
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

            if (_asiceSigningConfiguration == null)
            {
                throw new ArgumentException(
                    "AsiceSigningConfiguration missing. Have you called the WithAsiceSigningConfiguration( ... ) in this builder?");
            }
        }
    }
}