using System.Net.Security;
using RabbitMQ.Client;

namespace KS.Fiks.IO.Client.Configuration
{
    public class AmqpConfiguration
    {
        public const string ProdHost = "io.fiks.ks.no";
        public const string TestHost = "io.fiks.test.ks.no";

        public AmqpConfiguration(
            string host,
            int port = 5671,
            SslOption sslOption = null,
            string applicationName = "Fiks IO klient (dotnet)",
            ushort prefetchCount = 10,
            string vhost = null,
            RateLimitConfiguration rateLimitConfiguration = null)
        {
            Host = host;
            Port = port;
            Vhost = vhost;
            SslOption = sslOption ?? new SslOption
            {
                Enabled = true,
                ServerName = host,
                CertificateValidationCallback = (sender, certificate, chain, errors) => errors == SslPolicyErrors.None
            };
            ApplicationName = applicationName;
            PrefetchCount = prefetchCount;
            RateLimitConfiguration = rateLimitConfiguration ?? new RateLimitConfiguration();
        }

        public string Host { get; }

        public int Port { get; }

        public string Vhost { get; }

        public SslOption SslOption { get; }

        /**
         * Setter et menneskelig-leslig navn på applikasjonen som bruker klient. Er til veldig god hjelp ved debugging.
         */
         public string ApplicationName { get; }

        /**
         * Hvor mange meldinger skal buffres i klienten når man lytter på nye meldinger? Tilsvarer AMQP Qos/Prefetch størrelse.
         */
        public ushort PrefetchCount { get; }

        public RateLimitConfiguration RateLimitConfiguration { get; }

        public static AmqpConfiguration CreateProdConfiguration(string applicationName = null, RateLimitConfiguration rateLimitConfiguration = null)
        {
            return new AmqpConfiguration(ProdHost, applicationName: applicationName, rateLimitConfiguration: rateLimitConfiguration);
        }

        public static AmqpConfiguration CreateTestConfiguration(string applicationName = null, RateLimitConfiguration rateLimitConfiguration = null)
        {
            return new AmqpConfiguration(TestHost, applicationName: applicationName, rateLimitConfiguration: rateLimitConfiguration);
        }
    }
}