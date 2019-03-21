using System.Collections.Generic;
using System.IO;
using KS.Fiks.IO.Client.Models;

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

        public Stream Encrypt(string key, IEnumerable<IPayload> payload)
        {
            return new MemoryStream();
        }
    }
}