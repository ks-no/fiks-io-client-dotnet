using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using KS.Fiks.ASiC_E;
using KS.Fiks.ASiC_E.Model;
using KS.Fiks.IO.Client.Asic;
using KS.Fiks.IO.Client.FileIO;

namespace KS.Fiks.IO.Client.Models
{
    public class MottattMelding : MottattMeldingMetadata, IMottattMelding
    {
        private readonly Func<Task<Stream>> _streamProvider;
        private readonly IAsicDecrypter _decrypter;
        private readonly IFileWriter _fileWriter;
        private IEnumerable<IPayload> _payloads;

        internal MottattMelding(
            bool hasPayload,
            MottattMeldingMetadata metadata,
            Func<Task<Stream>> streamProvider,
            IAsicDecrypter decrypter,
            IFileWriter fileWriter)
            : base(metadata)
        {
            HasPayload = hasPayload;
            _streamProvider = streamProvider;
            _decrypter = decrypter;
            _fileWriter = fileWriter;
        }

        public bool HasPayload { get; }

        public Task<Stream> EncryptedStream => _streamProvider();

        public Task<Stream> DecryptedStream => _decrypter.Decrypt(_streamProvider());

        public async Task WriteEncryptedZip(string outPath)
        {
            _fileWriter.Write(await _streamProvider().ConfigureAwait(false), outPath);
        }

        public async Task WriteDecryptedZip(string outPath)
        {
            await _decrypter.WriteDecrypted(_streamProvider(), outPath).ConfigureAwait(false);
        }

        public Task<IEnumerable<IPayload>> Payloads => ExtractPayloads();

        private async Task<IEnumerable<IPayload>> ExtractPayloads()
        {
            if (_payloads == null)
            {
                List<StreamPayload> payloads = new List<StreamPayload>();

                using (Stream stream = await DecryptedStream.ConfigureAwait(false))
                {
                    AsiceReadModel asiceReader = new AsiceReader().Read(stream);

                    foreach (AsiceReadEntry entry in asiceReader.Entries)
                    {
                        using (Stream entryStream = entry.OpenStream())
                        {
                            MemoryStream memoryStream = new MemoryStream();

                            await entryStream.CopyToAsync(memoryStream).ConfigureAwait(false);

                            StreamPayload payload = new StreamPayload(memoryStream, entry.FileName);

                            payloads.Add(payload);
                        }
                    }
                }

                _payloads = payloads;
            }

            return _payloads;
        }
    }
}