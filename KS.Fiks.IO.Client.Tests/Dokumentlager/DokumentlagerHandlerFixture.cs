using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using KS.Fiks.IO.Client.Configuration;
using KS.Fiks.IO.Client.Dokumentlager;
using Moq;
using Moq.Protected;

namespace KS.Fiks.IO.Client.Tests.Dokumentlager
{
    internal class DokumentlagerHandlerFixture
    {
        private DokumentlagerConfiguration _configuration;

        private HttpStatusCode _statusCode = HttpStatusCode.OK;

        private string _scheme;

        private string _host;

        private string _downloadPath;

        private int _port;

        public DokumentlagerHandlerFixture()
        {
            HttpMessageHandleMock = new Mock<HttpMessageHandler>();

        }

        public Mock<HttpMessageHandler> HttpMessageHandleMock { get; }
        
        public Uri RequestUri { get; private set; }

        public DokumentlagerHandler CreateSut()
        {
            SetupConfiguration();
            SetupMocks();
            return new DokumentlagerHandler(_configuration, new HttpClient(HttpMessageHandleMock.Object));
        }

        public DokumentlagerHandlerFixture WithSchema(string scheme)
        {
            _scheme = scheme;
            return this;
        }

        public DokumentlagerHandlerFixture WithHost(string host)
        {
            _host = host;
            return this;
        }

        public DokumentlagerHandlerFixture WithPort(int port)
        {
            _port = port;
            return this;
        }

        public DokumentlagerHandlerFixture WithDownloadPath(string path)
        {
            _downloadPath = path;
            return this;
        }

        private void SetupConfiguration()
        {
            _configuration = new DokumentlagerConfiguration(scheme: _scheme, host: _host, port: _port, downloadPath: _downloadPath);
        }

        private void SetupMocks()
        {
            var responseMessage = new HttpResponseMessage()
            {
                StatusCode = _statusCode
            };

            HttpMessageHandleMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage,CancellationToken>((req, c) => RequestUri = req.RequestUri)
                .ReturnsAsync(responseMessage)
                .Verifiable();
        }
    }
}