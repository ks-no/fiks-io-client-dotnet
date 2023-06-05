using System.Security.Cryptography.X509Certificates;

namespace KS.Fiks.IO.Client.Configuration
{
    public class AsiceSigningConfiguration
    {
        public readonly string PublicKeyPath;
        public readonly string PrivateKeyPath;
        public readonly X509Certificate2 Certificate;

        /*
         * A public/private key pair
         */
        public AsiceSigningConfiguration(string publicKeyPath, string privateKeyPath)
        {
            PublicKeyPath = publicKeyPath;
            PrivateKeyPath = privateKeyPath;
        }

       /*
        * The x509Certificate2 parameter must be a x509Certificate that holds a matching private key
        */
        public AsiceSigningConfiguration(X509Certificate2 x509Certificate2)
        {
            Certificate = x509Certificate2;
        }
    }
}