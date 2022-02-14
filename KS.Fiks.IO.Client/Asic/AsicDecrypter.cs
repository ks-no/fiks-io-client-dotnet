using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using KS.Fiks.ASiC_E;
using KS.Fiks.Crypto;
using KS.Fiks.IO.Client.Exceptions;
using KS.Fiks.IO.Client.Models;

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

        public async Task<IEnumerable<IPayload>> DecryptAndExtractPayloads(Task<Stream> encryptedZipStream)
        {
            var payloads = new List<StreamPayload>();

            using (var stream = await Decrypt(encryptedZipStream).ConfigureAwait(false))
            {
                var asiceReader = new AsiceReader().Read(stream);

                foreach (var entry in asiceReader.Entries)
                {
                    using (var entryStream = entry.OpenStream())
                    {

                        var memoryStream = new MemoryStream();

                        await entryStream.CopyToAsync(memoryStream).ConfigureAwait(false);

                        var payload = new StreamPayload(memoryStream, entry.FileName);

                        payloads.Add(payload);
                    }
                }
            }

            return payloads;
        }
    }
}