using KS.Fiks.Crypto;
using Org.BouncyCastle.X509;

namespace KS.Fiks.IO.Client.Asic
{
    public class EncryptionServiceFactory : IEncryptionServiceFactory
    {
        public IEncryptionService Create(X509Certificate certificate)
        {
            return EncryptionService.Create(certificate);
        }
    }
}