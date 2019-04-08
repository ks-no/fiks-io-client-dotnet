using System.IO;
using KS.Fiks.IO.Client.Asic;
using KS.Fiks.IO.Client.Encryption;
using KS.Fiks.IO.Client.FileIO;

namespace KS.Fiks.IO.Client.Models
{
    public class ReceivedMessage : ReceivedMessageMetadata, IReceivedMessage
    {
        private readonly byte[] _data;
        private readonly IAsicDecrypter _decrypter;
        private readonly IFileWriter _fileWriter;

        internal ReceivedMessage()
        {
        }

        internal ReceivedMessage(
            ReceivedMessageMetadata metadata,
            byte[] data,
            IAsicDecrypter decrypter,
            IFileWriter fileWriter)
            : base(metadata)
        {
            _data = data;
            _decrypter = decrypter;
            _fileWriter = fileWriter;
        }

        public Stream EncryptedStream => new MemoryStream(_data);

        public Stream DecryptedStream => _decrypter.Decrypt(_data);

        public void WriteEncryptedZip(string outPath)
        {
            _fileWriter.Write(outPath, _data);
        }

        public void WriteDecryptedZip(string outPath)
        {
            _decrypter.WriteDecrypted(_data, outPath);
        }
    }
}