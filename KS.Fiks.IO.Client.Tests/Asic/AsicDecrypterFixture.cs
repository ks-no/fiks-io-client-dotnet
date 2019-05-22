using System;
using System.IO;
using KS.Fiks.Crypto;
using KS.Fiks.IO.Client.Asic;
using KS.Fiks.IO.Client.FileIO;
using Moq;

namespace KS.Fiks.IO.Client.Tests.Asic
{
    internal class AsicDecrypterFixture : IDisposable
    {
        private Stream _decryptedStream = new MemoryStream();

        private bool _shouldThrow = false;

        internal AsicDecrypterFixture()
        {
            DecryptionServiceMock = new Mock<IDecryptionService>();
        }

        public void Dispose()
        {
            _decryptedStream.Dispose();
        }

        internal AsicDecrypter CreateSut()
        {
            SetupMocks();
            return new AsicDecrypter(DecryptionServiceMock.Object);
        }

        internal AsicDecrypterFixture WithDecryptedStream(Stream decryptedStream)
        {
            _decryptedStream = decryptedStream;
            return this;
        }

        internal AsicDecrypterFixture WithExceptionThrown()
        {
            _shouldThrow = true;
            return this;
        }

        internal Mock<IDecryptionService> DecryptionServiceMock { get; }

        private void SetupMocks()
        {
            if (_shouldThrow)
            {
                DecryptionServiceMock.Setup(_ => _.Decrypt(It.IsAny<Stream>())).Throws<Exception>();
            }
            else
            {
                DecryptionServiceMock.Setup(_ => _.Decrypt(It.IsAny<Stream>())).Returns(_decryptedStream);
            }
        }
    }
}