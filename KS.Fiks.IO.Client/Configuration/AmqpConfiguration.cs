using System.Net.Security;
using RabbitMQ.Client;

namespace KS.Fiks.IO.Client.Configuration
{
    public class AmqpConfiguration
    {
        public const string ProdHost = "io.fiks.ks.no";
        public const string TestHost = "io.fiks.test.ks.no";
        public const int DefaultKeepAliveHealthCheckInterval = 1 * 60 * 1000;
        public const int DefaultPort = 5671;

        public AmqpConfiguration(string host, int port = DefaultPort, SslOption sslOption = null, string applicationName = "Fiks IO klient (dotnet)", ushort prefetchCount = 10, bool keepAlive = true, int keepAliveCheckInterval = DefaultKeepAliveHealthCheckInterval)
        {
            Host = host;
            Port = port;
            SslOption = sslOption ?? new SslOption
            {
                Enabled = true,
                ServerName = host,
                CertificateValidationCallback = (sender, certificate, chain, errors) => errors == SslPolicyErrors.None
            };
            ApplicationName = applicationName;
            PrefetchCount = prefetchCount;
            KeepAlive = keepAlive;
            KeepAliveHealthCheckInterval = keepAliveCheckInterval;
        }

        public string Host { get; }

        public int Port { get; }

        public SslOption SslOption { get; }

        /**
         * Setter et menneskelig-leslig navn på applikasjonen som bruker klient. Er til veldig god hjelp ved debugging.
         */
         public string ApplicationName { get; }

        /**
         * Hvor mange meldinger skal buffres i klienten når man lytter på nye meldinger? Tilsvarer AMQP Qos/Prefetch størrelse.
         */
        public ushort PrefetchCount { get; }

        public bool KeepAlive { get; }

        public int KeepAliveHealthCheckInterval { get; }

        public static AmqpConfiguration CreateProdConfiguration(bool keepAlive = false, string applicationName = null)
        {
            return new AmqpConfiguration(ProdHost, keepAlive: keepAlive, applicationName: applicationName);
        }

        public static AmqpConfiguration CreateTestConfiguration(bool keepAlive = false, string applicationName = null)
        {
            return new AmqpConfiguration(TestHost, keepAlive: keepAlive, applicationName: applicationName);
        }
    }
}