using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using KS.Fiks.IO.Client;
using KS.Fiks.IO.Client.Configuration;
using KS.Fiks.IO.Client.Models;
using Ks.Fiks.Maskinporten.Client;

namespace ExampleApplication
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            // Relative or absolute path to the *.p12-file containing the test certificate used to sign tokens for Maskinporten
            var p12Filename = Environment.GetEnvironmentVariable("P12FILENAME");
            
            // Password required to use the certificate
            var p12Password = Environment.GetEnvironmentVariable("P12PWD");
            
            // The issuer as defined in Maskinporten
            var issuer = Environment.GetEnvironmentVariable("MASKINPORTEN_ISSUER");

            // accountId as defined in the Fiks Forvaltning Interface
            var fiksIoAccountId = Environment.GetEnvironmentVariable("FIKS_IO_ACCOUNT_ID");
            // private key corresponding to the public key uploaded in the Fiks Forvaltning Interface
            var fiksIoPrivateKeyPath = Environment.GetEnvironmentVariable("FIKS_IO_PRIVATE_KEY_PATH");

            // Values generated in Fiks Forvaltning when creating the "Integrasjon"
            var integrasjonId = Environment.GetEnvironmentVariable("INTEGRASJON_ID");
            var integrasjonPassword = Environment.GetEnvironmentVariable("INTEGRASJON_PWD");
            
            // Relative or absolute path to the public cert you want to use with the signing of the asice packages
            var asiceCertFilepath = Environment.GetEnvironmentVariable("ASICE_CERT_FILENAME");
            // Relative or absolute path to the privatekey (that is created from the cert above) that you want to use with the signing of the asice packages
            var asiceCertPrivateKeyPath = Environment.GetEnvironmentVariable("ASICE_CERT_PRIVATEKEY");
            
            // Create configuration easy with the fluent configuration builder
            var configuration = CreateConfigurationWithFluentBuilder(p12Filename, p12Password, issuer, integrasjonId, integrasjonPassword, fiksIoAccountId, fiksIoPrivateKeyPath, asiceCertFilepath, asiceCertPrivateKeyPath);
            
            // Or create the configuration manually 
            //var configuration = CreateConfig(issuer, p12Filename, p12Password, fiksIoAccountId, fiksIoPrivateKeyPath, integrasjonId, integrasjonPassword);

            using (var client = await FiksIOClient.CreateAsync(configuration))
            {
                var konto = await client.Lookup(new LookupRequest("999999999", "no.ks.fiks.melding", 2));
                Console.Out.WriteLineAsync($"Konto hentet! Kontonavn: {konto.KontoNavn}");
            }
            
        }

        // Creates a FiksIOConfiguration using the fluent builder
        private static FiksIOConfiguration CreateConfigurationWithFluentBuilder(string maskinportenCertFilename, string maskinportenCertPassword, string issuer,
            string integrasjonId, string integrasjonPassword, string fiksIoAccountId, string fiksIoPrivateKeyPath, string asiceCertFilepath = null, string asiceCertPrivateKeyPath = null)
        {
            // Combine all configurations
            return FiksIOConfigurationBuilder
                .Init()
                .WithAmqpConfiguration("fiks-io-klient-test-program-2", 1, false)
                .WithMaskinportenConfiguration(new X509Certificate2(maskinportenCertFilename, maskinportenCertPassword), issuer)
                .WithFiksIntegrasjonConfiguration(Guid.Parse(integrasjonId), integrasjonPassword)
                .WithFiksKontoConfiguration(Guid.Parse(fiksIoAccountId), ReadFromFile(fiksIoPrivateKeyPath))
                .WithAsiceSigningConfiguration(asiceCertFilepath, asiceCertPrivateKeyPath)
                .BuildTestConfiguration();
        }

        // Creates a FiksIOConfiguration manually
        private static void CreateConfig(string issuer, string p12Filename, string p12Password, string fiksIoAccountId,
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
            var configuration = new FiksIOConfiguration(kontoConfig, integrasjonConfig, maskinportenConfig, asiceSigningConfig, apiConfig, amqpConfig);
        }

        private static string ReadFromFile(string path)
        {
            return File.ReadAllText(path, Encoding.UTF8);
        }
    }
}