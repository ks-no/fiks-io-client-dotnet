using System.Collections.Generic;
using System.IO;
using KS.Fiks.IO.Client.Models;
using Org.BouncyCastle.X509;

namespace KS.Fiks.IO.Client.Asic
{
    public interface IAsicEncrypter
    {
        Stream Encrypt(X509Certificate publicKey, IList<IPayload> payload);
    }
}