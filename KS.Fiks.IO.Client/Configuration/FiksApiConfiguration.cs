namespace KS.Fiks.IO.Client.Configuration
{
    public class FiksApiConfiguration
    {
        private const string DefaultScheme = "https";

        private const string DefaultHost = "api.fiks.ks.no";

        private const int DefaultPort = 443;

        public FiksApiConfiguration(string scheme = null, string host = null, int? port = null)
        {
            Scheme = scheme ?? DefaultScheme;
            Host = host ?? DefaultHost;
            Port = port ?? DefaultPort;
        }

        protected FiksApiConfiguration(FiksApiConfiguration configuration)
        {
            Host = configuration.Host;
            Port = configuration.Port;
            Scheme = configuration.Scheme;
        }

        public string Host { get; }

        public int Port { get; }

        public string Scheme { get; }
    }
}