using System;
using System.Net.Http;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace KS.Fiks.IO.ProtokollKonfigurasjon.Client.Tests
{
    public class KontoKonfigurasjonClientTests
    {
        [Fact]
        public void CreateClient_ReturnsClientWithConfiguredBaseUrl()
        {
            using var client = (ProtokollKonfigurasjonClient)KontoKonfigurasjonClient.CreateClient(
                "https://example.test",
                Guid.NewGuid(),
                "password",
                () => Task.FromResult("token"));

            client.BaseUrl.ShouldBe("https://example.test/");
        }

        [Fact]
        public async Task Dispose_DisposesTheUnderlyingHttpClient()
        {
            var client = KontoKonfigurasjonClient.CreateClient(
                "https://example.test",
                Guid.NewGuid(),
                "password",
                () => Task.FromResult("token"));

            client.Dispose();

            await Should.ThrowAsync<ObjectDisposedException>(() =>
                client.CreateKontoAsync(Guid.NewGuid(), Guid.NewGuid(), new CreateProtokollKontoRequest()));
        }

        [Fact]
        public void Dispose_CanBeCalledMoreThanOnce()
        {
            var client = KontoKonfigurasjonClient.CreateClient(
                "https://example.test",
                Guid.NewGuid(),
                "password",
                () => Task.FromResult("token"));

            client.Dispose();

            Should.NotThrow(() => client.Dispose());
        }
    }
}
