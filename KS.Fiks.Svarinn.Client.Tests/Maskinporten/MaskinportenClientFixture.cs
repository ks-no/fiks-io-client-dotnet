using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Ks.Fiks.Svarinn.Client.Maskinporten;
using Moq;
using Moq.Protected;

namespace Ks.Fiks.Svarinn.ClientTest.Maskinporten
{
    public class MaskinportenClientFixture
    {
        private MaskinportenClientProperties _properties;
        private HttpResponseMessage _responseMessage;

        public MaskinportenClientFixture()
        {
            SetDefaultValues();
            SetupMocks();
        }

        public MaskinportenClient CreateSut()
        {
            return new MaskinportenClient(_properties, new HttpClient(HttpMessageHandleMock.Object));
        }

        public Mock<HttpMessageHandler> HttpMessageHandleMock { get; private set; }

        private void SetDefaultProperties()
        {
            _properties = new MaskinportenClientProperties("testAudience", "testEndpoint", "testIssuer", 1);
        }

        private void SetDefaultValues()
        {
            SetDefaultProperties();
            _responseMessage = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("test content"),
            };
        }
        
        private void SetupMocks()
        {
            HttpMessageHandleMock = new Mock<HttpMessageHandler>();
            HttpMessageHandleMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(_responseMessage)
                .Verifiable();
        }
    }
}