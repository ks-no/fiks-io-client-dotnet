using System;
using System.Security.Cryptography.X509Certificates;
using KS.Fiks.IO.Send.Client.Configuration;
using Ks.Fiks.Maskinporten.Client;

namespace KS.Fiks.IO.Client.Configuration
{
    public class FiksIOConfiguration
    {
        public FiksIOConfiguration(
            KontoConfiguration kontoConfiguration,
            IntegrasjonConfiguration integrasjonConfiguration,
            MaskinportenClientConfiguration maskinportenConfiguration,
            ApiConfiguration apiConfiguration = null,
            AmqpConfiguration amqpConfiguration = null,
            KatalogConfiguration katalogConfiguration = null,
            FiksIOSenderConfiguration fiksIOSenderConfiguration = null,
            DokumentlagerConfiguration dokumentlagerConfiguration = null)
        {
            KontoConfiguration = kontoConfiguration;
            IntegrasjonConfiguration = integrasjonConfiguration;
            MaskinportenConfiguration = maskinportenConfiguration;
            ApiConfiguration = apiConfiguration ?? new ApiConfiguration();
            AmqpConfiguration = amqpConfiguration ?? new AmqpConfiguration(ApiConfiguration.Host);
            KatalogConfiguration = katalogConfiguration ?? new KatalogConfiguration(ApiConfiguration);
            DokumentlagerConfiguration = dokumentlagerConfiguration ?? new DokumentlagerConfiguration(ApiConfiguration);
            FiksIOSenderConfiguration = fiksIOSenderConfiguration ?? new FiksIOSenderConfiguration(
                                            null,
                                            ApiConfiguration.Scheme,
                                            ApiConfiguration.Host,
                                            ApiConfiguration.Port);
        }

        public KontoConfiguration KontoConfiguration { get; }

        public AmqpConfiguration AmqpConfiguration { get; }

        public KatalogConfiguration KatalogConfiguration { get; }

        public ApiConfiguration ApiConfiguration { get; }

        public IntegrasjonConfiguration IntegrasjonConfiguration { get; }

        public FiksIOSenderConfiguration FiksIOSenderConfiguration { get; }

        public MaskinportenClientConfiguration MaskinportenConfiguration { get; }

        public DokumentlagerConfiguration DokumentlagerConfiguration { get; }

        public static FiksIOConfiguration CreateProdConfiguration(
            Guid integrasjonId,
            string integrasjonPassord,
            Guid kontoId,
            string privatNokkel,
            string issuer,
            X509Certificate2 certificate,
            bool keepAlive = false)
        {
            return new FiksIOConfiguration(
                amqpConfiguration: AmqpConfiguration.CreateProdConfiguration(keepAlive),
                apiConfiguration: ApiConfiguration.CreateProdConfiguration(),
                integrasjonConfiguration: new IntegrasjonConfiguration(integrasjonId, integrasjonPassord),
                kontoConfiguration: new KontoConfiguration(kontoId, privatNokkel),
                maskinportenConfiguration: CreateMaskinportenProdConfig(issuer, certificate));
        }

        public static FiksIOConfiguration CreateTestConfiguration(
            Guid integrasjonId,
            string integrasjonPassord,
            Guid kontoId,
            string privatNokkel,
            string issuer,
            X509Certificate2 certificate,
            bool keepAlive = false)
        {
            return new FiksIOConfiguration(
                amqpConfiguration: AmqpConfiguration.CreateTestConfiguration(keepAlive),
                apiConfiguration: ApiConfiguration.CreateTestConfiguration(),
                integrasjonConfiguration: new IntegrasjonConfiguration(integrasjonId, integrasjonPassord),
                kontoConfiguration: new KontoConfiguration(kontoId, privatNokkel),
                maskinportenConfiguration: CreateMaskinportenTestConfig(issuer, certificate));
        }

        private static MaskinportenClientConfiguration CreateMaskinportenProdConfig(string issuer, X509Certificate2 certificate)
        {
            return new MaskinportenClientConfiguration(
                audience: @"https://oidc.difi.no/idporten-oidc-provider/token",
                tokenEndpoint: @"https://oidc.difi.no/idporten-oidc-provider/token",
                issuer: issuer, // KS issuer name
                numberOfSecondsLeftBeforeExpire: 10,
                certificate: certificate);
        }

        private static MaskinportenClientConfiguration CreateMaskinportenTestConfig(string issuer, X509Certificate2 certificate)
        {
            return new MaskinportenClientConfiguration(
                audience: @"https://oidc-ver2.difi.no/idporten-oidc-provider/",
                tokenEndpoint: @"https://oidc-ver2.difi.no/idporten-oidc-provider/token",
                issuer: issuer, // KS issuer name
                numberOfSecondsLeftBeforeExpire: 10,
                certificate: certificate);
        }
    }
}