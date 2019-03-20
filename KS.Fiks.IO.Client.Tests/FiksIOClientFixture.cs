using System.Collections.Generic;
using KS.Fiks.IO.Client.Catalog;
using KS.Fiks.IO.Client.Configuration;
using KS.Fiks.IO.Client.Models;
using KS.Fiks.IO.Client.Send;
using KS.Fiks.Io.Send.Client;
using Ks.Fiks.Maskinporten.Client;
using Moq;

namespace KS.Fiks.IO.Client.Tests
{
    public class FiksIOClientFixture
    {
        private FiksIOConfiguration _configuration;

        private Account _lookupReturn = null;
        private SentMessage _sentMessageReturn = null;

        private string _scheme = "http";
        private string _host = "api.fiks.dev.ks";
        private int _port = 80;
        private string _path = "/svarinn2/katalog/api/v1";
        private string _accessToken = "token";
        private string _integrasjonPassword = "default";
        private string _accountId = "defaultId";
        private CatalogConfiguration _catalogConfiguration;

        public FiksIOClientFixture()
        {
            CatalogHandlerMock = new Mock<ICatalogHandler>();
            MaskinportenClientMock = new Mock<IMaskinportenClient>();
            FiksIOSenderMock = new Mock<IFiksIOSender>();
            SendHandlerMock = new Mock<ISendHandler>();
        }

        public Mock<ICatalogHandler> CatalogHandlerMock { get; }

        public Mock<IMaskinportenClient> MaskinportenClientMock { get; }

        public Mock<IFiksIOSender> FiksIOSenderMock { get; }

        public Mock<ISendHandler> SendHandlerMock { get; }

        public FiksIOClient CreateSut()
        {
            SetupConfiguration();
            SetupMocks();
            return new FiksIOClient(
                _configuration,
                CatalogHandlerMock.Object,
                MaskinportenClientMock.Object,
                SendHandlerMock.Object);
        }

        public FiksIOClientFixture WithAccountId(string id)
        {
            _accountId = id;
            return this;
        }

        public FiksIOClientFixture WithLookupAccount(Account account)
        {
            _lookupReturn = account;
            return this;
        }

        public FiksIOClientFixture WithScheme(string scheme)
        {
            _scheme = scheme;
            return this;
        }

        public FiksIOClientFixture WithHost(string host)
        {
            _host = host;
            return this;
        }

        public FiksIOClientFixture WithPort(int port)
        {
            _port = port;
            return this;
        }

        public FiksIOClientFixture WithPath(string path)
        {
            _path = path;
            return this;
        }

        public FiksIOClientFixture WithSentMessageReturned(SentMessage message)
        {
            _sentMessageReturn = message;
            return this;
        }

        public FiksIOClientFixture WithCatalogConfiguration(CatalogConfiguration configuration)
        {
            _catalogConfiguration = configuration;
            return this;
        }

        private void SetupMocks()
        {
            CatalogHandlerMock.Setup(_ => _.Lookup(It.IsAny<LookupRequest>())).ReturnsAsync(_lookupReturn);
            SendHandlerMock.Setup(_ => _.Send(It.IsAny<MessageRequest>(), It.IsAny<IEnumerable<IPayload>>()))
                           .ReturnsAsync(_sentMessageReturn);
        }

        private void SetupConfiguration()
        {
            _configuration = new FiksIOConfiguration
            {
                Host = _host,
                Port = _port,
                Scheme = _scheme,
                Path = _path,
                AccountConfiguration = new AccountConfiguration
                {
                    AccountId = _accountId
                },
                CatalogConfiguration = _catalogConfiguration
            };
        }
    }
}