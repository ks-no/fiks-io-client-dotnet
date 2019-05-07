using System;
using System.IO;
using FluentAssertions;
using Moq;
using Xunit;

namespace KS.Fiks.IO.Client.Tests.Asic
{
    public class AsicDecrypterTests : IDisposable
    {
        private AsicDecrypterFixture _fixture;

        public AsicDecrypterTests()
        {
            _fixture = new AsicDecrypterFixture();
        }

        [Fact]
        public void CallsWriteWithExpectedPathWithStream()
        {
            var sut = _fixture.CreateSut();
            var stream = new MemoryStream();
            var path = "test/path/some.zip";
            sut.WriteDecrypted(stream, path);

            _fixture.FileWriterMock.Verify(_ => _.Write(path, It.IsAny<Stream>()));
        }

        [Fact]
        public void CallsWriteWithExpectedPathWithBytes()
        {
            var sut = _fixture.CreateSut();
            var stream = new byte[10];
            var path = "test/path/some.zip";
            sut.WriteDecrypted(stream, path);

            _fixture.FileWriterMock.Verify(_ => _.Write(path, It.IsAny<Stream>()));
        }

        [Fact]
        public void CallsDecryptOnStream()
        {
            var sut = _fixture.CreateSut();
            var stream = new MemoryStream();
            var path = "test/path/some.zip";
            sut.WriteDecrypted(stream, path);

            _fixture.DecryptionServiceMock.Verify(_ => _.Decrypt(stream));
        }

        [Fact]
        public void WritesDecryptedStream()
        {
            var decryptedStream = new MemoryStream(20);
            var sut = _fixture.WithDecryptedStream(decryptedStream).CreateSut();
            var stream = new MemoryStream();
            var path = "test/path/some.zip";
            sut.WriteDecrypted(stream, path);

            _fixture.DecryptionServiceMock.Verify(_ => _.Decrypt(stream));
            _fixture.FileWriterMock.Verify(_ => _.Write(path, decryptedStream));
        }

        [Fact]
        public void ReturnsDecryptedStream()
        {
            var decryptedStream = new MemoryStream(20);
            var sut = _fixture.WithDecryptedStream(decryptedStream).CreateSut();
            var stream = new MemoryStream();
            sut.Decrypt(stream).Should().Be(decryptedStream);
        }

        [Fact]
        public void ReturnsDecryptedStreamFromBytes()
        {
            var decryptedStream = new MemoryStream(20);
            var sut = _fixture.WithDecryptedStream(decryptedStream).CreateSut();
            var stream = new byte[10];
            sut.Decrypt(stream).Should().Be(decryptedStream);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _fixture.Dispose();
            }
        }
    }
}