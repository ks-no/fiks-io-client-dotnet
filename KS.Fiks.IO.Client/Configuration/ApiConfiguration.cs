namespace KS.Fiks.IO.Client.Configuration
{
    public class ApiConfiguration
    {
        public const string ProdHost = "api.fiks.ks.no";
        public const string TestHost = "api.fiks.test.ks.no";
        private const string DefaultScheme = "https";
        private const int DefaultPort = 443;

        public ApiConfiguration(string scheme = null, string host = null, int? port = null)
        {
            Scheme = scheme ?? DefaultScheme;
            Host = host ?? ProdHost;
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
            return new ApiConfiguration(host: TestHost);
        }
    }
}