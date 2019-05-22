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
        private readonly IDecryptionService _decryptionService;

        public AsicDecrypter(IDecryptionService decryptionService)
        {
            _decryptionService = decryptionService;
        }

        public async Task WriteDecrypted(Task<Stream> encryptedZipStream, string outPath)
        {
            using (var fileStream = new FileStream(outPath, FileMode.OpenOrCreate))
            {
                try
                {
                    _decryptionService.Decrypt(await encryptedZipStream.ConfigureAwait(false)).CopyTo(fileStream);
                    fileStream.Flush();
                }
                catch (Exception ex)
                {
                    throw new FiksIODecryptionException("Unable to decrypt message. Is your private key correct?", ex);
                }
            }
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