using System.Collections.Generic;
using System.IO;
using System.Linq;
using KS.Fiks.IO.Client.Models;
using Org.BouncyCastle.X509;

namespace KS.Fiks.IO.Client.Encryption
{
    internal class DummyCrypt : IPayloadDecrypter, IPayloadEncrypter
    {
        public Stream Decrypt(Stream data)
        {
            return data;
        }

        public Stream Decrypt(byte[] data)
        {
            return new MemoryStream(data);
        }

        public Stream Encrypt(X509Certificate key, IEnumerable<IPayload> payload)
        {
            return payload.FirstOrDefault()?.Payload ?? new MemoryStream();
        }
    }
}