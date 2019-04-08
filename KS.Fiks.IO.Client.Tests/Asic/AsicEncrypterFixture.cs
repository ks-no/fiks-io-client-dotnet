using System;
using System.IO;
using KS.Fiks.ASiC_E;
using KS.Fiks.ASiC_E.Model;
using KS.Fiks.Crypto;
using KS.Fiks.IO.Client.Asic;
using Moq;
using Org.BouncyCastle.X509;

namespace KS.Fiks.IO.Client.Tests.Asic
{
    public class AsicEncrypterFixture
    {
        private readonly Mock<IAsiceBuilderFactory> _asiceBuilderFactoryMock;
        private readonly Mock<IEncryptionServiceFactory> _encryptionServiceFactoryMock;
        private Stream _outZipStream;
        private Stream _outEncryptedZipStream;

        public AsicEncrypterFixture()
        {
            _asiceBuilderFactoryMock = new Mock<IAsiceBuilderFactory>();
            _encryptionServiceFactoryMock = new Mock<IEncryptionServiceFactory>();
            AsiceBuilderMock = new Mock<IAsiceBuilder<AsiceArchive>>();
            EncryptionServiceMock = new Mock<IEncryptionService>();
        }

        public Mock<IAsiceBuilder<AsiceArchive>> AsiceBuilderMock { get; }

        public Mock<IEncryptionService> EncryptionServiceMock { get; }

        public AsicEncrypterFixture WithContentAsZipStreamed(Stream stream)
        {
            _outZipStream = stream;
            return this;
        }

        public AsicEncrypterFixture WithContentAsEncryptedZipStreamed(Stream stream)
        {
            _outEncryptedZipStream = stream;
            return this;
        }

        internal AsicEncrypter CreateSut()
        {
            SetDefaults();
            SetupMocks();
            return new AsicEncrypter(_asiceBuilderFactoryMock.Object, _encryptionServiceFactoryMock.Object);
        }

        internal MemoryStream RandomStream
        {
            get
            {
                var rnd = new Random();
                var b = new byte[13];
                rnd.NextBytes(b);
                return new MemoryStream(b);
            }
        }

        private void SetDefaults()
        {
            if (_outZipStream == null)
            {
                _outZipStream = new MemoryStream(new[] {byte.MaxValue});
            }

            if (_outEncryptedZipStream == null)
            {
                _outEncryptedZipStream = new MemoryStream(new[] {byte.MaxValue});
            }
        }

        private void SetupMocks()
        {
            _asiceBuilderFactoryMock.Setup(_ =>
                                        _.GetBuilder(
                                            It.IsAny<Stream>(),
                                            It.IsAny<MessageDigestAlgorithm>()))
                                    .Callback<Stream, MessageDigestAlgorithm>((outStream, a) =>
                                    {
                                        _outZipStream.CopyTo(outStream);
                                        outStream.Seek(0L, SeekOrigin.Begin);
                                    })
                                    .Returns(AsiceBuilderMock.Object);
            _encryptionServiceFactoryMock.Setup(_ => _.Create(It.IsAny<X509Certificate>()))
                                         .Returns(EncryptionServiceMock.Object);

            AsiceBuilderMock.Setup(_ => _.AddFile(It.IsAny<Stream>(), It.IsAny<string>()))
                            .Returns(AsiceBuilderMock.Object);

            AsiceBuilderMock.Setup(_ => _.Dispose());

            EncryptionServiceMock.Setup(_ => _.Encrypt(It.IsAny<Stream>(), It.IsAny<Stream>()))
                                 .Callback<Stream, Stream>(
                                     (inStream, outStream) =>
                                     {
                                         _outEncryptedZipStream.CopyTo(outStream);
                                         outStream.Seek(0L, SeekOrigin.Begin);
                                     });
        }
    }
}