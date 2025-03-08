using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using KS.Fiks.IO.Client.Exceptions;
using KS.Fiks.IO.Client.FileIO;
using KS.Fiks.IO.Client.Models;
using KS.Fiks.IO.Client.Utility;
using KS.Fiks.IO.Crypto.Asic;
using Moq;
using RabbitMQ.Client;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace KS.Fiks.IO.Client.Tests.Amqp
{
    public class AmqpReceiveConsumerTests
    {
        private AmqpReceiveConsumerFixture _fixture;

        public AmqpReceiveConsumerTests(ITestOutputHelper testOutputHelper)
        {
            _fixture = new AmqpReceiveConsumerFixture();
        }

        [Fact]
        public async Task ReceivedHandlerAsync()
        {
            var sut = await _fixture.CreateSutAsync();

            var hasBeenCalled = false;

            Func<MottattMeldingArgs, Task> handler = async args =>
            {
                hasBeenCalled = true;
                await Task.CompletedTask.ConfigureAwait(false);
            };

            sut.ReceivedAsync += handler;


            await sut.HandleBasicDeliverAsync(
                "tag",
                34,
                false,
                "exchange",
                Guid.NewGuid().ToString(),
                _fixture.DefaultProperties,
                Array.Empty<byte>());

            hasBeenCalled.ShouldBeTrue();
        }

        [Fact]
        public async Task ReceivesExpectedMessageMetadataAsync()
        {
            var expectedMessageMetadata = _fixture.DefaultMetadata;

            var headers = new Dictionary<string, object>
            {
                { "avsender-id", Encoding.UTF8.GetBytes(expectedMessageMetadata.AvsenderKontoId.ToString()) },
                { "melding-id", Encoding.UTF8.GetBytes(expectedMessageMetadata.MeldingId.ToString()) },
                { "type", Encoding.UTF8.GetBytes(expectedMessageMetadata.MeldingType) },
                { "svar-til", Encoding.UTF8.GetBytes(expectedMessageMetadata.SvarPaMelding.ToString()) },
                { ReceivedMessageParser.EgendefinertHeaderPrefix + "test", Encoding.UTF8.GetBytes("Test") }
            };

            var propertiesMock = new Mock<IReadOnlyBasicProperties>();
            propertiesMock.Setup(_ => _.Headers).Returns(headers);
            propertiesMock.Setup(_ => _.Expiration)
                .Returns(expectedMessageMetadata.Ttl.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));

            var sut = await _fixture.CreateSutAsync();

            IMottattMelding actualMelding = null;

            Func<MottattMeldingArgs, Task> handler = async messageArgs =>
            {
                actualMelding = messageArgs.Melding;
                await Task.CompletedTask.ConfigureAwait(false);
            };

            sut.ReceivedAsync += handler;

            await sut.HandleBasicDeliverAsync(
                "tag",
                34,
                false,
                "exchange",
                expectedMessageMetadata.MottakerKontoId.ToString(),
                propertiesMock.Object,
                Array.Empty<byte>());

            actualMelding.ShouldNotBeNull();
            actualMelding.MeldingId.ShouldBe(expectedMessageMetadata.MeldingId);
            actualMelding.MeldingType.ShouldBe(expectedMessageMetadata.MeldingType);
            actualMelding.MottakerKontoId.ShouldBe(expectedMessageMetadata.MottakerKontoId);
            actualMelding.AvsenderKontoId.ShouldBe(expectedMessageMetadata.AvsenderKontoId);
            actualMelding.SvarPaMelding.ShouldBe(expectedMessageMetadata.SvarPaMelding);
            actualMelding.Ttl.ShouldBe(expectedMessageMetadata.Ttl);
            actualMelding.Headere["test"].ShouldBe("Test");
            actualMelding.Resendt.ShouldBeFalse();
        }

        [Fact]
        public async Task ReceivesExpectedMessageMetadataWithRedeliveredTrueAsync()
        {
            var expectedMessageMetadata = _fixture.DefaultMetadata;

            var headers = new Dictionary<string, object>
            {
                { "avsender-id", Encoding.UTF8.GetBytes(expectedMessageMetadata.AvsenderKontoId.ToString()) },
                { "melding-id", Encoding.UTF8.GetBytes(expectedMessageMetadata.MeldingId.ToString()) },
                { "type", Encoding.UTF8.GetBytes(expectedMessageMetadata.MeldingType) },
                { "svar-til", Encoding.UTF8.GetBytes(expectedMessageMetadata.SvarPaMelding.ToString()) },
                { ReceivedMessageParser.EgendefinertHeaderPrefix + "test", Encoding.UTF8.GetBytes("Test") }
            };

            var propertiesMock = new Mock<IReadOnlyBasicProperties>();
            propertiesMock.Setup(_ => _.Headers).Returns(headers);
            propertiesMock.Setup(_ => _.Expiration)
                .Returns(expectedMessageMetadata.Ttl.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));

            var sut = await _fixture.CreateSutAsync();

            IMottattMelding actualMelding = new MottattMelding(
                hasPayload: true,
                metadata: _fixture.DefaultMetadata,
                 () => Task.FromResult<Stream>(new MemoryStream(new byte[1])),
                decrypter: Mock.Of<IAsicDecrypter>(),
                fileWriter: Mock.Of<IFileWriter>());

            Func<MottattMeldingArgs, Task> handler = async messageArgs =>
            {
                actualMelding = messageArgs.Melding;
                await Task.CompletedTask.ConfigureAwait(false);
            };

            sut.ReceivedAsync += handler;

            await sut.HandleBasicDeliverAsync(
                "tag",
                34,
                true,
                "exchange",
                expectedMessageMetadata.MottakerKontoId.ToString(),
                propertiesMock.Object,
                Array.Empty<byte>());

            actualMelding.ShouldNotBeNull();
            actualMelding.MeldingId.ShouldBe(expectedMessageMetadata.MeldingId);
            actualMelding.MeldingType.ShouldBe(expectedMessageMetadata.MeldingType);
            actualMelding.MottakerKontoId.ShouldBe(expectedMessageMetadata.MottakerKontoId);
            actualMelding.AvsenderKontoId.ShouldBe(expectedMessageMetadata.AvsenderKontoId);
            actualMelding.SvarPaMelding.ShouldBe(expectedMessageMetadata.SvarPaMelding);
            actualMelding.Ttl.ShouldBe(expectedMessageMetadata.Ttl);
            actualMelding.Headere["test"].ShouldBe("Test");
            actualMelding.Resendt.ShouldBeTrue();
        }

        [Fact]
        public async Task ThrowsParseExceptionIfMessageIdIsNotValidGuidAsync()
        {
            var expectedMessageMetadata = _fixture.DefaultMetadata;

            var headers = new Dictionary<string, object>
            {
                {"avsender-id", Encoding.UTF8.GetBytes(expectedMessageMetadata.AvsenderKontoId.ToString()) },
                {"melding-id", Encoding.UTF8.GetBytes("NoTGuid") },
                {"type", Encoding.UTF8.GetBytes(expectedMessageMetadata.MeldingType) },
                {"svar-til", Encoding.UTF8.GetBytes(expectedMessageMetadata.SvarPaMelding.ToString()) }
            };

            var propertiesMock = new Mock<IReadOnlyBasicProperties>();
            propertiesMock.Setup(_ => _.Headers).Returns(headers);
            propertiesMock.Setup(_ => _.Expiration)
                .Returns(expectedMessageMetadata.Ttl.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));

            var sut = await _fixture.CreateSutAsync();

            Func<MottattMeldingArgs, Task> handler = _ => Task.CompletedTask;

            sut.ReceivedAsync += handler;

            await Should.ThrowAsync<FiksIOParseException>(async () =>
            {
                await sut.HandleBasicDeliverAsync(
                    "tag",
                    34,
                    false,
                    "exchange",
                    expectedMessageMetadata.MottakerKontoId.ToString(),
                    propertiesMock.Object,
                    Array.Empty<byte>()).ConfigureAwait(false);
            });
        }

        [Fact]
        public async Task ThrowsMissingHeaderExceptionIfHeaderIsNullAsync()
        {
            var expectedMessageMetadata = _fixture.DefaultMetadata;

            var propertiesMock = new Mock<IReadOnlyBasicProperties>();
            propertiesMock.Setup(_ => _.Headers).Returns((IDictionary<string, object>)null);
            propertiesMock.Setup(_ => _.Expiration)
                .Returns(expectedMessageMetadata.Ttl.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));

            var sut = await _fixture.CreateSutAsync();

            Func<MottattMeldingArgs, Task> handler = _ => Task.CompletedTask;
            sut.ReceivedAsync += handler;

            await Should.ThrowAsync<FiksIOMissingHeaderException>(async () =>
            {
                await sut.HandleBasicDeliverAsync(
                    "tag",
                    34,
                    false,
                    "exchange",
                    expectedMessageMetadata.MottakerKontoId.ToString(),
                    propertiesMock.Object,
                    Array.Empty<byte>()).ConfigureAwait(false);
            });
        }

        [Fact]
        public async Task ThrowsMissingHeaderExceptionIfMessageIdIsMissingAsync()
        {
            var expectedMessageMetadata = _fixture.DefaultMetadata;

            var headers = new Dictionary<string, object>
            {
                {"avsender-id", Encoding.UTF8.GetBytes(expectedMessageMetadata.AvsenderKontoId.ToString()) },
                {"type", Encoding.UTF8.GetBytes(expectedMessageMetadata.MeldingType) },
                {"svar-til", Encoding.UTF8.GetBytes(expectedMessageMetadata.SvarPaMelding.ToString()) }
            };

            var propertiesMock = new Mock<IReadOnlyBasicProperties>();
            propertiesMock.Setup(_ => _.Headers).Returns(headers);
            propertiesMock.Setup(_ => _.Expiration)
                .Returns(expectedMessageMetadata.Ttl.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));

            var sut = await _fixture.CreateSutAsync();
            Func<MottattMeldingArgs, Task> handler = _ => Task.CompletedTask;
            sut.ReceivedAsync += handler;

            await Should.ThrowAsync<FiksIOMissingHeaderException>(async () =>
            {
                await sut.HandleBasicDeliverAsync(
                    "tag",
                    34,
                    false,
                    "exchange",
                    expectedMessageMetadata.MottakerKontoId.ToString(),
                    propertiesMock.Object,
                    Array.Empty<byte>()).ConfigureAwait(false);
            });
        }

        [Fact]
        public async Task FileWriterWriteIsCalledWhenWriteEncryptedZipAsync()
        {
            var sut = await _fixture.CreateSutAsync();

            var filePath = "/my/path/something.zip";
            var data = new[] { default(byte) };

            Func<MottattMeldingArgs, Task> handler = async messageArgs =>
            {
                await messageArgs.Melding.WriteEncryptedZip(filePath).ConfigureAwait(false);
            };

            sut.ReceivedAsync += handler;

            await sut.HandleBasicDeliverAsync(
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
        public async Task DokumentlagerHandlerIsUsedWhenHeaderIsSetAsync()
        {
            var sut = await _fixture.WithDokumentlagerHeader().CreateSutAsync();

            Func<MottattMeldingArgs, Task> handler = async messageArgs =>
            {
                await messageArgs.Melding.EncryptedStream.ConfigureAwait(false);
            };

            sut.ReceivedAsync += handler;

            await sut.HandleBasicDeliverAsync(
                "tag",
                34,
                false,
                "exchange",
                Guid.NewGuid().ToString(),
                _fixture.DefaultProperties,
                null);

            _fixture.DokumentlagerHandler.Verify(_ => _.Download(It.IsAny<Guid>()), Times.Once);
        }

        [Fact]
        public async Task DokumentlagerHandlerIsNotUsedWhenHeaderIsNotSetAsync()
        {
            var sut = await _fixture.WithoutDokumentlagerHeader().CreateSutAsync();

            var data = new[] { default(byte) };

            Func<MottattMeldingArgs, Task> handler = async messageArgs =>
            {
                await messageArgs.Melding.EncryptedStream.ConfigureAwait(false);
            };

            sut.ReceivedAsync += handler;

            await sut.HandleBasicDeliverAsync(
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
        public async Task DataAsStreamIsReturnedWhenGettingEncryptedStreamAsync()
        {
            var sut = await _fixture.CreateSutAsync();

            var data = new[] { default(byte), byte.MaxValue };

            Stream actualDataStream = new MemoryStream();
            Func<MottattMeldingArgs, Task> handler = async messageArgs =>
            {
                actualDataStream = await messageArgs.Melding.EncryptedStream.ConfigureAwait(false);
            };

            sut.ReceivedAsync += handler;

            await sut.HandleBasicDeliverAsync(
                "tag",
                34,
                false,
                "exchange",
                Guid.NewGuid().ToString(),
                _fixture.DefaultProperties,
                data);

            var actualData = new byte[2];
            await actualDataStream.ReadAsync(actualData, 0, 2);

            actualData[0].ShouldBe(data[0]);
            actualData[1].ShouldBe(data[1]);
        }

        [Fact]
        public async Task PayloadDecrypterDecryptIsCalledWhenGettingDecryptedStreamAsync()
        {
            var sut = await _fixture.CreateSutAsync();

            var data = new[] { default(byte), byte.MaxValue };

            Stream actualDataStream = new MemoryStream();
            Func<MottattMeldingArgs, Task> handler = async messageArgs =>
            {
                actualDataStream = await messageArgs.Melding.DecryptedStream.ConfigureAwait(false);
            };

            sut.ReceivedAsync += handler;

            await sut.HandleBasicDeliverAsync(
                "tag",
                34,
                false,
                "exchange",
                Guid.NewGuid().ToString(),
                _fixture.DefaultProperties,
                data);

            _fixture.AsicDecrypterMock.Verify(_ => _.Decrypt(It.IsAny<Task<Stream>>()), Times.Once);

            var actualData = new byte[2];
            await actualDataStream.ReadAsync(actualData, 0, 2);

            actualData[0].ShouldBe(data[0]);
            actualData[1].ShouldBe(data[1]);
        }

        [Fact]
        public async Task PayloadDecrypterAndFileWriterIsCalledWhenWriteDecryptedFileAsync()
        {
            var sut = await _fixture.CreateSutAsync();

            var data = new[] { default(byte), byte.MaxValue };
            var filePath = "/my/path/something.zip";

            Func<MottattMeldingArgs, Task> handler = async messageArgs =>
            {
                await messageArgs.Melding.WriteDecryptedZip(filePath).ConfigureAwait(false);
            };

            sut.ReceivedAsync += handler;

            await sut.HandleBasicDeliverAsync(
                "tag",
                34,
                false,
                "exchange",
                Guid.NewGuid().ToString(),
                _fixture.DefaultProperties,
                data);

            _fixture.AsicDecrypterMock.Verify(_ => _.WriteDecrypted(It.IsAny<Task<Stream>>(), filePath), Times.Once);
        }

        [Fact]
        public async Task BasicAckIsCalledFromReplySenderAsync()
        {
            var sut = await _fixture.CreateSutAsync();
            var data = new[] { default(byte), byte.MaxValue };

            var deliveryTag = (ulong)3423423;

            Func<MottattMeldingArgs, Task> handler = async messageArgs =>
            {
                await messageArgs.SvarSender.AckAsync().ConfigureAwait(false);
            };

            sut.ReceivedAsync += handler;

            await sut.HandleBasicDeliverAsync(
                "tag",
                deliveryTag,
                false,
                "exchange",
                Guid.NewGuid().ToString(),
                _fixture.DefaultProperties,
                data);

            _fixture.ChannelMock.Verify(_ => _.BasicAckAsync(deliveryTag, false, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task BasicAckIsNotCalledWithDeliveryTagIfReceiverIsNotSetAsync()
        {
            var sut = await _fixture.CreateSutAsync();
            var data = new[] { default(byte), byte.MaxValue };

            await sut.HandleBasicDeliverAsync(
                "tag",
                34,
                false,
                "exchange",
                Guid.NewGuid().ToString(),
                _fixture.DefaultProperties,
                data);

            _fixture.ChannelMock.Verify(
                _ => _.BasicAckAsync(It.IsAny<ulong>(), false, It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task ThrowsExceptionWhenGettingEncryptedStreamWithNoDataAsync()
        {
            var sut = await _fixture.CreateSutAsync();
            var data = Array.Empty<byte>();
            var correctExceptionThrown = false;

            Func<MottattMeldingArgs, Task> handler = async messageArgs =>
            {
                try
                {
                    var stream = await messageArgs.Melding.EncryptedStream.ConfigureAwait(false);
                }
                catch (FiksIOMissingDataException)
                {
                    correctExceptionThrown = true;
                }
            };

            var deliveryTag = (ulong)3423423;

            sut.ReceivedAsync += handler;

            await sut.HandleBasicDeliverAsync(
                "tag",
                deliveryTag,
                false,
                "exchange",
                Guid.NewGuid().ToString(),
                _fixture.DefaultProperties,
                data);

            correctExceptionThrown.ShouldBeTrue();
        }

        [Fact]
        public async Task ThrowsExceptionWhenGettingDecryptedStreamWithNoDataAsync()
        {
            var sut = await _fixture.CreateSutAsync();
            var data = Array.Empty<byte>();
            var correctExceptionThrown = false;

            Func<MottattMeldingArgs, Task> handler = async messageArgs =>
            {
                try
                {
                    var stream = await messageArgs.Melding.DecryptedStream.ConfigureAwait(false);
                }
                catch (FiksIOMissingDataException)
                {
                    correctExceptionThrown = true;
                }
            };

            var deliveryTag = (ulong)3423423;

            sut.ReceivedAsync += handler;

            await sut.HandleBasicDeliverAsync(
                "tag",
                deliveryTag,
                false,
                "exchange",
                Guid.NewGuid().ToString(),
                _fixture.DefaultProperties,
                data);

            correctExceptionThrown.ShouldBeTrue();
        }

        [Fact]
        public async Task ThrowsExceptionWhenWritingDecryptedStreamWithNoDataAsync()
        {
            var sut = await _fixture.CreateSutAsync();
            var data = Array.Empty<byte>();
            var correctExceptionThrown = false;

            Func<MottattMeldingArgs, Task> handler = async messageArgs =>
            {
                try
                {
                    await messageArgs.Melding.WriteDecryptedZip("out.zip").ConfigureAwait(false);
                }
                catch (FiksIOMissingDataException)
                {
                    correctExceptionThrown = true;
                }
            };

            var deliveryTag = (ulong)3423423;

            sut.ReceivedAsync += handler;

            await sut.HandleBasicDeliverAsync(
                "tag",
                deliveryTag,
                false,
                "exchange",
                Guid.NewGuid().ToString(),
                _fixture.DefaultProperties,
                data);

            correctExceptionThrown.ShouldBeTrue();
        }

        [Fact]
        public async Task ThrowsExceptionWhenWritingEncryptedStreamWithNoDataAsync()
        {
            var sut = await _fixture.CreateSutAsync();
            var data = Array.Empty<byte>();
            var correctExceptionThrown = false;

            Func<MottattMeldingArgs, Task> handler = async messageArgs =>
            {
                try
                {
                    await messageArgs.Melding.WriteEncryptedZip("out.zip").ConfigureAwait(false);
                }
                catch (FiksIOMissingDataException)
                {
                    correctExceptionThrown = true;
                }
            };

            var deliveryTag = (ulong)3423423;

            sut.ReceivedAsync += handler;

            await sut.HandleBasicDeliverAsync(
                "tag",
                deliveryTag,
                false,
                "exchange",
                Guid.NewGuid().ToString(),
                _fixture.DefaultProperties,
                data);

            correctExceptionThrown.ShouldBeTrue();
        }
    }
}