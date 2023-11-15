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
        private bool ampqKeepAlive = true;
        private int ampqKeepAliveHealthCheckInterval = AmqpConfiguration.DefaultKeepAliveHealthCheckInterval;
        private string amqpApplicationName = string.Empty;
        private ushort amqpPrefetchCount = 10;
        private string amqpHost = string.Empty;
        private int amqpPort = AmqpConfiguration.DefaultPort;
        private string apiHost = string.Empty;
        private int apiPort = ApiConfiguration.DefaultPort;
        private string apiScheme = ApiConfiguration.DefaultScheme;
        private string maskinportenAudience = string.Empty;
        private string maskinportenTokenEndpoint = string.Empty;
        private string maskinportenIssuer = string.Empty;
        private X509Certificate2 maskinportenCertificate;

        public static FiksIOConfigurationBuilder Init()
        {
            return new FiksIOConfigurationBuilder();
        }

        public FiksIOConfiguration BuildTestConfiguration()
        {
            ValidateMinimumConfigurations();

            if (string.IsNullOrEmpty(amqpHost))
            {
                amqpHost = AmqpConfiguration.TestHost;
            }

            if (string.IsNullOrEmpty(apiHost))
            {
                apiHost = ApiConfiguration.TestHost;
            }

            if (string.IsNullOrEmpty(maskinportenAudience))
            {
                maskinportenAudience = FiksIOConfiguration.maskinportenTestAudience;
            }

            if (string.IsNullOrEmpty(maskinportenTokenEndpoint))
            {
                maskinportenTokenEndpoint = FiksIOConfiguration.maskinportenTestTokenEndpoint;
            }

            return new FiksIOConfiguration(
                amqpConfiguration: new AmqpConfiguration(amqpHost, amqpPort, applicationName: amqpApplicationName, prefetchCount: amqpPrefetchCount, keepAlive: ampqKeepAlive, keepAliveCheckInterval: ampqKeepAliveHealthCheckInterval),
                apiConfiguration: new ApiConfiguration(apiScheme, apiHost, apiPort),
                asiceSigningConfiguration: _asiceSigningConfiguration,
                integrasjonConfiguration: _integrasjonConfiguration,
                kontoConfiguration: _kontoConfiguration,
                maskinportenConfiguration: FiksIOConfiguration.CreateMaskinportenConfig(maskinportenAudience, maskinportenTokenEndpoint, maskinportenIssuer, maskinportenCertificate));
        }

        public FiksIOConfiguration BuildProdConfiguration()
        {
            ValidateMinimumConfigurations();

            if (string.IsNullOrEmpty(amqpHost))
            {
                amqpHost = AmqpConfiguration.ProdHost;
            }

            if (string.IsNullOrEmpty(apiHost))
            {
                apiHost = ApiConfiguration.ProdHost;
            }

            if (string.IsNullOrEmpty(maskinportenAudience))
            {
                maskinportenAudience = FiksIOConfiguration.maskinportenProdAudience;
            }

            if (string.IsNullOrEmpty(maskinportenTokenEndpoint))
            {
                maskinportenTokenEndpoint = FiksIOConfiguration.maskinportenProdTokenEndpoint;
            }

            return new FiksIOConfiguration(
                amqpConfiguration: new AmqpConfiguration(amqpHost, amqpPort, applicationName: amqpApplicationName, prefetchCount: amqpPrefetchCount, keepAlive: ampqKeepAlive),
                apiConfiguration: new ApiConfiguration(apiScheme, apiHost, apiPort),
                asiceSigningConfiguration: _asiceSigningConfiguration,
                integrasjonConfiguration: _integrasjonConfiguration,
                kontoConfiguration: _kontoConfiguration,
                maskinportenConfiguration: FiksIOConfiguration.CreateMaskinportenConfig(maskinportenAudience, maskinportenTokenEndpoint, maskinportenIssuer, maskinportenCertificate));
        }

        public FiksIOConfiguration BuildConfiguration()
        {
            ValidateMinimumConfigurations();
            ValidateExtensiveConfiguration();

            return new FiksIOConfiguration(
                amqpConfiguration: new AmqpConfiguration(amqpHost, amqpPort, applicationName: amqpApplicationName, prefetchCount: amqpPrefetchCount, keepAlive: ampqKeepAlive),
                apiConfiguration: new ApiConfiguration(apiScheme, apiHost, apiPort),
                asiceSigningConfiguration: _asiceSigningConfiguration,
                integrasjonConfiguration: _integrasjonConfiguration,
                kontoConfiguration: _kontoConfiguration,
                maskinportenConfiguration: FiksIOConfiguration.CreateMaskinportenConfig(maskinportenAudience, maskinportenTokenEndpoint, maskinportenIssuer, maskinportenCertificate));
        }

        public FiksIOConfigurationBuilder WithMaskinportenConfiguration(X509Certificate2 certificate, string issuer, string audience = null, string tokenEndpoint = null)
        {
            maskinportenIssuer = issuer;
            maskinportenCertificate = certificate;
            maskinportenAudience = audience;
            maskinportenTokenEndpoint = tokenEndpoint;
            return this;
        }

        public FiksIOConfigurationBuilder WithApiConfiguration(string host, string scheme = ApiConfiguration.DefaultScheme, int port = ApiConfiguration.DefaultPort)
        {
            apiHost = host;
            apiScheme = scheme;
            apiPort = port;
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

        public FiksIOConfigurationBuilder WithAmqpConfiguration(string applicationName, ushort prefetchCount, bool keepAlive = true, int keepAliveHealthCheckInterval = AmqpConfiguration.DefaultKeepAliveHealthCheckInterval, string host = null, int port = AmqpConfiguration.DefaultPort )
        {
            ampqKeepAlive = keepAlive;
            amqpApplicationName = applicationName;
            amqpPrefetchCount = prefetchCount;
            ampqKeepAliveHealthCheckInterval = keepAliveHealthCheckInterval;
            amqpHost = host;
            amqpPort = port;
            return this;
        }

        private void ValidateMinimumConfigurations()
        {
            if (string.IsNullOrEmpty(maskinportenIssuer) || maskinportenCertificate == null)
            {
                throw new ArgumentException(
                    "MaskinportenConfiguration not correct. Have you called the WithMaskinportenConfiguration( ... ) in this builder?");
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

        private void ValidateExtensiveConfiguration()
        {
            if (string.IsNullOrEmpty(amqpHost))
            {
                throw new ArgumentException(
                    "Amqp setting 'amqpHost' not set. Have you called the WithAmqpConfiguration( ... ) and set 'amqpHost' in this builder?");
            }

            if (string.IsNullOrEmpty(apiHost))
            {
                throw new ArgumentException(
                    "API setting 'apiHost' not set. Have you called the WithApiConfiguration( ... ) in this builder?");
            }

            if (string.IsNullOrEmpty(maskinportenAudience))
            {
                throw new ArgumentException(
                    "Maskinporten setting 'maskinportenAudience' not set. Have you called the WithMaskinportenConfiguration( ... ) and set 'audience' in this builder?");
            }

            if (string.IsNullOrEmpty(maskinportenTokenEndpoint))
            {
                throw new ArgumentException(
                    "Maskinporten setting 'maskinportenTokenEndpoint' not set. Have you called the WithMaskinportenConfiguration( ... ) and set 'tokenEndpoint' in this builder?");
            }
        }
    }
}