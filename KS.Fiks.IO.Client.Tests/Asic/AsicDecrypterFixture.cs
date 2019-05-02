using System.IO;
using KS.Fiks.Crypto;
using KS.Fiks.IO.Client.Asic;
using KS.Fiks.IO.Client.FileIO;
using Moq;

namespace KS.Fiks.IO.Client.Tests.Asic
{
    internal class AsicDecrypterFixture
    {
        private Stream _decryptedStream = new MemoryStream();

        internal AsicDecrypterFixture()
        {
            FileWriterMock = new Mock<IFileWriter>();
            DecryptionServiceMock = new Mock<IDecryptionService>();
        }

        internal AsicDecrypter CreateSut()
        {
            SetupMocks();
            return new AsicDecrypter(DecryptionServiceMock.Object, FileWriterMock.Object);
        }

        internal AsicDecrypterFixture WithDecryptedStream(Stream decryptedStream)
        {
            _decryptedStream = decryptedStream;
            return this;
        }

        internal Mock<IFileWriter> FileWriterMock { get; }

        internal Mock<IDecryptionService> DecryptionServiceMock { get; }

        private void SetupMocks()
        {
            FileWriterMock.Setup(_ => _.Write(It.IsAny<string>(), It.IsAny<Stream>()));
            FileWriterMock.Setup(_ => _.Write(It.IsAny<string>(), It.IsAny<byte[]>()));
            DecryptionServiceMock.Setup(_ => _.Decrypt(It.IsAny<Stream>())).Returns(_decryptedStream);
        }
    }
}