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
        
        // Create configuration with the fluent builder
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
            var amqpHost = appSettings.FiksIOConfig.AmqpHost;
            var amqpPort = appSettings.FiksIOConfig.AmqpPort;
            var apiHost = appSettings.FiksIOConfig.ApiHost;
            var apiPort = appSettings.FiksIOConfig.ApiPort;
                

            return FiksIOConfigurationBuilder
                .Init()
                .WithAmqpConfiguration("fiks-io-client-dotnet-example-application", 1, true,20 * 1000)
                .WithMaskinportenConfiguration(new X509Certificate2(certPath, certPassword), issuer)
                .WithFiksIntegrasjonConfiguration(integrationId, integrationPassword)
                .WithFiksKontoConfiguration(accountId, ReadFromFile(privateKeyPath))
                .WithAsiceSigningConfiguration(asiceSigningPublicKey, asiceSigningPrivateKey)
                .BuildConfiguration(amqpHost, apiHost, amqpPort, apiPort);
        }
        
        // Create a FiksIOConfiguration manually
        public static FiksIOConfiguration CreateConfig(string issuer, string p12Filename, string p12Password, string fiksIoAccountId,
            string fiksIoPrivateKeyPath, string integrasjonId, string integrasjonPassword)
        {
            // ID-porten machine to machine configuration
            var maskinportenConfig = new MaskinportenClientConfiguration(
                audience: @"https://ver2.maskinporten.no/", // ID-porten audience path
                tokenEndpoint: @"https://ver2.maskinporten.no/token", // ID-porten token path
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