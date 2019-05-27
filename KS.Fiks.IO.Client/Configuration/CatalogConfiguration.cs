namespace KS.Fiks.IO.Client.Configuration
{
    public class CatalogConfiguration : ApiConfiguration
    {
        private const string DefaultPath = "/svarinn2/katalog/api/v1";

        public CatalogConfiguration(string path = null, string scheme = null, string host = null, int? port = null)
            : base(scheme, host, port)
        {
            Path = path ?? DefaultPath;
        }

        public CatalogConfiguration(ApiConfiguration apiConfiguration)
            : base(apiConfiguration)
        {
            Path = DefaultPath;
        }

        public string Path { get; }
    }
}