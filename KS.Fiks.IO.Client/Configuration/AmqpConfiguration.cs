using RabbitMQ.Client;

namespace KS.Fiks.IO.Client.Configuration
{
    public class AmqpConfiguration
    {
        public AmqpConfiguration(string host, int port = 5671, SslOption sslOption = null)
        {
            Host = host;
            Port = port;
            SslOption = sslOption;
        }

        public string Host { get; }

        public int Port { get; }

        public SslOption SslOption { get; }
    }
}