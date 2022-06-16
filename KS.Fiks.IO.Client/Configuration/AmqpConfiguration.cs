using System.Net.Security;
using RabbitMQ.Client;

namespace KS.Fiks.IO.Client.Configuration
{
    public class AmqpConfiguration
    {
        public AmqpConfiguration(string host, int port = 5671, SslOption sslOption = null, string applicationName = "Fiks IO klient (dotnet)", int prefetchSize = 10)
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
            PrefetchSize = prefetchSize;
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
        public int PrefetchSize { get; }
        
        public static AmqpConfiguration CreateProdConfiguration()
        {
            return new AmqpConfiguration("io.fiks.ks.no");
        }

        public static AmqpConfiguration CreateTestConfiguration()
        {
            return new AmqpConfiguration("io.fiks.test.ks.no");
        }
    }
}