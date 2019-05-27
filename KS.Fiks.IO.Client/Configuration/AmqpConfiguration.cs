using System.Net.Security;
using RabbitMQ.Client;

namespace KS.Fiks.IO.Client.Configuration
{
    public class AmqpConfiguration
    {
        private const string DefaultSslServerName = "minside.fiks.ks.no";

        public AmqpConfiguration(string host, int port = 5671, SslOption sslOption = null)
        {
            Host = host;
            Port = port;
            SslOption = sslOption ?? new SslOption
            {
                Enabled = true,
                ServerName = DefaultSslServerName,
                CertificateValidationCallback = (sender, certificate, chain, errors) => errors == SslPolicyErrors.None
            };
        }

        public string Host { get; }

        public int Port { get; }

        public SslOption SslOption { get; }
    }
}