using System.IO;
using KS.Fiks.ASiC_E.Crypto;

namespace KS.Fiks.IO.Client.Asic
{
    public class AsicSigningCertificateHolderFactory
    {
        public static PreloadedCertificateHolder Create(string publicKeyPath, string privateKeyPath)
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
    }
}