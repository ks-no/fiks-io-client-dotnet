using System.Security.Cryptography.X509Certificates;

namespace KS.Fiks.IO.Client.Configuration
{
    public class VirksomhetssertifikatConfiguration
    {
        public X509Certificate2 publicCert;
        public string privateKey;

        public VirksomhetssertifikatConfiguration(X509Certificate2 publicCert, string privateKey)
        {
            this.publicCert = publicCert;
            this.privateKey = privateKey;
        }
    }
}