using System.Collections.Generic;
using System.IO;
using KS.Fiks.IO.Client.Models;

namespace KS.Fiks.IO.Client.Encryption
{
    public interface IPayloadEncrypter
    {
        Stream Encrypt(string key, IEnumerable<IPayload> payload);
    }
}