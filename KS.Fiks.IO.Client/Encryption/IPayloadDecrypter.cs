using System.IO;

namespace KS.Fiks.IO.Client.Encryption
{
    public interface IPayloadDecrypter
    {
        Stream Decrypt(Stream data);

        Stream Decrypt(byte[] data);
    }
}