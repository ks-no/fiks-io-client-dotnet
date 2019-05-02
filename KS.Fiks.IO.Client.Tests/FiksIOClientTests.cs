using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using KS.Fiks.IO.Client.Models;
using Moq;
using RabbitMQ.Client.Events;
using Xunit;

namespace KS.Fiks.IO.Client.Tests
{
    public class FiksIOClientTests
    {
        private FiksIOClientFixture _fixture;

        public FiksIOClientTests()
        {
            _fixture = new FiksIOClientFixture();
        }

        [Fact]
        public void HasExpectedAccountId()
        {
            var expectedAccountId = "TestId";
            var sut = _fixture.WithAccountId(expectedAccountId).CreateSut();
            var actualAccountId = sut.AccountId;
            actualAccountId.Should().Be(expectedAccountId);
        }

        [Fact]
        public async Task LookupReturnsExpectedAccount()
        {
            var expectedAccount = new Account
            {
                AccountId = Guid.NewGuid(),
                AccountName = "testName",
                IsValidSender = true,
                IsValidReceiver = false,
                OrgId = Guid.NewGuid(),
                OrgName = "testOrgName"
            };
            var lookup = new LookupRequest();
            var sut = _fixture.WithLookupAccount(expectedAccount).CreateSut();
            var actualAccount = await sut.Lookup(lookup).ConfigureAwait(false);
            actualAccount.Should().Be(expectedAccount);
        }

        [Fact]
        public async Task LookupCallsCatalogHandlerWithExpectedLookup()
        {
            var lookup = new LookupRequest
            {
                Identifier = "testIdentifier",
                AccessLevel = 3,
                MessageType = "testType"
            };
            var sut = _fixture.WithLookupAccount(new Account()).CreateSut();
            var actualAccount = await sut.Lookup(lookup).ConfigureAwait(false);

            _fixture.CatalogHandlerMock.Verify(_ => _.Lookup(lookup));
        }

        [Fact]
        public async Task SendCallsSendHandlerWithList()
        {
            var sut = _fixture.CreateSut();

            var request = new MessageRequest();

            var payload = new List<IPayload>();
            payload.Add(Mock.Of<IPayload>());

            var result = await sut.Send(request, payload).ConfigureAwait(false);

            _fixture.SendHandlerMock.Verify(_ => _.Send(request, payload));
        }

        [Fact]
        public async Task SendCallsSendHandlerAsPayloadList()
        {
            var sut = _fixture.CreateSut();

            var request = new MessageRequest();

            var stream = Mock.Of<Stream>();
            var filename = "filename.file";

            var result = await sut.Send(request, stream, filename).ConfigureAwait(false);

            _fixture.SendHandlerMock.Verify(_ => _.Send(
                request,
                It.Is<IList<IPayload>>(actualPayload =>
                    actualPayload.Count() == 1 &&
                    actualPayload.FirstOrDefault().Payload == stream &&
                    actualPayload.FirstOrDefault().Filename == filename)));
        }

        [Fact]
        public async Task SendCallsSendHandlerWithString()
        {
            var sut = _fixture.CreateSut();

            var request = new MessageRequest();

            var payload = "string payload";
            var filename = "filename.txt";

            var result = await sut.Send(request, payload, filename).ConfigureAwait(false);

            _fixture.SendHandlerMock.Verify(_ => _.Send(
                request,
                It.Is<IList<IPayload>>(actualPayload =>
                    actualPayload.Count() == 1 &&
                    actualPayload.FirstOrDefault().Payload.Length == payload.Length &&
                    actualPayload.FirstOrDefault().Filename == filename)));
        }

        [Fact]
        public async Task SendCallsSendHandlerWithFile()
        {
            var sut = _fixture.CreateSut();

            var request = new MessageRequest();

            var filename = "testfile.txt";
            var path = $"{filename}";

            var result = await sut.Send(request, path).ConfigureAwait(false);

            _fixture.SendHandlerMock.Verify(_ => _.Send(
                request,
                It.Is<IList<IPayload>>(actualPayload =>
                    actualPayload.Count() == 1 &&
                    actualPayload.FirstOrDefault().Filename == filename)));
        }

        [Fact]
        public async Task SendReturnsExpectedSentMessage()
        {
            var expectedMessage = new SentMessage
            {
                MessageId = Guid.NewGuid(),
                MessageType = "msgType",
                ReceiverAccountId = Guid.NewGuid(),
                SenderAccountId = Guid.NewGuid(),
                Ttl = TimeSpan.FromDays(1)
            };
            var sut = _fixture.WithSentMessageReturned(expectedMessage).CreateSut();

            var payload = new List<IPayload>();
            payload.Add(Mock.Of<IPayload>());

            var request = new MessageRequest();

            var result = await sut.Send(request, payload).ConfigureAwait(false);

            result.Should().Be(expectedMessage);
        }

        [Fact]
        public void NewSubscriptionCallsAmqpHandlerWithOnReceived()
        {
            var sut = _fixture.CreateSut();

            var onReceived = new EventHandler<MessageReceivedArgs>((a, b) => { });

            sut.NewSubscription(onReceived);

            _fixture.AmqpHandlerMock.Verify(_ => _.AddMessageReceivedHandler(onReceived, null));
        }

        [Fact]
        public void NewSubscriptionCallsAmqpHandlerWithOnReceivedAndOnCanceled()
        {
            var sut = _fixture.CreateSut();

            var onReceived = new EventHandler<MessageReceivedArgs>((a, b) => { });

            var onCanceled = new EventHandler<ConsumerEventArgs>((a, b) => { });

            sut.NewSubscription(onReceived, onCanceled);

            _fixture.AmqpHandlerMock.Verify(_ => _.AddMessageReceivedHandler(onReceived, onCanceled));
        }
    }
}