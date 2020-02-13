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
            var handler = new EventHandler<MottattMeldingArgs>((a, _) => { hasBeenCalled = true; });

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
                {"avsender-id", Encoding.UTF8.GetBytes(expectedMessageMetadata.AvsenderKontoId.ToString()) },
                {"melding-id", Encoding.UTF8.GetBytes(expectedMessageMetadata.MeldingId.ToString()) },
                {"type", Encoding.UTF8.GetBytes(expectedMessageMetadata.MeldingType) },
                {"svar-til", Encoding.UTF8.GetBytes(expectedMessageMetadata.SvarPaMelding.ToString()) },
                {Utility.ReceivedMessageParser.EgendefinertHeaderPrefix + "test", Encoding.UTF8.GetBytes("Test")}
            };

            var propertiesMock = new Mock<IBasicProperties>();
            propertiesMock.Setup(_ => _.Headers).Returns(headers);
            propertiesMock.Setup(_ => _.Expiration)
                          .Returns(
                              expectedMessageMetadata.Ttl.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));

            var sut = _fixture.CreateSut();
            IMottattMelding actualMelding = new MottattMelding(
                true,
                _fixture.DefaultMetadata,
                () => Task.FromResult((Stream)new MemoryStream(new byte[1])),
                Mock.Of<IAsicDecrypter>(),
                Mock.Of<IFileWriter>());
            var handler = new EventHandler<MottattMeldingArgs>((a, messageArgs) =>
            {
                actualMelding = messageArgs.Melding;
            });

            sut.Received += handler;

            sut.HandleBasicDeliver(
                "tag",
                34,
                false,
                "exchange",
                expectedMessageMetadata.MottakerKontoId.ToString(),
                propertiesMock.Object,
                Array.Empty<byte>());

            actualMelding.MeldingId.Should().Be(expectedMessageMetadata.MeldingId);
            actualMelding.MeldingType.Should().Be(expectedMessageMetadata.MeldingType);
            actualMelding.MottakerKontoId.Should().Be(expectedMessageMetadata.MottakerKontoId);
            actualMelding.AvsenderKontoId.Should().Be(expectedMessageMetadata.AvsenderKontoId);
            actualMelding.SvarPaMelding.Should().Be(expectedMessageMetadata.SvarPaMelding);
            actualMelding.Ttl.Should().Be(expectedMessageMetadata.Ttl);
            actualMelding.Headere["test"].ToString().Should().Be("Test");
        }

        [Fact]
        public void ThrowsParseExceptionIfMessageIdIsNotValidGuid()
        {
            var expectedMessageMetadata = _fixture.DefaultMetadata;

            var headers = new Dictionary<string, object>
            {
                {"avsender-id", Encoding.UTF8.GetBytes(expectedMessageMetadata.AvsenderKontoId.ToString()) },
                {"melding-id", Encoding.UTF8.GetBytes("NoTGuid") },
                {"type", Encoding.UTF8.GetBytes(expectedMessageMetadata.MeldingType) },
                {"svar-til", Encoding.UTF8.GetBytes(expectedMessageMetadata.SvarPaMelding.ToString()) }
            };

            var propertiesMock = new Mock<IBasicProperties>();
            propertiesMock.Setup(_ => _.Headers).Returns(headers);
            propertiesMock.Setup(_ => _.Expiration)
                          .Returns(
                              expectedMessageMetadata.Ttl.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));

            var sut = _fixture.CreateSut();
            var handler = new EventHandler<MottattMeldingArgs>((a, messageArgs) => { });

            sut.Received += handler;
            Assert.Throws<FiksIOParseException>(() =>
            {
                sut.HandleBasicDeliver(
                    "tag",
                    34,
                    false,
                    "exchange",
                    expectedMessageMetadata.MottakerKontoId.ToString(),
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
            var handler = new EventHandler<MottattMeldingArgs>((a, messageArgs) => { });

            sut.Received += handler;
            Assert.Throws<FiksIOMissingHeaderException>(() =>
            {
                sut.HandleBasicDeliver(
                    "tag",
                    34,
                    false,
                    "exchange",
                    expectedMessageMetadata.MottakerKontoId.ToString(),
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
                {"avsender-id", Encoding.UTF8.GetBytes(expectedMessageMetadata.AvsenderKontoId.ToString()) },
                {"type", Encoding.UTF8.GetBytes(expectedMessageMetadata.MeldingType) },
                {"svar-til", Encoding.UTF8.GetBytes(expectedMessageMetadata.SvarPaMelding.ToString()) }
            };

            var propertiesMock = new Mock<IBasicProperties>();
            propertiesMock.Setup(_ => _.Headers).Returns(headers);
            propertiesMock.Setup(_ => _.Expiration)
                          .Returns(
                              expectedMessageMetadata.Ttl.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));

            var sut = _fixture.CreateSut();
            var handler = new EventHandler<MottattMeldingArgs>((a, messageArgs) => { });

            sut.Received += handler;
            Assert.Throws<FiksIOMissingHeaderException>(() =>
            {
                sut.HandleBasicDeliver(
                    "tag",
                    34,
                    false,
                    "exchange",
                    expectedMessageMetadata.MottakerKontoId.ToString(),
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

            var handler = new EventHandler<MottattMeldingArgs>((a, messageArgs) =>
            {
                messageArgs.Melding.WriteEncryptedZip(filePath);
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

            var handler = new EventHandler<MottattMeldingArgs>((a, messageArgs) =>
            {
                var stream = messageArgs.Melding.EncryptedStream;
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

            var handler = new EventHandler<MottattMeldingArgs>((a, messageArgs) =>
            {
                var stream = messageArgs.Melding.EncryptedStream;
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
            var handler = new EventHandler<MottattMeldingArgs>(async (a, messageArgs) =>
            {
                actualDataStream = await messageArgs.Melding.EncryptedStream.ConfigureAwait(false);
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
            var handler = new EventHandler<MottattMeldingArgs>(async (a, messageArgs) =>
            {
                actualDataStream = await messageArgs.Melding.DecryptedStream.ConfigureAwait(false);
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

            var handler = new EventHandler<MottattMeldingArgs>((a, messageArgs) =>
            {
                messageArgs.Melding.WriteDecryptedZip(filePath);
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

            var handler = new EventHandler<MottattMeldingArgs>((a, messageArgs) => { messageArgs.SvarSender.Ack(); });
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

        [Fact]
        public void ThrowsExceptionWhenGettingEncryptedStreamWithNoData()
        {
            var sut = _fixture.CreateSut();
            var data = Array.Empty<byte>();
            var correctExceptionThrown = false;

            var handler = new EventHandler<MottattMeldingArgs>(async (a, messageArgs) =>
            {
                try
                {
                    var stream = await messageArgs.Melding.EncryptedStream;
                }
                catch (FiksIOMissingDataException ex)
                {
                    correctExceptionThrown = true;
                }
            });
            var deliveryTag = (ulong) 3423423;

            sut.Received += handler;
            sut.HandleBasicDeliver(
                "tag",
                deliveryTag,
                false,
                "exchange",
                Guid.NewGuid().ToString(),
                _fixture.DefaultProperties,
                data);

            correctExceptionThrown.Should().BeTrue();
        }

        [Fact]
        public void ThrowsExceptionWhenGettingDecryptedStreamWithNoData()
        {
            var sut = _fixture.CreateSut();
            var data = Array.Empty<byte>();
            var correctExceptionThrown = false;

            var handler = new EventHandler<MottattMeldingArgs>(async (a, messageArgs) =>
            {
                try
                {
                    var stream = await messageArgs.Melding.DecryptedStream;
                }
                catch (FiksIOMissingDataException ex)
                {
                    correctExceptionThrown = true;
                }
            });
            var deliveryTag = (ulong) 3423423;

            sut.Received += handler;
            sut.HandleBasicDeliver(
                "tag",
                deliveryTag,
                false,
                "exchange",
                Guid.NewGuid().ToString(),
                _fixture.DefaultProperties,
                data);

            correctExceptionThrown.Should().BeTrue();
        }

        [Fact]
        public void ThrowsExceptionWhenWritingDecryptedStreamWithNoData()
        {
            var sut = _fixture.CreateSut();
            var data = Array.Empty<byte>();
            var correctExceptionThrown = false;

            var handler = new EventHandler<MottattMeldingArgs>(async (a, messageArgs) =>
            {
                try
                {
                    await messageArgs.Melding.WriteDecryptedZip("out.zip").ConfigureAwait(false);
                }
                catch (FiksIOMissingDataException ex)
                {
                    correctExceptionThrown = true;
                }
            });
            var deliveryTag = (ulong) 3423423;

            sut.Received += handler;
            sut.HandleBasicDeliver(
                "tag",
                deliveryTag,
                false,
                "exchange",
                Guid.NewGuid().ToString(),
                _fixture.DefaultProperties,
                data);

            correctExceptionThrown.Should().BeTrue();
        }

        [Fact]
        public void ThrowsExceptionWhenWritingEnryptedStreamWithNoData()
        {
            var sut = _fixture.CreateSut();
            var data = Array.Empty<byte>();
            var correctExceptionThrown = false;

            var handler = new EventHandler<MottattMeldingArgs>(async (a, messageArgs) =>
            {
                try
                {
                    await messageArgs.Melding.WriteEncryptedZip("out.zip").ConfigureAwait(false);
                }
                catch (FiksIOMissingDataException ex)
                {
                    correctExceptionThrown = true;
                }
            });
            var deliveryTag = (ulong) 3423423;

            sut.Received += handler;
            sut.HandleBasicDeliver(
                "tag",
                deliveryTag,
                false,
                "exchange",
                Guid.NewGuid().ToString(),
                _fixture.DefaultProperties,
                data);

            correctExceptionThrown.Should().BeTrue();
        }
    }
}