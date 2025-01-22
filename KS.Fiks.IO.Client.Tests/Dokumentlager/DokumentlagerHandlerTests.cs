using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using KS.Fiks.IO.Client.Exceptions;
using Moq;
using Moq.Protected;
using Shouldly;
using Xunit;

namespace KS.Fiks.IO.Client.Tests.Dokumentlager
{
    public class DokumentlagerHandlerTests : IDisposable
    {
        private DokumentlagerHandlerFixture _fixture;

        public DokumentlagerHandlerTests()
        {
            _fixture = new DokumentlagerHandlerFixture();
        }

        [Fact]
        public void DownloadReturnsAStream()
        {
            var sut = _fixture.CreateSut();

            var result = sut.Download(Guid.NewGuid());
        }

        [Fact]
        public async Task CallsExpectedUri()
        {
            var sut = _fixture.WithSchema("https").WithPort(554).WithHost("api.ks.no").WithDownloadPath("/dokumentlager/download").CreateSut();

            var messageId = Guid.NewGuid();

            var expectedUri = $"https://api.ks.no:554/dokumentlager/download/{messageId}";

            var result = await sut.Download(messageId).ConfigureAwait(false);

            _fixture.RequestUri.ToString().ShouldBe(expectedUri);

            _fixture.HttpMessageHandleMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(
                    req => req.RequestUri.ToString() == expectedUri),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task CallsUsingGet()
        {
            var sut = _fixture.CreateSut();

            var messageId = Guid.NewGuid();

            var result = await sut.Download(messageId).ConfigureAwait(false);

            _fixture.HttpMessageHandleMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(
                    req => req.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task ThrowsFiksIODokumentlagerResponseExceptionIfOutStreamIsEmpty()
        {
            var sut = _fixture.WithEmptyOutStream().CreateSut();

            var messageId = Guid.NewGuid();

            await Assert.ThrowsAsync<FiksIODokumentlagerResponseException>(async () => await sut.Download(messageId).ConfigureAwait(false))
                        .ConfigureAwait(false);
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