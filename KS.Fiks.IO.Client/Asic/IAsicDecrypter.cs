using System.IO;

namespace KS.Fiks.IO.Client.Asic
{
    public interface IAsicDecrypter
    {
        void WriteDecrypted(Stream encryptedZipStream, string outPath);

        Stream Decrypt(Stream encryptedZipStream);
    }
}