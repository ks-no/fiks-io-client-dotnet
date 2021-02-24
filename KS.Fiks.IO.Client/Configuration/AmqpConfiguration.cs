using System.Net.Security;
using RabbitMQ.Client;

namespace KS.Fiks.IO.Client.Configuration
{
    public class AmqpConfiguration
    {
        public AmqpConfiguration(string host, int port = 5671, SslOption sslOption = null)
        {
            Host = host;
            Port = port;
            SslOption = sslOption ?? new SslOption
            {
                Enabled = true,
                ServerName = host,
                CertificateValidationCallback = (sender, certificate, chain, errors) => errors == SslPolicyErrors.None
            };
        }

        public string Host { get; }

        public int Port { get; }

        public SslOption SslOption { get; }

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