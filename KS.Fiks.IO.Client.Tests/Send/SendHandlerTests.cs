using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using KS.Fiks.IO.Client.Models;
using KS.Fiks.IO.Send.Client;
using Moq;
using Xunit;

namespace KS.Fiks.IO.Client.Tests.Send
{
    public class SendHandlerTests
    {
        private SendHandlerFixture _fixture;

        public SendHandlerTests()
        {
            _fixture = new SendHandlerFixture();
        }

        [Fact]
        public async Task CallsFiksIOSenderSend()
        {
            var sut = _fixture.CreateSut();

            var request = new MessageRequest();

            var payload = new List<IPayload>();

            await sut.Send(request, payload).ConfigureAwait(false);

            _fixture.FiksIOSenderMock.Verify(_ => _.Send(It.IsAny<MessageSpecificationApiModel>(), It.IsAny<Stream>()));
        }

        [Fact]
        public async Task CallsSendWithExpectedMessageSpecificationApiModel()
        {
            var sut = _fixture.CreateSut();

            var request = new MessageRequest
            {
                SenderAccountId = Guid.NewGuid(),
                MessageType = "MessageType",
                ReceiverAccountId = Guid.NewGuid(),
                SvarPaMelding = Guid.NewGuid(),
                Ttl = TimeSpan.FromDays(2)
            };

            var payload = new List<IPayload>();

            await sut.Send(request, payload).ConfigureAwait(false);

            _fixture.FiksIOSenderMock.Verify(_ => _.Send(
                It.Is<MessageSpecificationApiModel>(
                    model => model.Ttl == (long)request.Ttl.TotalMilliseconds &&
                             model.MeldingType == request.MessageType &&
                             model.SvarPaMelding == request.SvarPaMelding &&
                             model.AvsenderKontoId == request.SenderAccountId &&
                             model.MottakerKontoId == request.ReceiverAccountId),
                It.IsAny<Stream>()));
        }

        [Fact]
        public async Task CallsEncrypterWithExpectedPrivateKey()
        {
            var expectedPublicKey = _fixture.CreateTestCertificate();
            var sut = _fixture.WithPublicKey(expectedPublicKey).CreateSut();
            var request = new MessageRequest();
            var payload = Mock.Of<IEnumerable<IPayload>>();

            await sut.Send(request, payload).ConfigureAwait(false);

            _fixture.PayloadEncrypterMock.Verify(_ => _.Encrypt(expectedPublicKey, payload), Times.Once());
        }

        [Fact]
        public async Task GetsKeyFromCatalogHandler()
        {
            var sut = _fixture.CreateSut();
            var request = new MessageRequest
            {
                ReceiverAccountId = Guid.NewGuid()
            };
            var payload = Mock.Of<IEnumerable<IPayload>>();

            await sut.Send(request, payload).ConfigureAwait(false);

            _fixture.CatalogHandlerMock.Verify(_ => _.GetPublicKey(request.ReceiverAccountId), Times.Once());
        }
    }
}