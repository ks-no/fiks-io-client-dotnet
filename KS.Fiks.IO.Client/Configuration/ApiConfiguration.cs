namespace KS.Fiks.IO.Client.Configuration
{
    public class ApiConfiguration
    {
        private const string DefaultScheme = "https";

        private const string DefaultHost = "api.fiks.ks.no";

        private const int DefaultPort = 443;

        public ApiConfiguration(string scheme = null, string host = null, int? port = null)
        {
            Scheme = scheme ?? DefaultScheme;
            Host = host ?? DefaultHost;
            Port = port ?? DefaultPort;
        }

        protected ApiConfiguration(ApiConfiguration configuration)
        {
            Host = configuration.Host;
            Port = configuration.Port;
            Scheme = configuration.Scheme;
        }

        public string Host { get; }

        public int Port { get; }

        public string Scheme { get; }
        
        public static ApiConfiguration CreateProdConfiguration()
        {
            return new ApiConfiguration();
        }
        
        public static ApiConfiguration CreateTestConfiguration()
        {
            return new ApiConfiguration(host: "io.fiks.test.ks.no");
        }
    }
}