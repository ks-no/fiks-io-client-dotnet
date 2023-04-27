using System;
using System.IO;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using KS.Fiks.IO.Client.Configuration;
using Ks.Fiks.Maskinporten.Client;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace ExampleApplication
{
    public static class FiksIOClientBuilder
    {
        public static FiksIOConfiguration CreateConfiguration(AppSettings appSettings)
        {
            var accountId = appSettings.FiksIOConfig.FiksIoAccountId;
            var privateKeyPath = appSettings.FiksIOConfig.FiksIoPrivateKey;
            var integrationId = appSettings.FiksIOConfig.FiksIoIntegrationId; 
            var integrationPassword = appSettings.FiksIOConfig.FiksIoIntegrationPassword;
            var scope = appSettings.FiksIOConfig.FiksIoIntegrationScope;
            var audience = appSettings.FiksIOConfig.MaskinPortenAudienceUrl;
            var tokenEndpoint = appSettings.FiksIOConfig.MaskinPortenTokenUrl;
            var issuer = appSettings.FiksIOConfig.MaskinPortenIssuer;
            var certPath = appSettings.FiksIOConfig.MaskinPortenCompanyCertificatePath;
            var certPassword = appSettings.FiksIOConfig.MaskinPortenCompanyCertificatePassword;
            var asiceSigningPublicKey = appSettings.FiksIOConfig.AsiceSigningPublicKey;
            var asiceSigningPrivateKey = appSettings.FiksIOConfig.AsiceSigningPrivateKey;
            var host = appSettings.FiksIOConfig.AmqpHost;
            var port = appSettings.FiksIOConfig.AmqpPort;
            
            var ignoreSSLError = Environment.GetEnvironmentVariable("AMQP_IGNORE_SSL_ERROR");

            return FiksIOConfigurationBuilder
                .Init()
                .WithAmqpConfiguration("fiks-io-client-dotnet-example-application", 1)
                .WithMaskinportenConfiguration(new X509Certificate2(certPath, certPassword), issuer)
                .WithFiksIntegrasjonConfiguration(integrationId, integrationPassword)
                .WithFiksKontoConfiguration(accountId, ReadFromFile(privateKeyPath))
                .WithAsiceSigningConfiguration(asiceSigningPublicKey, asiceSigningPrivateKey)
                .BuildConfiguration(host, port);

        }
        
        public static async Task<KS.Fiks.IO.Client.FiksIOClient> CreateFiksIoClient(AppSettings appSettings, ILoggerFactory loggerFactory)
        {
            var accountId = appSettings.FiksIOConfig.FiksIoAccountId;
            var privateKey = await File.ReadAllTextAsync(appSettings.FiksIOConfig.FiksIoPrivateKey);
            var integrationId = appSettings.FiksIOConfig.FiksIoIntegrationId; 
            var integrationPassword = appSettings.FiksIOConfig.FiksIoIntegrationPassword;
            var scope = appSettings.FiksIOConfig.FiksIoIntegrationScope;
            var audience = appSettings.FiksIOConfig.MaskinPortenAudienceUrl;
            var tokenEndpoint = appSettings.FiksIOConfig.MaskinPortenTokenUrl;
            var issuer = appSettings.FiksIOConfig.MaskinPortenIssuer;
            
            var ignoreSSLError = Environment.GetEnvironmentVariable("AMQP_IGNORE_SSL_ERROR");


            
            
            // Fiks IO account configuration
            var account = new KontoConfiguration(
                                accountId,
                                privateKey);

            // Id and password for integration associated to the Fiks IO account.
            var integration = new IntegrasjonConfiguration(
                                    integrationId,
                                    integrationPassword, scope);

            // ID-porten machine to machine configuration
            var maskinporten = new MaskinportenClientConfiguration(
                audience: audience,
                tokenEndpoint: tokenEndpoint,
                issuer: issuer,
                numberOfSecondsLeftBeforeExpire: 10,
                certificate: GetCertificate(appSettings));

            // Optional: Use custom api host (i.e. for connecting to test api)
            var api = new ApiConfiguration(
                scheme: appSettings.FiksIOConfig.ApiScheme,
                host: appSettings.FiksIOConfig.ApiHost,
                port: appSettings.FiksIOConfig.ApiPort);
            
            var sslOption1 = (!string.IsNullOrEmpty(ignoreSSLError) && ignoreSSLError == "true")
                ? new SslOption()
                {
                    Enabled = true,
                    ServerName = appSettings.FiksIOConfig.AmqpHost,
                    CertificateValidationCallback =
                        (RemoteCertificateValidationCallback) ((sender, certificate, chain, errors) => true)
                }
                : null;
                

            // Optional: Use custom amqp host (i.e. for connection to test queue)
            var amqp = new AmqpConfiguration(
                host: appSettings.FiksIOConfig.AmqpHost,
                port: appSettings.FiksIOConfig.AmqpPort,
                sslOption1, "Fiks-Arkiv simulator", 10, false);

            var asiceSigning = new AsiceSigningConfiguration(appSettings.FiksIOConfig.AsiceSigningPublicKey,
                appSettings.FiksIOConfig.AsiceSigningPrivateKey);

            // Combine all configurations
            var configuration = new FiksIOConfiguration(account, integration, maskinporten, asiceSigning, api, amqp);
            return await KS.Fiks.IO.Client.FiksIOClient.CreateAsync(configuration, loggerFactory);
        }
        
        private static X509Certificate2 GetCertificate(AppSettings appSettings)
        {
            if (!string.IsNullOrEmpty(appSettings.FiksIOConfig.MaskinPortenCompanyCertificatePath))
            {
                return new X509Certificate2(File.ReadAllBytes(appSettings.FiksIOConfig.MaskinPortenCompanyCertificatePath), appSettings.FiksIOConfig.MaskinPortenCompanyCertificatePassword);
            }
           
            var store = new X509Store(StoreLocation.CurrentUser);

            store.Open(OpenFlags.ReadOnly);

            var certificates = store.Certificates.Find(X509FindType.FindByThumbprint, appSettings.FiksIOConfig.MaskinPortenCompanyCertificateThumbprint, false);

            store.Close();

            return certificates.Count > 0 ? certificates[0] : null;
        }
        
        
        private static string ReadFromFile(string path)
        {
            return File.ReadAllText(path, Encoding.UTF8);
        }
    }
}