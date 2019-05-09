using System;
using System.IO;
using KS.Fiks.Crypto;
using KS.Fiks.IO.Client.Exceptions;
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

        public Stream Decrypt(Stream encryptedZipStream)
        {
            try
            {
                return _decryptionService.Decrypt(encryptedZipStream);
            }
            catch (Exception ex)
            {
                throw new FiksIODecryptionException("Unable to decrypt message. Is your private key correct?", ex);
            }
        }
    }
}