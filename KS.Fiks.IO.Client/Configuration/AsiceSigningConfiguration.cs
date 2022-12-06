namespace KS.Fiks.IO.Client.Configuration
{
    public class AsiceSigningConfiguration
    {
        public string publicCertPath;
        public string privateKeyPath;

        public AsiceSigningConfiguration(string publicCertPath, string privateKeyPath)
        {
            this.publicCertPath = publicCertPath;
            this.privateKeyPath = privateKeyPath;
        }
    }
}