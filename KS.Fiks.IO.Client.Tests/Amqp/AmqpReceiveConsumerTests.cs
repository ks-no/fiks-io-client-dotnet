using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using KS.Fiks.IO.Client.Asic;
using KS.Fiks.IO.Client.Exceptions;
using KS.Fiks.IO.Client.FileIO;
using KS.Fiks.IO.Client.Models;
using Moq;
using RabbitMQ.Client;
using Xunit;

namespace KS.Fiks.IO.Client.Tests.Amqp
{
    public class AmqpReceiveConsumerTests
    {
        private AmqpReceiveConsumerFixture _fixture;

        public AmqpReceiveConsumerTests()
        {
            _fixture = new AmqpReceiveConsumerFixture();
        }

        [Fact]
        public void ReceivedHandler()
        {
            var sut = _fixture.CreateSut();

            var hasBeenCalled = false;
            var handler = new EventHandler<MessageReceivedArgs>((a, _) => { hasBeenCalled = true; });

            sut.Received += handler;

            sut.HandleBasicDeliver(
                "tag",
                34,
                false,
                "exchange",
                Guid.NewGuid().ToString(),
                _fixture.DefaultProperties,
                Array.Empty<byte>());

            hasBeenCalled.Should().BeTrue();
        }

        [Fact]
        public void ReceivesExpectedMessageMetadata()
        {
            var expectedMessageMetadata = _fixture.DefaultMetadata;

            var headers = new Dictionary<string, object>
            {
                {"avsender-id", Encoding.UTF8.GetBytes(expectedMessageMetadata.SenderAccountId.ToString()) },
                {"melding-id", Encoding.UTF8.GetBytes(expectedMessageMetadata.MessageId.ToString()) },
                {"type", Encoding.UTF8.GetBytes(expectedMessageMetadata.MessageType) },
                {"svar-til", Encoding.UTF8.GetBytes(expectedMessageMetadata.RelatedMessageId.ToString()) }
            };

            var propertiesMock = new Mock<IBasicProperties>();
            propertiesMock.Setup(_ => _.Headers).Returns(headers);
            propertiesMock.Setup(_ => _.Expiration)
                          .Returns(
                              expectedMessageMetadata.Ttl.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));

            var sut = _fixture.CreateSut();
            IReceivedMessage actualMessage = new ReceivedMessage(
                _fixture.DefaultMetadata,
                () => Task.FromResult((Stream)new MemoryStream(new byte[1])),
                Mock.Of<IAsicDecrypter>(),
                Mock.Of<IFileWriter>());
            var handler = new EventHandler<MessageReceivedArgs>((a, messageArgs) =>
            {
                actualMessage = messageArgs.Message;
            });

            sut.Received += handler;

            sut.HandleBasicDeliver(
                "tag",
                34,
                false,
                "exchange",
                expectedMessageMetadata.ReceiverAccountId.ToString(),
                propertiesMock.Object,
                Array.Empty<byte>());

            actualMessage.MessageId.Should().Be(expectedMessageMetadata.MessageId);
            actualMessage.MessageType.Should().Be(expectedMessageMetadata.MessageType);
            actualMessage.ReceiverAccountId.Should().Be(expectedMessageMetadata.ReceiverAccountId);
            actualMessage.SenderAccountId.Should().Be(expectedMessageMetadata.SenderAccountId);
            actualMessage.RelatedMessageId.Should().Be(expectedMessageMetadata.RelatedMessageId);
            actualMessage.Ttl.Should().Be(expectedMessageMetadata.Ttl);
        }

        [Fact]
        public void ThrowsParseExceptionIfMessageIdIsNotValidGuid()
        {
            var expectedMessageMetadata = _fixture.DefaultMetadata;

            var headers = new Dictionary<string, object>
            {
                {"avsender-id", Encoding.UTF8.GetBytes(expectedMessageMetadata.SenderAccountId.ToString()) },
                {"melding-id", Encoding.UTF8.GetBytes("NoTGuid") },
                {"type", Encoding.UTF8.GetBytes(expectedMessageMetadata.MessageType) },
                {"svar-til", Encoding.UTF8.GetBytes(expectedMessageMetadata.RelatedMessageId.ToString()) }
            };

            var propertiesMock = new Mock<IBasicProperties>();
            propertiesMock.Setup(_ => _.Headers).Returns(headers);
            propertiesMock.Setup(_ => _.Expiration)
                          .Returns(
                              expectedMessageMetadata.Ttl.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));

