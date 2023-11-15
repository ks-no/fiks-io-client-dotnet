using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using KS.Fiks.IO.Client.Configuration;
using Ks.Fiks.Maskinporten.Client;

namespace ExampleApplication.FiksIO
{
    public static class FiksIoConfigurationBuilder
    {
        
        /* Create a configuration for the test environment with the fluent builder.
         * API, AMQP and maskinporten host urls and endpoints for test-environment are set by the client.
         * Use this for easy setup in a test-environment.
         */
        public static FiksIOConfiguration CreateTestConfiguration(AppSettings appSettings)
        {
            var accountId = appSettings.FiksIOConfig.FiksIoAccountId;
            var privateKeyPath = appSettings.FiksIOConfig.FiksIoPrivateKey;
            var integrationId = appSettings.FiksIOConfig.FiksIoIntegrationId; 
            var integrationPassword = appSettings.FiksIOConfig.FiksIoIntegrationPassword;
            var issuer = appSettings.FiksIOConfig.MaskinPortenIssuer;
            var certPath = appSettings.FiksIOConfig.MaskinPortenCompanyCertificatePath;
            var certPassword = appSettings.FiksIOConfig.MaskinPortenCompanyCertificatePassword;
            var asiceSigningPublicKey = appSettings.FiksIOConfig.AsiceSigningPublicKey;
            var asiceSigningPrivateKey = appSettings.FiksIOConfig.AsiceSigningPrivateKey;

            return FiksIOConfigurationBuilder
                .Init()
                .WithAmqpConfiguration("fiks-io-client-dotnet-example-application", 1, true,20 * 1000)
                .WithMaskinportenConfiguration(new X509Certificate2(certPath, certPassword), issuer)
                .WithFiksIntegrasjonConfiguration(integrationId, integrationPassword)
                .WithFiksKontoConfiguration(accountId, ReadFromFile(privateKeyPath))
                .WithAsiceSigningConfiguration(asiceSigningPublicKey, asiceSigningPrivateKey)
                .BuildTestConfiguration();
        }

        /* Create a configuration for any environment with the fluent builder. All required settings must be set from app settings.
         * API, AMQP and maskinporten settings must be set by you.
         * Use this for configuration that can be used in any environment and the environment holds the settings. 
         */

        public static FiksIOConfiguration CreateConfiguration(AppSettings appSettings)
        {
            var accountId = appSettings.FiksIOConfig.FiksIoAccountId;
            var privateKeyPath = appSettings.FiksIOConfig.FiksIoPrivateKey;
            var integrationId = appSettings.FiksIOConfig.FiksIoIntegrationId; 
            var integrationPassword = appSettings.FiksIOConfig.FiksIoIntegrationPassword;
            var issuer = appSettings.FiksIOConfig.MaskinPortenIssuer;
            var certPath = appSettings.FiksIOConfig.MaskinPortenCompanyCertificatePath;
            var certPassword = appSettings.FiksIOConfig.MaskinPortenCompanyCertificatePassword;
            var asiceSigningPublicKey = appSettings.FiksIOConfig.AsiceSigningPublicKey;
            var asiceSigningPrivateKey = appSettings.FiksIOConfig.AsiceSigningPrivateKey;
            var apiHost = appSettings.FiksIOConfig.ApiHost;
            var apiScheme = appSettings.FiksIOConfig.ApiScheme;
            var apiPort = appSettings.FiksIOConfig.ApiPort;
            var amqpHost = appSettings.FiksIOConfig.AmqpHost;
            var amqpPort = appSettings.FiksIOConfig.AmqpPort;
            var maskinportenAudience = appSettings.FiksIOConfig.MaskinPortenAudienceUrl;
            var maskinportenTokenEndpoint = appSettings.FiksIOConfig.MaskinPortenTokenUrl;
                

            return FiksIOConfigurationBuilder
                .Init()
                .WithAmqpConfiguration("fiks-io-client-dotnet-example-application", 1, true,20 * 1000, amqpHost, amqpPort)
                .WithMaskinportenConfiguration(new X509Certificate2(certPath, certPassword), issuer, maskinportenAudience, maskinportenTokenEndpoint)
                .WithFiksIntegrasjonConfiguration(integrationId, integrationPassword)
                .WithFiksKontoConfiguration(accountId, ReadFromFile(privateKeyPath))
                .WithAsiceSigningConfiguration(asiceSigningPublicKey, asiceSigningPrivateKey)
                .WithApiConfiguration(apiHost, apiScheme, apiPort)
                .BuildConfiguration();
        }

        // Create a FiksIOConfiguration manually. 
        public static FiksIOConfiguration CreateConfig(string issuer, string p12Filename, string p12Password, string fiksIoAccountId,
            string fiksIoPrivateKeyPath, string integrasjonId, string integrasjonPassword)
        {
            // ID-porten machine to machine configuration
            var maskinportenConfig = new MaskinportenClientConfiguration(
                audience: @"https://test.maskinporten.no/", // maskinporten audience path
                tokenEndpoint: @"https://test.maskinporten.no/token", // maskinporten token path
                issuer: issuer, // KS issuer name
                numberOfSecondsLeftBeforeExpire: 10, // The token will be refreshed 10 seconds before it expires
                certificate: new X509Certificate2(p12Filename, p12Password));

            // Fiks IO account configuration
            var kontoConfig = new KontoConfiguration(
                kontoId: Guid.Parse(fiksIoAccountId) /* Fiks IO accountId as Guid */,
                privatNokkel: ReadFromFile(
                    fiksIoPrivateKeyPath) /* Private key in PEM format, paired with the public key supplied to Fiks IO account */);


            // Id and password for integration associated to the Fiks IO account.
            var integrasjonConfig = new IntegrasjonConfiguration(
                Guid.Parse(integrasjonId) /* Integration id as Guid */,
                integrasjonPassword /* Integration password */);

            var asiceSigningConfig = new AsiceSigningConfiguration(new X509Certificate2(p12Filename, p12Password));


            // Optional: Use custom api host (i.e. for connecting to test api)
            var apiConfig = new ApiConfiguration(
                scheme: "https",
                host: "api.fiks.test.ks.no",
                port: 443);

            // Optional: Use custom amqp host (i.e. for connection to test queue)
            var amqpConfig = new AmqpConfiguration(
                host: "io.fiks.test.ks.no",
                port: 5671);

            // Combine all configurations
            return new FiksIOConfiguration(kontoConfig, integrasjonConfig, maskinportenConfig, asiceSigningConfig, apiConfig, amqpConfig);
        }

        private static string ReadFromFile(string path)
        {
            return File.ReadAllText(path, Encoding.UTF8);
        }
    }
}