using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using KS.Fiks.IO.Client;
using KS.Fiks.IO.Client.Configuration;
using KS.Fiks.IO.Client.Models;
using Ks.Fiks.Maskinporten.Client;

namespace ExampleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            // Relative or absolute path to the *.p12-file containing the test certificate used to sign tokens for Maskinporten
            var p12Filename = Environment.GetEnvironmentVariable("P12FILENAME");
            
            // Password required to use the certificate
            var p12Password = Environment.GetEnvironmentVariable("P12PWD");
            
            // The issuer as defined in Maskinporten
            var issuer = Environment.GetEnvironmentVariable("MASKINPORTEN_ISSUER");
            
            // ID-porten machine to machine configuration
            var maskinportenConfig = new MaskinportenClientConfiguration(
                audience: @"https://ver2.maskinporten.no/", // ID-porten audience path
                tokenEndpoint: @"https://ver2.maskinporten.no/token", // ID-porten token path
                issuer: issuer, // KS issuer name
                numberOfSecondsLeftBeforeExpire: 10, // The token will be refreshed 10 seconds before it expires
                certificate: new X509Certificate2(p12Filename, p12Password));

            // accountId as defined in the Fiks Forvaltning Interface
            var fiksIoAccountId = Environment.GetEnvironmentVariable("FIKS_IO_ACCOUNT_ID");
            // private key corresponding to the public key uploaded in the Fiks Forvaltning Interface
            var fiksIoPrivateKeyPath = Environment.GetEnvironmentVariable("FIKS_IO_PRIVATE_KEY_PATH");
            
            // Fiks IO account configuration
            var kontoConfig = new KontoConfiguration(
                kontoId: Guid.Parse(fiksIoAccountId) /* Fiks IO accountId as Guid */,
                privatNokkel: ReadFromFile(fiksIoPrivateKeyPath) /* Private key in PEM format, paired with the public key supplied to Fiks IO account */);

            // Values generated in Fiks Forvaltning when creating the "Integrasjon"
            var integrasjonId = Environment.GetEnvironmentVariable("INTEGRASJON_ID");
            var integrasjonPassword = Environment.GetEnvironmentVariable("INTEGRASJON_PWD");
            
            // Id and password for integration associated to the Fiks IO account.
            var integrasjonConfig = new IntegrasjonConfiguration(
                Guid.Parse(integrasjonId) /* Integration id as Guid */,
                integrasjonPassword /* Integration password */);

            
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
            var configuration = new FiksIOConfiguration(kontoConfig, integrasjonConfig, maskinportenConfig, apiConfig,
                amqpConfig);
            using (var client = new FiksIOClient(configuration))
            {
                var lookupTask = client.Lookup(new LookupRequest("999999999", "no.ks.fiks.melding", 2));
                lookupTask.Wait(TimeSpan.FromSeconds(30));

                var konto = lookupTask.Result;
            }
            
        }

        private static string ReadFromFile(string path)
        {
            return File.ReadAllText(path, Encoding.UTF8);
        }
    }
}