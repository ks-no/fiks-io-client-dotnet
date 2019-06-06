using System;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using KS.Fiks.IO.Client.Catalog;
using KS.Fiks.IO.Client.Configuration;
using KS.Fiks.IO.Client.Models;
using Ks.Fiks.Maskinporten.Client;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;

namespace KS.Fiks.IO.Client.Tests.Catalog
{
    public class CatalogHandlerFixture
    {
        private HttpStatusCode _statusCode = HttpStatusCode.OK;
        private KatalogKonto _katalogKonto;
        private KontoOffentligNokkel _kontoOffentligNokkel;
        private string _scheme = "http";
        private string _host = "api.fiks.dev.ks";
        private int _port = 80;
        private string _path = "/svarinn2/katalog/api/v1";
        private string _accessToken = "token";
        private string _integrasjonPassword = "default";
        private Guid _integrasjonId;

        public CatalogHandlerFixture()
        {
            HttpMessageHandleMock = new Mock<HttpMessageHandler>();
            MaskinportenClientMock = new Mock<IMaskinportenClient>();
            SetDefaultValues();
        }

        internal CatalogHandler CreateSut()
        {
            SetupMocks();
            return new CatalogHandler(
                CreateConfiguration().KatalogConfiguration,
                CreateConfiguration().IntegrasjonConfiguration,
                MaskinportenClientMock.Object,
                new HttpClient(HttpMessageHandleMock.Object));
        }

        public Mock<HttpMessageHandler> HttpMessageHandleMock { get; }

        public Mock<IMaskinportenClient> MaskinportenClientMock { get; }

        public LookupRequest DefaultLookupRequest => new LookupRequest(
            "defaultIdentifier",
            "defaultMessageType",
            0);

        public CatalogHandlerFixture WithStatusCode(HttpStatusCode code)
        {
            _statusCode = code;
            return this;
        }

        public CatalogHandlerFixture WithAccountResponse(KatalogKonto response)
        {
            _katalogKonto = response;
            return this;
        }

        public CatalogHandlerFixture WithPublicKeyResponse(KontoOffentligNokkel response)
        {
            _kontoOffentligNokkel = response;
            return this;
        }

        public CatalogHandlerFixture WithScheme(string scheme)
        {
            _scheme = scheme;
            return this;
        }

        public CatalogHandlerFixture WithHost(string host)
        {
            _host = host;
            return this;
        }

        public CatalogHandlerFixture WithPort(int port)
        {
            _port = port;
            return this;
        }

        public CatalogHandlerFixture WithPath(string path)
        {
            _path = path;
            return this;
        }

        public CatalogHandlerFixture WithAccessToken(string token)
        {
            _accessToken = token;
            return this;
        }

        public CatalogHandlerFixture WithIntegrasjonId(Guid id)
        {
            _integrasjonId = id;
            return this;
        }

        public CatalogHandlerFixture WithIntegrasjonPassword(string password)
        {
            _integrasjonPassword = password;
            return this;
        }