            var sut = _fixture.CreateSut();
            var handler = new EventHandler<MessageReceivedArgs>((a, messageArgs) => { });

            sut.Received += handler;
            Assert.Throws<FiksIOParseException>(() =>
            {
                sut.HandleBasicDeliver(
                    "tag",
                    34,
                    false,
                    "exchange",
                    expectedMessageMetadata.ReceiverAccountId.ToString(),
                    propertiesMock.Object,
                    Array.Empty<byte>());
            });
        }

        [Fact]
        public void ThrowsMissingHeaderExceptionExceptionIfHeaderIsNull()
        {
            var expectedMessageMetadata = _fixture.DefaultMetadata;

            var propertiesMock = new Mock<IBasicProperties>();
            propertiesMock.Setup(_ => _.Headers).Returns((IDictionary<string, object>)null);
            propertiesMock.Setup(_ => _.Expiration)
                          .Returns(
                              expectedMessageMetadata.Ttl.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));

            var sut = _fixture.CreateSut();
            var handler = new EventHandler<MessageReceivedArgs>((a, messageArgs) => { });

            sut.Received += handler;
            Assert.Throws<FiksIOMissingHeaderException>(() =>
            {
                sut.HandleBasicDeliver(
                    "tag",
                    34,
                    false,
                    "exchange",
                    expectedMessageMetadata.ReceiverAccountId.ToString(),
                    propertiesMock.Object,
                    Array.Empty<byte>());
            });
        }

        [Fact]
        public void ThrowsMissingHeaderExceptionExceptionIfMessageIdIsMissing()
        {
            var expectedMessageMetadata = _fixture.DefaultMetadata;

            var headers = new Dictionary<string, object>
            {
                {"avsender-id", Encoding.UTF8.GetBytes(expectedMessageMetadata.SenderAccountId.ToString()) },
                {"type", Encoding.UTF8.GetBytes(expectedMessageMetadata.MessageType) },
                {"svar-til", Encoding.UTF8.GetBytes(expectedMessageMetadata.RelatedMessageId.ToString()) }
            };

            var propertiesMock = new Mock<IBasicProperties>();
            propertiesMock.Setup(_ => _.Headers).Returns(headers);
            propertiesMock.Setup(_ => _.Expiration)
                          .Returns(
                              expectedMessageMetadata.Ttl.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));

            var sut = _fixture.CreateSut();
            var handler = new EventHandler<MessageReceivedArgs>((a, messageArgs) => { });

            sut.Received += handler;
            Assert.Throws<FiksIOMissingHeaderException>(() =>
            {
                sut.HandleBasicDeliver(
                    "tag",
                    34,
                    false,
                    "exchange",
                    expectedMessageMetadata.ReceiverAccountId.ToString(),
                    propertiesMock.Object,
                    Array.Empty<byte>());
            });
        }

        [Fact]
        public void FileWriterWriteIsCalledWhenWriteEncryptedZip()
        {
            var sut = _fixture.CreateSut();

            var filePath = "/my/path/something.zip";
            var data = new[] {default(byte) };

            var handler = new EventHandler<MessageReceivedArgs>((a, messageArgs) =>
            {
                messageArgs.Message.WriteEncryptedZip(filePath);
            });

            sut.Received += handler;

            sut.HandleBasicDeliver(
                "tag",
                34,
                false,
                "exchange",
                Guid.NewGuid().ToString(),
                _fixture.DefaultProperties,
                data);

            _fixture.FileWriterMock.Verify(_ => _.Write(It.IsAny<Stream>(), filePath));
        }

        [Fact]
        public void DokumentlagerHandlerIsUsedWhenHeaderIsSet()
        {
            var sut = _fixture.WithDokumentlagerHeader().CreateSut();

            var handler = new EventHandler<MessageReceivedArgs>((a, messageArgs) =>
            {
                var stream = messageArgs.Message.EncryptedStream;
            });

            sut.Received += handler;

            sut.HandleBasicDeliver(
                "tag",
                34,
                false,
                "exchange",
                Guid.NewGuid().ToString(),
                _fixture.DefaultProperties,
                null);

            _fixture.DokumentlagerHandler.Verify(_ => _.Download(It.IsAny<Guid>()));
        }

        [Fact]
        public void DokumentlagerHandlerIsNotUsedWhenHeaderIsNotSet()
        {
            var sut = _fixture.WithoutDokumentlagerHeader().CreateSut();

            var data = new[] {default(byte) };

            var handler = new EventHandler<MessageReceivedArgs>((a, messageArgs) =>
            {
                var stream = messageArgs.Message.EncryptedStream;
            });

            sut.Received += handler;

            sut.HandleBasicDeliver(
                "tag",
                34,
                false,
                "exchange",
                Guid.NewGuid().ToString(),
                _fixture.DefaultProperties,
                data);

            _fixture.DokumentlagerHandler.Verify(_ => _.Download(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task DataAsStreamIsReturnedWhenGettingEncryptedStream()
        {
            var sut = _fixture.CreateSut();

            var data = new[] {default(byte), byte.MaxValue};

            Stream actualDataStream = new MemoryStream();
            var handler = new EventHandler<MessageReceivedArgs>(async (a, messageArgs) =>
            {
                actualDataStream = await messageArgs.Message.EncryptedStream.ConfigureAwait(false);
            });

            sut.Received += handler;

            sut.HandleBasicDeliver(
                "tag",
                34,
                false,
                "exchange",
                Guid.NewGuid().ToString(),
                _fixture.DefaultProperties,
                data);

            var actualData = new byte[2];
            await actualDataStream.ReadAsync(actualData, 0, 2).ConfigureAwait(false);

            actualData[0].Should().Be(data[0]);
            actualData[1].Should().Be(data[1]);
        }

        [Fact]
        public async Task PayloadDecrypterDecryptIsCalledWhenGettingDecryptedStream()
        {
            var sut = _fixture.CreateSut();

            var data = new[] {default(byte), byte.MaxValue};

            Stream actualDataStream = new MemoryStream();
            var handler = new EventHandler<MessageReceivedArgs>(async (a, messageArgs) =>
            {
                actualDataStream = await messageArgs.Message.DecryptedStream.ConfigureAwait(false);
            });

            sut.Received += handler;

            sut.HandleBasicDeliver(
                "tag",
                34,
                false,
                "exchange",
                Guid.NewGuid().ToString(),
                _fixture.DefaultProperties,
                data);

            _fixture.AsicDecrypterMock.Verify(_ => _.Decrypt(It.IsAny<Task<Stream>>()));

            var actualData = new byte[2];
            await actualDataStream.ReadAsync(actualData, 0, 2).ConfigureAwait(false);
            actualData[0].Should().Be(data[0]);
            actualData[1].Should().Be(data[1]);
        }

        [Fact]
        public void PayloadDecrypterAndFileWriterIsCalledWhenWriteDecryptedFile()
        {
            var sut = _fixture.CreateSut();

            var data = new[] {default(byte), byte.MaxValue};
            var filePath = "/my/path/something.zip";

            var handler = new EventHandler<MessageReceivedArgs>((a, messageArgs) =>
            {
                messageArgs.Message.WriteDecryptedZip(filePath);
            });

            sut.Received += handler;

            sut.HandleBasicDeliver(
                "tag",
                34,
                false,
                "exchange",
                Guid.NewGuid().ToString(),
                _fixture.DefaultProperties,
                data);

            _fixture.AsicDecrypterMock.Verify(_ => _.WriteDecrypted(It.IsAny<Task<Stream>>(), filePath));
        }

        [Fact]
        public void BasicAckIsCalledFromReplySender()
        {
            var sut = _fixture.CreateSut();
            var data = new[] {default(byte), byte.MaxValue};

            var handler = new EventHandler<MessageReceivedArgs>((a, messageArgs) => { messageArgs.ReplySender.Ack(); });
            var deliveryTag = (ulong)3423423;

            sut.Received += handler;
            sut.HandleBasicDeliver(
                "tag",
                deliveryTag,
                false,
                "exchange",
                Guid.NewGuid().ToString(),
                _fixture.DefaultProperties,
                data);

            _fixture.ModelMock.Verify(_ => _.BasicAck(deliveryTag, false));
        }

        [Fact]
        public void BasicAckIsNotCalledWithDeliveryTagIfReceiverIsNotSet()
        {
            var sut = _fixture.CreateSut();
            var data = new[] {default(byte), byte.MaxValue};

            sut.HandleBasicDeliver(
                "tag",
                34,
                false,
                "exchange",
                Guid.NewGuid().ToString(),
                _fixture.DefaultProperties,
                data);

            _fixture.ModelMock.Verify(_ => _.BasicAck(It.IsAny<ulong>(), false), Times.Never);
        }
    }
}