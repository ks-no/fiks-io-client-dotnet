using System;
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
        public async Task ReturnsAccessToken()
        {
            var expectedAccessToken = "kldsfh39psdjf239i32+u9f";
            _fixture.SetAccessToken(expectedAccessToken);
            var sut = _fixture.CreateSut();
            
            var accessToken = await sut.GetAccessToken(_fixture.DefaultScopes);
            accessToken.Should().Be(expectedAccessToken);
        }

        [Fact]
        public async Task SendsRequestToTokenEndpoint()
        {
            var tokenEndpoint = "https://test.ks.no/api/token";
            _fixture.Properties.TokenEndpoint = tokenEndpoint;
            
            var sut = _fixture.CreateSut();
            
            await sut.GetAccessToken(_fixture.DefaultScopes);
            
            _fixture.HttpMessageHandleMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get
                        && req.RequestUri == new Uri(tokenEndpoint)
                ),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task DoesNotSendRequestTwiceIfSecondCallIsWithinTimelimit()
        {
            _fixture.Properties.NumberOfSecondsLeftBeforeExpire = 1000;
            var sut = _fixture.CreateSut();
            
            var token1 = await sut.GetAccessToken(_fixture.DefaultScopes);
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            var token2 = await sut.GetAccessToken(_fixture.DefaultScopes);

            token1.Should().Be(token2);
            _fixture.HttpMessageHandleMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get
                ),
                ItExpr.IsAny<CancellationToken>()
            );

        }
    }
}