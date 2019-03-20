using System;
using System.Collections.Generic;
using System.IO;
using KS.Fiks.IO.Client.Catalog;
using KS.Fiks.IO.Client.Encryption;
using KS.Fiks.IO.Client.Models;
using KS.Fiks.IO.Client.Send;
using KS.Fiks.Io.Send.Client;
using Moq;

namespace KS.Fiks.IO.Client.Tests.Send
{
    public class SendHandlerFixture
    {
        private Stream _encryptedStream;

        private string _publicKey = "defaultKey";

        public SendHandlerFixture()
        {
            FiksIOSenderMock = new Mock<IFiksIOSender>();
            PayloadEncrypterMock = new Mock<IPayloadEncrypter>();
            CatalogHandlerMock = new Mock<ICatalogHandler>();
        }

        public Mock<IFiksIOSender> FiksIOSenderMock { get; }

        public Mock<IPayloadEncrypter> PayloadEncrypterMock { get; }

        public Mock<ICatalogHandler> CatalogHandlerMock { get; }

        public SendHandler CreateSut()
        {
            SetupMocks();
            return new SendHandler(FiksIOSenderMock.Object, PayloadEncrypterMock.Object, CatalogHandlerMock.Object);
        }

        public SendHandlerFixture WithPublicKey(string publicKey)
        {
            _publicKey = publicKey;
            return this;
        }

        private void SetupMocks()
        {
            FiksIOSenderMock.Setup(_ => _.Send(
                                It.IsAny<MessageSpecificationApiModel>(),
                                It.IsAny<Stream>()))
                            .ReturnsAsync(new SentMessageApiModel());

            PayloadEncrypterMock.Setup(_ => _.Encrypt(It.IsAny<string>(), It.IsAny<IEnumerable<IPayload>>()))
                                .Returns(_encryptedStream);

            CatalogHandlerMock.Setup(_ => _.GetPublicKey(It.IsAny<Guid>())).ReturnsAsync(_publicKey);
        }
    }
}