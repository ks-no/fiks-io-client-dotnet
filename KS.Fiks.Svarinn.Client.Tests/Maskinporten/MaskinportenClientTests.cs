using System;
using System.Linq;
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
                    req.RequestUri == new Uri(tokenEndpoint)
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
                ItExpr.Is<HttpRequestMessage>(req => true),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task SendsGrantTypeInPost()
        {
            var sut = _fixture.CreateSut();

            await sut.GetAccessToken(_fixture.DefaultScopes);

            _fixture.HttpMessageHandleMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    TestHelper.RequestContentAsDictionary(req).ContainsKey("grant_type")
                ),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task SendsAssertionInPost()
        {
            var sut = _fixture.CreateSut();

            await sut.GetAccessToken(_fixture.DefaultScopes);

            _fixture.HttpMessageHandleMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    TestHelper.RequestContentAsDictionary(req).ContainsKey("assertion")
                ),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task SendsHeaderCharsetUtf8()
        {
            var sut = _fixture.CreateSut();

            await sut.GetAccessToken(_fixture.DefaultScopes);

            _fixture.HttpMessageHandleMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Headers.GetValues("Charset").Contains("utf-8")
                ),
                ItExpr.IsAny<CancellationToken>()
            );
        }
        
        
        [Fact]
        public async Task SendsHeaderCorrectContentType()
        {
            var sut = _fixture.CreateSut();

            await sut.GetAccessToken(_fixture.DefaultScopes);

            _fixture.HttpMessageHandleMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Content.Headers.ContentType.MediaType =="application/x-www-form-urlencoded"
                ),
                ItExpr.IsAny<CancellationToken>()
            );
        }
    }
}