using System;
using System.Net;
using System.Net.Http;
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
        private AccountResponse _accountResponse;
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
                CreateConfiguration().CatalogConfiguration,
                CreateConfiguration().FiksIntegrationConfiguration,
                MaskinportenClientMock.Object,
                new HttpClient(HttpMessageHandleMock.Object));
        }

        public Mock<HttpMessageHandler> HttpMessageHandleMock { get; }

        public Mock<IMaskinportenClient> MaskinportenClientMock { get; }

        public LookupRequest DefaultLookupRequest => new LookupRequest
        {
            Identifier = "defaultIdentifier",
            MessageType = "defaultMessageType",
            AccessLevel = 0
        };

        public CatalogHandlerFixture WithStatusCode(HttpStatusCode code)
        {
            _statusCode = code;
            return this;
        }

        public CatalogHandlerFixture WithAccountResponse(AccountResponse response)
        {
            _accountResponse = response;
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
            if (_accountResponse != null)
            {
                return new StringContent(JsonConvert.SerializeObject(_accountResponse));
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
            var catalogConfiguration = new CatalogConfiguration(_path, _scheme, _host, _port);

            var apiConfiguration = new FiksApiConfiguration(_scheme, _host, _port);
            var accountConfiguration = new AccountConfiguration("notUsedId");

            return new FiksIOConfiguration(
                accountConfiguration,
                new FiksIntegrationConfiguration(_integrasjonId, _integrasjonPassword),
                new MaskinportenClientConfiguration(),
                fiksApiConfiguration: apiConfiguration,
                catalogConfiguration: catalogConfiguration);
        }
    }
}