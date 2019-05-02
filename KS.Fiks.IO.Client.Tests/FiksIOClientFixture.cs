using System;
using System.Collections.Generic;
using KS.Fiks.IO.Client.Amqp;
using KS.Fiks.IO.Client.Catalog;
using KS.Fiks.IO.Client.Configuration;
using KS.Fiks.IO.Client.Models;
using KS.Fiks.IO.Client.Send;
using KS.Fiks.IO.Send.Client;
using Ks.Fiks.Maskinporten.Client;
using Moq;
using RabbitMQ.Client.Events;

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
        private Guid _integrasjonId = Guid.NewGuid();
        private string _accountId = "defaultId";
        private CatalogConfiguration _catalogConfiguration;

        public FiksIOClientFixture()
        {
            CatalogHandlerMock = new Mock<ICatalogHandler>();
            MaskinportenClientMock = new Mock<IMaskinportenClient>();
            FiksIOSenderMock = new Mock<IFiksIOSender>();
            SendHandlerMock = new Mock<ISendHandler>();
            AmqpHandlerMock = new Mock<IAmqpHandler>();
        }

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
                SendHandlerMock.Object,
                AmqpHandlerMock.Object);
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

        internal Mock<ICatalogHandler> CatalogHandlerMock { get; }

        internal Mock<IAmqpHandler> AmqpHandlerMock { get; }

        private void SetupMocks()
        {
            CatalogHandlerMock.Setup(_ => _.Lookup(It.IsAny<LookupRequest>())).ReturnsAsync(_lookupReturn);
            SendHandlerMock.Setup(_ => _.Send(It.IsAny<MessageRequest>(), It.IsAny<IList<IPayload>>()))
                           .ReturnsAsync(_sentMessageReturn);
            AmqpHandlerMock.Setup(_ => _.AddMessageReceivedHandler(
                It.IsAny<EventHandler<MessageReceivedArgs>>(),
                It.IsAny<EventHandler<ConsumerEventArgs>>()));
        }

        private void SetupConfiguration()
        {
            var apiConfiguration = new FiksApiConfiguration(_scheme, _host, _port);
            var accountConfiguration = new AccountConfiguration(_accountId, "dummyKey");

            _configuration = new FiksIOConfiguration(
                accountConfiguration,
                new FiksIntegrationConfiguration(_integrasjonId, _integrasjonPassword),
                new MaskinportenClientConfiguration(),
                fiksApiConfiguration: apiConfiguration,
                catalogConfiguration: _catalogConfiguration);
        }
    }
}