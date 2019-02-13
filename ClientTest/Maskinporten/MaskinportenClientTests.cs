using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NFluent;
using NUnit.Framework;
using Moq.Protected;


namespace Ks.Fiks.Svarinn.ClientTest.Maskinporten
{
    public class TestBase
    {
        protected MaskinportenClientFixture Fixture;
        
        [SetUp]
        public void Setup()
        {
            Fixture = new MaskinportenClientFixture();
        }
    }

    [TestFixture]
    public class GetAccessToken : TestBase
    {
        [Test]
        public async Task ReturnsAString()
        {
            var sut = Fixture.CreateSut();
            var scopes = new List<string>();
            
            var accessToken = await sut.GetAccessToken(scopes);
            
            Check.That(accessToken).IsInstanceOf<string>();
        }

        [Test]
        public async Task CallsSendAsyncExactlyOnce()
        {
            var sut = Fixture.CreateSut();
            var scopes = new List<string>();
            
            await sut.GetAccessToken(scopes);

            Fixture.HttpMessageHandleMock.Protected().Verify(
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