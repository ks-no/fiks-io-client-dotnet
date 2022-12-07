using System;
using System.IO;
using KS.Fiks.ASiC_E;
using KS.Fiks.ASiC_E.Crypto;
using KS.Fiks.ASiC_E.Model;
using KS.Fiks.Crypto;
using KS.Fiks.IO.Client.Asic;
using Moq;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.X509;

namespace KS.Fiks.IO.Client.Tests.Asic
{
    internal class AsicEncrypterFixture : IDisposable
    {
        private readonly Mock<IEncryptionServiceFactory> _encryptionServiceFactoryMock;
        private readonly CertHolderMock _certificateHolder;
        private Stream _outZipStream;
        private Stream _outEncryptedZipStream;

        public AsicEncrypterFixture()
        {
            AsiceBuilderFactoryMock = new Mock<IAsiceBuilderFactory>();
            _encryptionServiceFactoryMock = new Mock<IEncryptionServiceFactory>();
            AsiceBuilderMock = new Mock<IAsiceBuilder<AsiceArchive>>();
            EncryptionServiceMock = new Mock<IEncryptionService>();
            _certificateHolder = new CertHolderMock(null, null);
        }

        public Mock<IAsiceBuilderFactory> AsiceBuilderFactoryMock { get; }

        public Mock<IAsiceBuilder<AsiceArchive>> AsiceBuilderMock { get; }

        public Mock<IEncryptionService> EncryptionServiceMock { get; }

        public AsicEncrypterFixture WithContentAsZipStreamed(Stream stream)
        {
            _outZipStream = stream;
            return this;
        }

        public void Dispose()
        {
            _outZipStream.Dispose();
            _outEncryptedZipStream.Dispose();
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
            return new AsicEncrypter(AsiceBuilderFactoryMock.Object, _encryptionServiceFactoryMock.Object);
        }

        internal AsicEncrypter CreateSutWithAsicSigning()
        {
            SetDefaults();
            SetupMocksWithAsiceSigning();
            return new AsicEncrypter(AsiceBuilderFactoryMock.Object, _encryptionServiceFactoryMock.Object, _certificateHolder);
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
            AsiceBuilderFactoryMock.Setup(_ =>
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

        private void SetupMocksWithAsiceSigning()
        {
            AsiceBuilderFactoryMock.Setup(_ =>
                    _.GetBuilder(
                        It.IsAny<Stream>(),
                        It.IsAny<MessageDigestAlgorithm>(),
                        It.IsAny<ICertificateHolder>()))
                .Callback<Stream, MessageDigestAlgorithm, ICertificateHolder>((outStream, a, i) =>
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

        public class CertHolderMock : ICertificateHolder
        {
            private AsymmetricKeyParameter _asymmetricKeyParameter;
            private X509Certificate _certificate;

            public CertHolderMock(X509Certificate certificate, AsymmetricKeyParameter asymmetricKeyParameter)
            {
                _asymmetricKeyParameter = asymmetricKeyParameter;
                _certificate = certificate;
            }

            public AsymmetricKeyParameter GetPrivateKey()
            {
                return _asymmetricKeyParameter;
            }

            public X509Certificate GetPublicCertificate()
            {
                return _certificate;
            }
        }
    }
}