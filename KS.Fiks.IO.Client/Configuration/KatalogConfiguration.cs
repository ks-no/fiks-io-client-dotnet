namespace KS.Fiks.IO.Client.Configuration
{
    public class KatalogConfiguration : ApiConfiguration
    {
        private const string DefaultPath = "/svarinn2/katalog/api/v1";

        public KatalogConfiguration(string path = null, string scheme = null, string host = null, int? port = null)
            : base(scheme, host, port)
        {
            Path = path ?? DefaultPath;
        }

        public KatalogConfiguration(ApiConfiguration apiConfiguration)
            : base(apiConfiguration)
        {
            Path = DefaultPath;
        }

        public string Path { get; }
    }
}