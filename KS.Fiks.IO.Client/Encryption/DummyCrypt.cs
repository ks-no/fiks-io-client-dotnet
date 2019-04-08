using System.IO;

namespace KS.Fiks.IO.Client.Encryption
{
    internal class DummyCrypt : IPayloadDecrypter
    {
        public Stream Decrypt(Stream data)
        {
            return data;
        }

        public Stream Decrypt(byte[] data)
        {
            return new MemoryStream(data);
        }
    }
}