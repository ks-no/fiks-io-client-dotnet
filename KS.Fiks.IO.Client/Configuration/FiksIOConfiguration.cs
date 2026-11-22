using System;
using System.Security.Cryptography.X509Certificates;
using KS.Fiks.IO.Crypto.Configuration;
using KS.Fiks.IO.Send.Client.Configuration;
using Ks.Fiks.Maskinporten.Client;

namespace KS.Fiks.IO.Client.Configuration
{
    public class FiksIOConfiguration
    {
        public const string maskinportenProdAudience = @"https://maskinporten.no/";
        public const string maskinportenProdTokenEndpoint = @"https://maskinporten.no/token";
        public const string maskinportenTestAudience = @"https://test.maskinporten.no/";
        public const string maskinportenTestTokenEndpoint = @"https://test.maskinporten.no/token";

        public KontoConfiguration KontoConfiguration { get; }

        public AmqpConfiguration AmqpConfiguration { get; }

        public KatalogConfiguration KatalogConfiguration { get; }

        public ApiConfiguration ApiConfiguration { get; }

        public IntegrasjonConfiguration IntegrasjonConfiguration { get; }

        public FiksIOConfiguration(
            KontoConfiguration kontoConfiguration,
            IntegrasjonConfiguration integrasjonConfiguration,
            MaskinportenClientConfiguration maskinportenConfiguration,
            AsiceSigningConfiguration asiceSigningConfiguration,
            ApiConfiguration apiConfiguration = null,
            AmqpConfiguration amqpConfiguration = null,
            KatalogConfiguration katalogConfiguration = null,
            FiksIOSenderConfiguration fiksIOSenderConfiguration = null,
            DokumentlagerConfiguration dokumentlagerConfiguration = null
            )
        {
            KontoConfiguration = kontoConfiguration;
            IntegrasjonConfiguration = integrasjonConfiguration;
            MaskinportenConfiguration = maskinportenConfiguration;
            AsiceSigningConfiguration = asiceSigningConfiguration;
            ApiConfiguration = apiConfiguration ?? new ApiConfiguration();
            AmqpConfiguration = amqpConfiguration ?? CreateDefaultAmqpConfiguration(ApiConfiguration);
            KatalogConfiguration = katalogConfiguration ?? new KatalogConfiguration(ApiConfiguration);
            DokumentlagerConfiguration = dokumentlagerConfiguration ?? new DokumentlagerConfiguration(ApiConfiguration);
            FiksIOSenderConfiguration = fiksIOSenderConfiguration ?? new FiksIOSenderConfiguration(
                null,
                ApiConfiguration.Scheme,
                ApiConfiguration.Host,
                ApiConfiguration.Port);
        }

        public FiksIOConfiguration(
            KontoConfiguration kontoConfiguration,
            IntegrasjonConfiguration integrasjonConfiguration,
            AsiceSigningConfiguration asiceSigningConfiguration,
            ApiConfiguration apiConfiguration = null,
            AmqpConfiguration amqpConfiguration = null,
            KatalogConfiguration katalogConfiguration = null,
            FiksIOSenderConfiguration fiksIOSenderConfiguration = null,
            DokumentlagerConfiguration dokumentlagerConfiguration = null
        )
        {
            KontoConfiguration = kontoConfiguration;
            IntegrasjonConfiguration = integrasjonConfiguration;
            AsiceSigningConfiguration = asiceSigningConfiguration;
            ApiConfiguration = apiConfiguration ?? new ApiConfiguration();
            AmqpConfiguration = amqpConfiguration ?? CreateDefaultAmqpConfiguration(ApiConfiguration);
            KatalogConfiguration = katalogConfiguration ?? new KatalogConfiguration(ApiConfiguration);
            DokumentlagerConfiguration = dokumentlagerConfiguration ?? new DokumentlagerConfiguration(ApiConfiguration);
            FiksIOSenderConfiguration = fiksIOSenderConfiguration ?? new FiksIOSenderConfiguration(
                null,
                ApiConfiguration.Scheme,
                ApiConfiguration.Host,
                ApiConfiguration.Port);
        }

        public FiksIOSenderConfiguration FiksIOSenderConfiguration { get; }

