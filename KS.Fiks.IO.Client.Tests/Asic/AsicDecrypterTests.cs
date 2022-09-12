using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using KS.Fiks.IO.Client.Exceptions;
using Moq;
using Xunit;

namespace KS.Fiks.IO.Client.Tests.Asic
{
    [Collection("Sequential")]
    public class AsicDecrypterTests : IDisposable
    {
        private AsicDecrypterFixture _fixture;

        public AsicDecrypterTests()
        {
            _fixture = new AsicDecrypterFixture();
        }

        [Fact]
        public async Task CallsDecryptOnFileStream()
        {
            var sut = _fixture.CreateSut();
            var stream = new MemoryStream();
            var streamTask = Task.FromResult((Stream)stream);
            var path = "some.zip";
            await sut.WriteDecrypted(streamTask, path).ConfigureAwait(false);

            _fixture.DecryptionServiceMock.Verify(_ => _.Decrypt(It.IsAny<Stream>()));
        }

        [Fact]
        public async Task ReturnsDecryptedStream()
        {
            var decryptedStream = new MemoryStream(20);
            var sut = _fixture.WithDecryptedStream(decryptedStream).CreateSut();
            var stream = Task.FromResult((Stream)new MemoryStream());
            var result = await sut.Decrypt(stream).ConfigureAwait(false);
            result.Should().BeSameAs(decryptedStream);
        }

        [Fact]
        public async Task ThrowsFiksIODecryptionExceptionIfDecrypterThrowsOnDecrypt()
        {
            var sut = _fixture.WithExceptionThrown().CreateSut();
            var stream = Task.FromResult((Stream)new MemoryStream());

            await Assert.ThrowsAsync<FiksIODecryptionException>(async () => await sut.Decrypt(stream).ConfigureAwait(false)).ConfigureAwait(false);
        }

        [Fact]
        public async Task ThrowsFiksIODecryptionExceptionIfDecrypterThrowsOnWriteDecrypted()
        {
            var sut = _fixture.WithExceptionThrown().CreateSut();
            var stream = Task.FromResult((Stream)new MemoryStream());
            var path = "some.zip";

            await Assert.ThrowsAsync<FiksIODecryptionException>(async () => await sut.WriteDecrypted(stream, path).ConfigureAwait(false)).ConfigureAwait(false);
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