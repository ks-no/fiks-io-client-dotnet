using System.IO;
using System.Security.Cryptography.X509Certificates;
using KS.Fiks.ASiC_E.Crypto;
using KS.Fiks.IO.Client.Configuration;

namespace KS.Fiks.IO.Client.Asic
{
    public static class AsicSigningCertificateHolderFactory
    {
        public static PreloadedCertificateHolder Create(AsiceSigningConfiguration configuration)
        {
            return configuration.certificate != null ? Create(configuration.certificate) : Create(configuration.publicCertPath, configuration.privateKeyPath);
        }

        private static PreloadedCertificateHolder Create(string publicKeyPath, string privateKeyPath)
        {
            using (var publicKeyStream = new FileStream(publicKeyPath, FileMode.Open))
            using (var privateKeyStream = new FileStream(privateKeyPath, FileMode.Open))
            {
                using (var publicKeyBufferStream = new MemoryStream())
                using (var privateKeyBufferStream = new MemoryStream())
                {
                    publicKeyStream.CopyTo(publicKeyBufferStream);
                    privateKeyStream.CopyTo(privateKeyBufferStream);
                    return PreloadedCertificateHolder.Create(publicKeyBufferStream.ToArray(),
                        privateKeyBufferStream.ToArray());
                }
            }
        }

        private static PreloadedCertificateHolder Create(X509Certificate2 x509Certificate2)
        {
            return PreloadedCertificateHolder.Create(x509Certificate2);
        }
    }
}