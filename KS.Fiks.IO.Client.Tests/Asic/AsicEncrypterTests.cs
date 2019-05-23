using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FluentAssertions;
using KS.Fiks.IO.Client.Models;
using Moq;
using Xunit;

namespace KS.Fiks.IO.Client.Tests.Asic
{
    public class AsicEncrypterTests : IDisposable
    {
        private AsicEncrypterFixture _fixture;

        public AsicEncrypterTests()
        {
            _fixture = new AsicEncrypterFixture();
        }

        [Fact]
        public void ReturnsANonNullStream()
        {
            var sut = _fixture.CreateSut();

            var payload = new StreamPayload(_fixture.RandomStream, "filename.file");

            var outStream = sut.Encrypt(null, new List<IPayload> {payload});

            outStream.Should().NotBeNull();
        }

        [Fact]
        public void CallsAsiceBuilderAddFile()
        {
            var sut = _fixture.CreateSut();

            var payload = new StreamPayload(_fixture.RandomStream, "filename.file");

            var outStream = sut.Encrypt(null, new List<IPayload> {payload});

            _fixture.AsiceBuilderMock.Verify(_ => _.AddFile(payload.Payload, payload.Filename));
        }

        [Fact]
        public void AsiceBuilderIsDisposed()
        {
            var sut = _fixture.CreateSut();

            var payload = new StreamPayload(_fixture.RandomStream, "filename.file");

            var outStream = sut.Encrypt(null, new List<IPayload> {payload});

            _fixture.AsiceBuilderMock.Verify(_ => _.Dispose());
        }

        [Fact]
        public void ReturnsExpectedStream()
        {
            var expectedOutputString = "myStringToSend";
            var expectedOutStream = new MemoryStream(Encoding.UTF8.GetBytes(expectedOutputString));

            var sut = _fixture.WithContentAsEncryptedZipStreamed(expectedOutStream).CreateSut();

            var payload = new StreamPayload(_fixture.RandomStream, "filename.file");

            var outStream = sut.Encrypt(null, new List<IPayload> {payload});

            var outStreamBytes = new byte[outStream.Length];
            outStream.Read(outStreamBytes);
            var outputAsString = Encoding.UTF8.GetString(outStreamBytes);

            outputAsString.Should().Be(expectedOutputString);
        }

        [Fact]
        public void CallsEncrypt()
        {
            var expectedOutputString = "myStringToSend";
            var expectedZipStream = new MemoryStream(Encoding.UTF8.GetBytes(expectedOutputString));

            var sut = _fixture.WithContentAsZipStreamed(expectedZipStream).CreateSut();

            var payload = new StreamPayload(_fixture.RandomStream, "filename.file");

            var outStream = sut.Encrypt(null, new List<IPayload> {payload});

            _fixture.EncryptionServiceMock.Verify(
                _ => _.Encrypt(
                It.IsAny<Stream>(),
                It.IsAny<Stream>()));
        }

        [Fact]
        public void ThrowsIfPayloadIsEmpty()
        {
            var sut = _fixture.CreateSut();
            Assert.Throws<ArgumentException>(() => { sut.Encrypt(null, new List<IPayload>()); });
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