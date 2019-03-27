using System;
using System.IO;
using KS.Fiks.ASiC_E;
using KS.Fiks.ASiC_E.Crypto;
using KS.Fiks.ASiC_E.Model;
using KS.Fiks.Crypto;
using KS.Fiks.IO.Client.Asic;
using Moq;

namespace KS.Fiks.IO.Client.Tests.Asic
{
    public class AsicEncrypterFixture
    {
        private readonly Mock<IAsiceBuilderFactory> _asiceBuilderFactoryMock;
        private Stream _outZipStream;
        private Stream _outEncryptedZipStream;


        public AsicEncrypterFixture()
        {
            _asiceBuilderFactoryMock = new Mock<IAsiceBuilderFactory>();
            AsiceBuilderMock = new Mock<IAsiceBuilder<AsiceArchive>>();
            CryptoServiceMock = new Mock<ICryptoService>();
        }

        public Mock<IAsiceBuilder<AsiceArchive>> AsiceBuilderMock { get; }

        public Mock<ICryptoService> CryptoServiceMock { get; }

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
            return new AsicEncrypter(_asiceBuilderFactoryMock.Object, CryptoServiceMock.Object);
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
                                            It.IsAny<MessageDigestAlgorithm>(),
                                            It.IsAny<ICertificateHolder>()))
                                    .Callback<Stream, MessageDigestAlgorithm, ICertificateHolder>((outStream, a, b) =>
                                    {
                                        _outZipStream.CopyTo(outStream);
                                        outStream.Seek(0l, SeekOrigin.Begin);
                                    })
                                    .Returns(AsiceBuilderMock.Object);


            AsiceBuilderMock.Setup(_ => _.AddFile(It.IsAny<Stream>(), It.IsAny<string>()))
                            .Returns(AsiceBuilderMock.Object);

            AsiceBuilderMock.Setup(_ => _.Dispose());

            CryptoServiceMock.Setup(_ => _.Encrypt(It.IsAny<Stream>(), It.IsAny<Stream>())).Callback<Stream, Stream>(
                (inStream, outStream) =>
                {
                    _outEncryptedZipStream.CopyTo(outStream);
                    outStream.Seek(0l, SeekOrigin.Begin);
                });
        }
    }
}