using System.IO;
using System.Security.Cryptography.X509Certificates;
using KS.Fiks.ASiC_E.Crypto;
using Org.BouncyCastle.Crypto;

namespace KS.Fiks.IO.Client.Asic
{
    public class CertificateHolderFactory
    {

        /*
         *  pemPublicCertificateFilePath er avsenders offentlige sertifikat og CA kjede, og privateKeyFilePath er matchende private nøkkel
         */
        public static PreloadedCertificateHolder Create(X509Certificate2 publicCert, string privateKeyFilePath)
        {
            AsymmetricKeyParameter privatNokkel = new
            return PreloadedCertificateHolder.Create(publicCert.GetRawCertData(), privateKeyBufferStream.ToArray());
        }
    }
}