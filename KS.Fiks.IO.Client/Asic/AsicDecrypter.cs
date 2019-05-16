using System;
using System.IO;
using System.Threading.Tasks;
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

        public async Task WriteDecrypted(Task<Stream> encryptedZipStream, string outPath)
        {
            _fileWriter.Write(outPath, await Decrypt(encryptedZipStream).ConfigureAwait(false));
        }

        public async Task<Stream> Decrypt(Task<Stream> encryptedZipStream)
        {
            try
            {
                return _decryptionService.Decrypt(await encryptedZipStream.ConfigureAwait(false));
            }
            catch (Exception ex)
            {
                throw new FiksIODecryptionException("Unable to decrypt message. Is your private key correct?", ex);
            }
        }
    }
}