using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using KS.Fiks.IO.Client.Amqp;
using KS.Fiks.IO.Client.Catalog;
using KS.Fiks.IO.Client.Configuration;
using KS.Fiks.IO.Client.Dokumentlager;
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

        private Konto _lookupReturn = null;
        private SendtMelding _sendtMeldingReturn = null;

        private string _scheme = "http";
        private string _host = "api.fiks.dev.ks";
        private int _port = 80;
        private string _path;
        private string _integrasjonPassword = "default";
        private Guid _integrasjonId = Guid.NewGuid();
        private Guid _accountId = Guid.NewGuid();
        private KatalogConfiguration _katalogConfiguration;

        public FiksIOClientFixture()
        {
            CatalogHandlerMock = new Mock<ICatalogHandler>();
            MaskinportenClientMock = new Mock<IMaskinportenClient>();
            FiksIOSenderMock = new Mock<IFiksIOSender>();
            SendHandlerMock = new Mock<ISendHandler>();
            DokumentlagerHandlerMock = new Mock<IDokumentlagerHandler>();
            AmqpHandlerMock = new Mock<IAmqpHandler>();
        }

        public Mock<IMaskinportenClient> MaskinportenClientMock { get; }

        public Mock<IFiksIOSender> FiksIOSenderMock { get; }

        public Mock<ISendHandler> SendHandlerMock { get; }

        public MeldingRequest DefaultRequest => new MeldingRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "defaultType");

        public FiksIOClient CreateSut()
        {
            SetupConfiguration();
            SetupMocks();
            return new FiksIOClient(
                configuration: _configuration,
                catalogHandler: CatalogHandlerMock.Object,
                maskinportenClient: MaskinportenClientMock.Object,
                sendHandler: SendHandlerMock.Object,
                dokumentlagerHandler: DokumentlagerHandlerMock.Object,
                amqpHandler: AmqpHandlerMock.Object);
        }

        public FiksIOClientFixture WithAccountId(Guid id)
        {
            _accountId = id;
            return this;
        }

        public FiksIOClientFixture WithLookupAccount(Konto konto)
        {
            _lookupReturn = konto;
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

        public FiksIOClientFixture WithSentMessageReturned(SendtMelding message)
        {
            _sendtMeldingReturn = message;
            return this;
        }

        public FiksIOClientFixture WithCatalogConfiguration(KatalogConfiguration configuration)
        {
            _katalogConfiguration = configuration;
            return this;
        }

        internal Mock<ICatalogHandler> CatalogHandlerMock { get; }

        internal Mock<IDokumentlagerHandler> DokumentlagerHandlerMock { get; }

        internal Mock<IAmqpHandler> AmqpHandlerMock { get; }

        private void SetupMocks()
        {
            CatalogHandlerMock.Setup(_ => _.Lookup(It.IsAny<LookupRequest>())).ReturnsAsync(_lookupReturn);
            SendHandlerMock.Setup(_ => _.Send(It.IsAny<MeldingRequest>(), It.IsAny<IList<IPayload>>()))
                           .ReturnsAsync(_sendtMeldingReturn);
            AmqpHandlerMock.Setup(_ => _.AddMessageReceivedHandler(
                It.IsAny<EventHandler<MottattMeldingArgs>>(),
                It.IsAny<EventHandler<ConsumerEventArgs>>()));
        }

        private void SetupConfiguration()
        {
            var apiConfiguration = new ApiConfiguration(_scheme, _host, _port);
            var accountConfiguration = new KontoConfiguration(_accountId, "dummyKey");

            _configuration = new FiksIOConfiguration(
                accountConfiguration,
                new IntegrasjonConfiguration(_integrasjonId, _integrasjonPassword),
                new MaskinportenClientConfiguration("audience", "token", "issuer", 1, new X509Certificate2()),
                apiConfiguration: apiConfiguration,
                katalogConfiguration: _katalogConfiguration);
        }
    }
}