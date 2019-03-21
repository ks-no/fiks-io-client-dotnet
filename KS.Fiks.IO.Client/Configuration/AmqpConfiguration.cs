namespace KS.Fiks.IO.Client.Configuration
{
    public class AmqpConfiguration
    {
        public AmqpConfiguration(string host, int port = 5672)
        {
            Host = host;
            Port = port;
        }

        public string Host { get; }

        public int Port { get; }
    }
}