using System.IO;

namespace KS.Fiks.IO.Client.Asic
{
    public class AsicDecrypter : IAsicDecrypter
    {
        public void WriteDecrypted(Stream encryptedZipStream, string outPath)
        {
            throw new System.NotImplementedException();
        }

        public Stream Decrypt(Stream encryptedZipStream)
        {
            throw new System.NotImplementedException();
        }

        public void WriteDecrypted(byte[] encryptedZipBytes, string outPath)
        {
            throw new System.NotImplementedException();
        }

        public Stream Decrypt(byte[] encryptedZipBytes)
        {
            throw new System.NotImplementedException();
        }
    }
}