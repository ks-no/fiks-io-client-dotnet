using System;
using System.Collections.Generic;
using System.IO;
using KS.Fiks.IO.Client.Asic;
using KS.Fiks.IO.Client.Catalog;
using KS.Fiks.IO.Client.Encryption;
using KS.Fiks.IO.Client.Models;
using KS.Fiks.IO.Client.Send;
using KS.Fiks.IO.Send.Client;
using Moq;
using Org.BouncyCastle.X509;

namespace KS.Fiks.IO.Client.Tests.Send
{
    public class SendHandlerFixture
    {
        private X509Certificate _publicKey;

        public SendHandlerFixture()
        {
            FiksIOSenderMock = new Mock<IFiksIOSender>();
            AsicEncrypterMock = new Mock<IAsicEncrypter>();
            CatalogHandlerMock = new Mock<ICatalogHandler>();
            _publicKey = CreateTestCertificate();
        }

        public SendHandlerFixture WithPublicKey(X509Certificate publicKey)
        {
            _publicKey = publicKey;
            return this;
        }

        public Mock<IFiksIOSender> FiksIOSenderMock { get; }

        internal X509Certificate CreateTestCertificate()
        {
            return Mock.Of<X509Certificate>();
        }

        internal Mock<IAsicEncrypter> AsicEncrypterMock { get; }

        internal Mock<ICatalogHandler> CatalogHandlerMock { get; }

        internal SendHandler CreateSut()
        {
            SetupMocks();
            return new SendHandler(CatalogHandlerMock.Object, FiksIOSenderMock.Object, AsicEncrypterMock.Object);
        }

        private void SetupMocks()
        {
            FiksIOSenderMock.Setup(_ => _.Send(
                                It.IsAny<MessageSpecificationApiModel>(),
                                It.IsAny<Stream>()))
                            .ReturnsAsync(new SentMessageApiModel());

            AsicEncrypterMock.Setup(_ => _.Encrypt(It.IsAny<X509Certificate>(), It.IsAny<IEnumerable<IPayload>>()))
                                .Returns(Mock.Of<Stream>());

            CatalogHandlerMock.Setup(_ => _.GetPublicKey(It.IsAny<Guid>())).ReturnsAsync(_publicKey);
        }
    }
}