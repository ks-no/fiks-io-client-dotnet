using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using KS.Fiks.IO.Client.FileIO;
using KS.Fiks.IO.Crypto.Asic;
using KS.Fiks.IO.Crypto.Models;

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
            KlientMeldingId = ExtractKlientMeldingId();
            KlientKorrelasjonsId = ExtractKlientKorrelasjonsId();
        }

        private Guid? ExtractKlientMeldingId()
        {
            if (Headere == null || !Headere.ContainsKey(HeaderKlientMeldingId))
            {
                return null;
            }

            var parsed = Guid.Empty;
            if (Guid.TryParse(Headere[HeaderKlientMeldingId], out parsed))
            {
                return parsed;
            }

            return parsed;
        }

        private string ExtractKlientKorrelasjonsId()
        {
            if (Headere == null || !Headere.ContainsKey(HeaderKlientKorrelasjonsId))
            {
                return null;
            }

            return Headere[HeaderKlientKorrelasjonsId];
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

        public Task<IEnumerable<IPayload>> DecryptedPayloads => _decrypter.DecryptAndExtractPayloads(_streamProvider());
    }
}