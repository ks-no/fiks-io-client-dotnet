using System.Security.Cryptography.X509Certificates;

namespace KS.Fiks.IO.Client.Configuration
{
    public class AsiceSigningConfiguration
    {
        public readonly string publicCertPath;
        public readonly string privateKeyPath;
        public X509Certificate2 certificate;

        public AsiceSigningConfiguration(string publicCertPath, string privateKeyPath)
        {
            this.publicCertPath = publicCertPath;
            this.privateKeyPath = privateKeyPath;
        }

        public AsiceSigningConfiguration(X509Certificate2 x509Certificate2)
        {
            certificate = x509Certificate2;
        }
    }
}