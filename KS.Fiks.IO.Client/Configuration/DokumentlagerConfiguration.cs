namespace KS.Fiks.IO.Client.Configuration
{
    public class DokumentlagerConfiguration : ApiConfiguration
    {
        private const string DefaultDownloadPath = "/dokumentlager/nedlasting/";

        public DokumentlagerConfiguration(string scheme = null, string host = null, int? port = null, string downloadPath = null)
        : base(scheme, host, port)
        {
            DownloadPath = downloadPath ?? DefaultDownloadPath;
        }

        public DokumentlagerConfiguration(ApiConfiguration apiConfiguration, string downloadPath = null)
            : base(apiConfiguration)
        {
            DownloadPath = downloadPath ?? DefaultDownloadPath;
        }

        public string DownloadPath { get; }
    }
}