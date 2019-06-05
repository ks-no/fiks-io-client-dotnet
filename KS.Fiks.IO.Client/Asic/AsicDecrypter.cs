using System;
using System.IO;
using System.Threading.Tasks;
using KS.Fiks.Crypto;
using KS.Fiks.IO.Client.Exceptions;

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
                        await _decryptionService.Decrypt(await encryptedZipStream.ConfigureAwait(false))
                                                .CopyToAsync(fileStream).ConfigureAwait(false);
                        await fileStream.FlushAsync().ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        throw new FiksIODecryptionException("Unable to decrypt melding. Is your private key correct?", ex);
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
                throw new FiksIODecryptionException("Unable to decrypt melding. Is your private key correct?", ex);
            }
        }
    }
}