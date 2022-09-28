using System.Net.Security;
using RabbitMQ.Client;

namespace KS.Fiks.IO.Client.Configuration
{
    public class AmqpConfiguration
    {
        public AmqpConfiguration(string host, int port = 5671, SslOption sslOption = null, string applicationName = "Fiks IO klient (dotnet)", ushort prefetchCount = 10, bool keepAlive = false)
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

        public static AmqpConfiguration CreateProdConfiguration(bool keepAlive = false)
        {
            return new AmqpConfiguration("io.fiks.ks.no", keepAlive: keepAlive);
        }

        public static AmqpConfiguration CreateTestConfiguration(bool keepAlive = false)
        {
            return new AmqpConfiguration("io.fiks.test.ks.no", keepAlive: keepAlive);
        }
    }
}