using System.IO;

namespace KS.Fiks.IO.Client.Encryption
{
    internal interface IPayloadDecrypter
    {
        Stream Decrypt(Stream data);

        Stream Decrypt(byte[] data);
    }
}