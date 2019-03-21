using System;
using System.Collections.Generic;
using System.IO;
using KS.Fiks.IO.Client.Catalog;
using KS.Fiks.IO.Client.Encryption;
using KS.Fiks.IO.Client.Models;
using KS.Fiks.IO.Client.Send;
using KS.Fiks.IO.Send.Client;
using Moq;

namespace KS.Fiks.IO.Client.Tests.Send
{
    public class SendHandlerFixture
    {
        private string _publicKey = "defaultKey";

        public SendHandlerFixture()
        {
            FiksIOSenderMock = new Mock<IFiksIOSender>();
            PayloadEncrypterMock = new Mock<IPayloadEncrypter>();
            CatalogHandlerMock = new Mock<ICatalogHandler>();
        }

        public SendHandlerFixture WithPublicKey(string publicKey)
        {
            _publicKey = publicKey;
            return this;
        }

        public Mock<IFiksIOSender> FiksIOSenderMock { get; }

        internal Mock<IPayloadEncrypter> PayloadEncrypterMock { get; }

        internal Mock<ICatalogHandler> CatalogHandlerMock { get; }

        internal SendHandler CreateSut()
        {
            SetupMocks();
            return new SendHandler(CatalogHandlerMock.Object, FiksIOSenderMock.Object, PayloadEncrypterMock.Object);
        }

        private void SetupMocks()
        {
            FiksIOSenderMock.Setup(_ => _.Send(
                                It.IsAny<MessageSpecificationApiModel>(),
                                It.IsAny<Stream>()))
                            .ReturnsAsync(new SentMessageApiModel());

            PayloadEncrypterMock.Setup(_ => _.Encrypt(It.IsAny<string>(), It.IsAny<IEnumerable<IPayload>>()))
                                .Returns(Mock.Of<Stream>());

            CatalogHandlerMock.Setup(_ => _.GetPublicKey(It.IsAny<Guid>())).ReturnsAsync(_publicKey);
        }
    }
}