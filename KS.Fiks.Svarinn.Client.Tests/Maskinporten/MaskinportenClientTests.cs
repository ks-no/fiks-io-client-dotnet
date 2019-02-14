using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Moq.Protected;
using Xunit;


namespace Ks.Fiks.Svarinn.ClientTest.Maskinporten
{
    public class GetAccessToken
    {
        private MaskinportenClientFixture _fixture;

        public GetAccessToken()
        {
            _fixture = new MaskinportenClientFixture();
        }
        
        [Fact]
        public async Task ReturnsAString()
        {
            var sut = _fixture.CreateSut();
            var scopes = new List<string>();
            
            var accessToken = await sut.GetAccessToken(scopes);
            accessToken.Should().BeOfType<string>();
        }

        [Fact]
        public async Task CallsSendAsyncExactlyOnce()
        {
            var sut = _fixture.CreateSut();
            var scopes = new List<string>();
            
            await sut.GetAccessToken(scopes);

            _fixture.HttpMessageHandleMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get 
                ),
                ItExpr.IsAny<CancellationToken>()
            );
        }
    }
}