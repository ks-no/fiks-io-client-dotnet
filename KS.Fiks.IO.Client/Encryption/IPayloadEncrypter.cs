using System.Collections.Generic;
using System.IO;
using KS.Fiks.IO.Client.Models;
using Org.BouncyCastle.X509;

namespace KS.Fiks.IO.Client.Encryption
{
    internal interface IPayloadEncrypter
    {
        Stream Encrypt(X509Certificate key, IEnumerable<IPayload> payload);
    }
}