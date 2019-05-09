using System;
using System.IO;
using KS.Fiks.IO.Client.Asic;
using KS.Fiks.IO.Client.FileIO;

namespace KS.Fiks.IO.Client.Models
{
    public class ReceivedMessage : ReceivedMessageMetadata, IReceivedMessage
    {
        private readonly Func<Stream> _dataProvider;
        private readonly IAsicDecrypter _decrypter;
        private readonly IFileWriter _fileWriter;

        internal ReceivedMessage(
            ReceivedMessageMetadata metadata,
            Func<Stream> dataProvider,
            IAsicDecrypter decrypter,
            IFileWriter fileWriter)
            : base(metadata)
        {
            _dataProvider = dataProvider;
            _decrypter = decrypter;
            _fileWriter = fileWriter;
        }

        public Stream EncryptedStream => _dataProvider();

        public Stream DecryptedStream => _decrypter.Decrypt(_dataProvider());

        public void WriteEncryptedZip(string outPath)
        {
            _fileWriter.Write(outPath, _dataProvider());
        }

        public void WriteDecryptedZip(string outPath)
        {
            _decrypter.WriteDecrypted(_dataProvider(), outPath);
        }
    }
}