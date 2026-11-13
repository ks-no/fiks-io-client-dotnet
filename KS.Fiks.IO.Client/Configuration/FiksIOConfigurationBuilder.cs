using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using KS.Fiks.IO.Crypto.Configuration;
using KS.Fiks.IO.Send.Client.Configuration;
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
        private RSA maskinportenPrivateKey;
        private string maskinportenKeyIdentifier;

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
                maskinportenConfiguration: CreateMaskinportenTestConfiguration());
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
                maskinportenConfiguration: CreateMaskinportenTestConfiguration());
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
                maskinportenConfiguration: CreateMaskinportenProdConfiguration());
        }

        public FiksIOConfigurationBuilder WithMaskinportenConfiguration(X509Certificate2 certificate, string issuer)
        {
            maskinportenIssuer = issuer;
            maskinportenCertificate = certificate;
            return this;
        }

        public FiksIOConfigurationBuilder WithMaskinportenConfiguration(X509Certificate2 certificate, string issuer, string keyIdentifier)
        {
            maskinportenIssuer = issuer;
            maskinportenCertificate = certificate;
            maskinportenKeyIdentifier = keyIdentifier;
            return this;
        }

        /*
         * Maskinporten with your own RSA key pair instead of a certificate. The keyIdentifier (kid) is required.
         * Cannot be combined with WithMaskinportenConfiguration( ... ).
         * The caller owns the RSA object. It is not disposed by the builder or the client, and it must be kept alive
         * as long as the FiksIOClient is in use. Otherwise token renewal fails with an ObjectDisposedException.
         */
        public FiksIOConfigurationBuilder WithMaskinportenKeyPairConfiguration(RSA privateKey, string issuer, string keyIdentifier)
        {
            maskinportenIssuer = issuer;
            maskinportenPrivateKey = privateKey;
            maskinportenKeyIdentifier = keyIdentifier;
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

        private MaskinportenClientConfiguration CreateMaskinportenTestConfiguration()
        {
            if (maskinportenPrivateKey == null)
            {
                return FiksIOConfiguration.CreateMaskinportenTestConfig(maskinportenIssuer, maskinportenCertificate, maskinportenKeyIdentifier);
            }

            return CreateMaskinportenKeyPairConfiguration(FiksIOConfiguration.maskinportenTestAudience, FiksIOConfiguration.maskinportenTestTokenEndpoint);
        }

        private MaskinportenClientConfiguration CreateMaskinportenProdConfiguration()
        {
            if (maskinportenPrivateKey == null)
            {
                return FiksIOConfiguration.CreateMaskinportenProdConfig(maskinportenIssuer, maskinportenCertificate, maskinportenKeyIdentifier);
            }

            return CreateMaskinportenKeyPairConfiguration(FiksIOConfiguration.maskinportenProdAudience, FiksIOConfiguration.maskinportenProdTokenEndpoint);
        }

        private MaskinportenClientConfiguration CreateMaskinportenKeyPairConfiguration(string audience, string tokenEndpoint)
        {
            return new MaskinportenClientConfiguration(
                audience: audience,
                tokenEndpoint: tokenEndpoint,
                issuer: maskinportenIssuer,
                numberOfSecondsLeftBeforeExpire: 10,
                privateKey: maskinportenPrivateKey,
                publicKey: maskinportenPrivateKey,
                keyIdentifier: maskinportenKeyIdentifier);
        }

        private void ValidateConfigurations()
        {
            if (maskinportenCertificate != null && maskinportenPrivateKey != null)
            {
                throw new ArgumentException(
                    "MaskinportenConfiguration has both a certificate and a key pair. Use either WithMaskinportenConfiguration( ... ) or WithMaskinportenKeyPairConfiguration( ... ), not both.");
            }

            if (string.IsNullOrEmpty(maskinportenIssuer) || (maskinportenCertificate == null && maskinportenPrivateKey == null))
            {
                throw new ArgumentException(
                    "MaskinportenConfiguration missing. Have you called the WithMaskinportenConfiguration( ... ) or WithMaskinportenKeyPairConfiguration( ... ) in this builder?");
            }

            if (maskinportenPrivateKey != null && string.IsNullOrWhiteSpace(maskinportenKeyIdentifier))
            {
                throw new ArgumentException(
                    "keyIdentifier missing. The keyIdentifier (kid) is required when using WithMaskinportenKeyPairConfiguration( ... ).");
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