        public MaskinportenClientConfiguration MaskinportenConfiguration { get; }

        public DokumentlagerConfiguration DokumentlagerConfiguration { get; }

        public AsiceSigningConfiguration AsiceSigningConfiguration { get; }

        public static FiksIOConfiguration CreateProdConfiguration(
            Guid integrasjonId,
            string integrasjonPassord,
            Guid kontoId,
            string privatNokkel,
            string issuer,
            X509Certificate2 maskinportenSertifikat,
            X509Certificate2 asiceSertifikat,
            string applicationName = null,
            string keyIdentifier = null)
        {
            return new FiksIOConfiguration(
                amqpConfiguration: AmqpConfiguration.CreateProdConfiguration(applicationName),
                apiConfiguration: ApiConfiguration.CreateProdConfiguration(),
                integrasjonConfiguration: new IntegrasjonConfiguration(integrasjonId, integrasjonPassord),
                kontoConfiguration: new KontoConfiguration(kontoId, privatNokkel),
                maskinportenConfiguration: CreateMaskinportenProdConfig(issuer, maskinportenSertifikat, keyIdentifier),
                asiceSigningConfiguration: new AsiceSigningConfiguration(asiceSertifikat));
        }

        public static FiksIOConfiguration CreateTestConfiguration(
            Guid fiksIntegrasjonId,
            string fiksIntegrasjonPassord,
            Guid fiksKontoId,
            string privatNokkel,
            string issuer,
            X509Certificate2 maskinportenSertifikat,
            X509Certificate2 asiceSertifikat,
            string applicationName = null,
            string keyIdentifier = null)
        {
            return new FiksIOConfiguration(
                amqpConfiguration: AmqpConfiguration.CreateTestConfiguration(applicationName),
                apiConfiguration: ApiConfiguration.CreateTestConfiguration(),
                integrasjonConfiguration: new IntegrasjonConfiguration(fiksIntegrasjonId, fiksIntegrasjonPassord),
                kontoConfiguration: new KontoConfiguration(fiksKontoId, privatNokkel),
                maskinportenConfiguration: CreateMaskinportenTestConfig(issuer, maskinportenSertifikat, keyIdentifier),
                asiceSigningConfiguration: new AsiceSigningConfiguration(asiceSertifikat));
        }

        public static MaskinportenClientConfiguration CreateMaskinportenProdConfig(string issuer, X509Certificate2 certificate, string keyIdentifier = null)
        {
            return new MaskinportenClientConfiguration(
                audience: maskinportenProdAudience,
                tokenEndpoint: maskinportenProdTokenEndpoint,
                issuer: issuer, // KS issuer name
                numberOfSecondsLeftBeforeExpire: 10,
                certificate: certificate,
                keyIdentifier: keyIdentifier);
        }

        public static MaskinportenClientConfiguration CreateMaskinportenTestConfig(string issuer, X509Certificate2 certificate, string keyIdentifier = null)
        {
            return new MaskinportenClientConfiguration(
                audience: maskinportenTestAudience,
                tokenEndpoint: maskinportenTestTokenEndpoint,
                issuer: issuer, // KS issuer name
                numberOfSecondsLeftBeforeExpire: 10,
                certificate: certificate,
                keyIdentifier: keyIdentifier);
        }

        /*
         * Used when no AmqpConfiguration is given. The AMQP host is derived from the API host for the known
         * Fiks environments. For any other API host there is no known AMQP host, so the API host is kept as before.
         */
        private static AmqpConfiguration CreateDefaultAmqpConfiguration(ApiConfiguration apiConfiguration)
        {
            var apiHost = apiConfiguration.Host;

            if (string.Equals(apiHost, ApiConfiguration.ProdHost, StringComparison.OrdinalIgnoreCase))
            {
                return new AmqpConfiguration(AmqpConfiguration.ProdHost);
            }

            if (string.Equals(apiHost, ApiConfiguration.TestHost, StringComparison.OrdinalIgnoreCase))
            {
                return new AmqpConfiguration(AmqpConfiguration.TestHost);
            }

            return new AmqpConfiguration(apiHost);
        }
    }
}