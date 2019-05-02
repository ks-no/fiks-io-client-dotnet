using System.IO;
using KS.Fiks.Crypto;
using KS.Fiks.IO.Client.FileIO;

namespace KS.Fiks.IO.Client.Asic
{
    internal class AsicDecrypter : IAsicDecrypter
    {
        private readonly IFileWriter _fileWriter;

        private readonly IDecryptionService _decryptionService;

        public AsicDecrypter(IDecryptionService decryptionService, IFileWriter fileWriter = null)
        {
            _decryptionService = decryptionService;
            _fileWriter = fileWriter ?? new FileWriter();
        }

        public void WriteDecrypted(Stream encryptedZipStream, string outPath)
        {
            _fileWriter.Write(outPath, Decrypt(encryptedZipStream));
        }

        public void WriteDecrypted(byte[] encryptedZipBytes, string outPath)
        {
            _fileWriter.Write(outPath, Decrypt(encryptedZipBytes));
        }

        public Stream Decrypt(Stream encryptedZipStream)
        {
            return _decryptionService.Decrypt(encryptedZipStream);
        }

        public Stream Decrypt(byte[] encryptedZipBytes)
        {
            return Decrypt(new MemoryStream(encryptedZipBytes));
        }
    }
}