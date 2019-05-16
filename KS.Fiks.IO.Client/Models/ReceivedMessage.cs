using System;
using System.IO;
using System.Threading.Tasks;
using KS.Fiks.IO.Client.Asic;
using KS.Fiks.IO.Client.FileIO;

namespace KS.Fiks.IO.Client.Models
{
    public class ReceivedMessage : ReceivedMessageMetadata, IReceivedMessage
    {
        private readonly Func<Task<Stream>> _dataProvider;
        private readonly IAsicDecrypter _decrypter;
        private readonly IFileWriter _fileWriter;

        internal ReceivedMessage(
            ReceivedMessageMetadata metadata,
            Func<Task<Stream>> dataProvider,
            IAsicDecrypter decrypter,
            IFileWriter fileWriter)
            : base(metadata)
        {
            _dataProvider = dataProvider;
            _decrypter = decrypter;
            _fileWriter = fileWriter;
        }

        public Task<Stream> EncryptedStream => _dataProvider();

        public Task<Stream> DecryptedStream => _decrypter.Decrypt(_dataProvider());

        public async Task WriteEncryptedZip(string outPath)
        {
            _fileWriter.Write(outPath, await _dataProvider().ConfigureAwait(false));
        }

        public async Task WriteDecryptedZip(string outPath)
        {
            await _decrypter.WriteDecrypted(_dataProvider(), outPath).ConfigureAwait(false);
        }
    }
}