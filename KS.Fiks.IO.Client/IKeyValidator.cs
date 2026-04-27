using Org.BouncyCastle.X509;

namespace KS.Fiks.IO.Client
{
    internal interface IKeyValidator
    {
        bool ValidateCertificateAgainstPrivateKeys(X509Certificate certificate);
    }
}
