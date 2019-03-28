using KS.Fiks.Crypto;
using Org.BouncyCastle.X509;

namespace KS.Fiks.IO.Client.Asic
{
    public interface IEncryptionServiceFactory
    {
        IEncryptionService Create(X509Certificate certificate);
    }
}