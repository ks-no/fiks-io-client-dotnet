namespace KS.Fiks.IO.Client.Configuration
{
    public class CatalogConfiguration
    {
        public CatalogConfiguration()
        {
            Path = "/svarinn2/katalog/api/v1";
        }

        public string Host { get; set; }

        public int Port { get; set; }

        public string Scheme { get; set; }

        public string Path { get; set; }
    }
}