        public KontoOffentligNokkel CreateDefaultPublicKey()
        {
            return new KontoOffentligNokkel
            {
                IssuerDN = "C=AU",
                Nokkel = "-----BEGIN CERTIFICATE-----\n"+
                "MIICLDCCAdKgAwIBAgIBADAKBggqhkjOPQQDAjB9MQswCQYDVQQGEwJCRTEPMA0G\n"+
                "A1UEChMGR251VExTMSUwIwYDVQQLExxHbnVUTFMgY2VydGlmaWNhdGUgYXV0aG9y\n"+
                "aXR5MQ8wDQYDVQQIEwZMZXV2ZW4xJTAjBgNVBAMTHEdudVRMUyBjZXJ0aWZpY2F0\n"+
                "ZSBhdXRob3JpdHkwHhcNMTEwNTIzMjAzODIxWhcNMTIxMjIyMDc0MTUxWjB9MQsw\n"+
                "CQYDVQQGEwJCRTEPMA0GA1UEChMGR251VExTMSUwIwYDVQQLExxHbnVUTFMgY2Vy\n"+
                "dGlmaWNhdGUgYXV0aG9yaXR5MQ8wDQYDVQQIEwZMZXV2ZW4xJTAjBgNVBAMTHEdu\n"+
                "dVRMUyBjZXJ0aWZpY2F0ZSBhdXRob3JpdHkwWTATBgcqhkjOPQIBBggqhkjOPQMB\n"+
                "BwNCAARS2I0jiuNn14Y2sSALCX3IybqiIJUvxUpj+oNfzngvj/Niyv2394BWnW4X\n"+
                "uQ4RTEiywK87WRcWMGgJB5kX/t2no0MwQTAPBgNVHRMBAf8EBTADAQH/MA8GA1Ud\n"+
                "DwEB/wQFAwMHBgAwHQYDVR0OBBYEFPC0gf6YEr+1KLlkQAPLzB9mTigDMAoGCCqG\n"+
                "SM49BAMCA0gAMEUCIDGuwD1KPyG+hRf88MeyMQcqOFZD0TbVleF+UsAGQ4enAiEA\n"+
                "l4wOuDwKQa+upc8GftXE2C//4mKANBC6It01gUaTIpo=\n"+
                "-----END CERTIFICATE-----",
                Serial = "500",
                SubjectDN = "C=AU",
                ValidFrom = DateTime.Now,
                ValidTo = DateTime.Now.Add(TimeSpan.FromDays(2))
            };
        }

        private void SetDefaultValues()
        {
            _integrasjonId = Guid.NewGuid();
        }

        private void SetupMocks()
        {
            SetHttpResponse();
            MaskinportenClientMock.Setup(_ => _.GetAccessToken(It.IsAny<string>()))
                                  .ReturnsAsync(new MaskinportenToken(_accessToken, 1000));
        }

        private void SetHttpResponse()
        {
            var responseMessage = new HttpResponseMessage()
            {
                StatusCode = _statusCode,
                Content = GetDefaultContent()
            };

            HttpMessageHandleMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage)
                .Verifiable();
        }

        private StringContent GetDefaultContent()
        {
            if (_katalogKonto != null)
            {
                return new StringContent(JsonConvert.SerializeObject(_katalogKonto));
            }

            if (_kontoOffentligNokkel != null)
            {
                return new StringContent(JsonConvert.SerializeObject(_kontoOffentligNokkel));
            }

            var responseAsJsonString = @"{" +
                                       "\"fiksOrgId\": \"3fa85f64-5717-4562-b3fc-2c963f66afa6\"," +
                                       "\"fiksOrgNavn\": \"string\"," +
                                       "\"kontoId\": \"3fa85f64-5717-4562-b3fc-2c963f66afa6\"," +
                                       "\"kontoNavn\": \"string\"," +
                                       "\"status\": {" +
                                       "\"gyldigAvsender\": true," +
                                       "\"gyldigMottaker\": true," +
                                       "\"melding\": \"string\"" +
                                       "}" +
                                       "}";
            return new StringContent(responseAsJsonString);
        }

        private FiksIOConfiguration CreateConfiguration()
        {
            var catalogConfiguration = new KatalogConfiguration(_path, _scheme, _host, _port);

            var apiConfiguration = new ApiConfiguration(_scheme, _host, _port);
            var accountConfiguration = new KontoConfiguration(Guid.NewGuid(), "privateKey");

            return new FiksIOConfiguration(
                accountConfiguration,
                new IntegrasjonConfiguration(_integrasjonId, _integrasjonPassword),
                new MaskinportenClientConfiguration("audience", "token", "issuer", 1, new X509Certificate2()),
                apiConfiguration: apiConfiguration,
                katalogConfiguration: catalogConfiguration);
        }
    }
}