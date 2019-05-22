using System;
using System.IO;
using System.Threading.Tasks;
using KS.Fiks.IO.Client.Asic;
using KS.Fiks.IO.Client.FileIO;

namespace KS.Fiks.IO.Client.Models
{
    public class ReceivedMessage : ReceivedMessageMetadata, IReceivedMessage
    {
        private readonly Func<Task<Stream>> _streamProvider;
        private readonly IAsicDecrypter _decrypter;
        private readonly IFileWriter _fileWriter;

        internal ReceivedMessage(
            ReceivedMessageMetadata metadata,
            Func<Task<Stream>> streamProvider,
            IAsicDecrypter decrypter,
            IFileWriter fileWriter)
            : base(metadata)
        {
            _streamProvider = streamProvider;
            _decrypter = decrypter;
            _fileWriter = fileWriter;
        }

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
    }
}