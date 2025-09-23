using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using KS.Fiks.IO.Client.Amqp;
using KS.Fiks.IO.Client.Dokumentlager;
using KS.Fiks.IO.Client.FileIO;
using KS.Fiks.IO.Client.Models;
using KS.Fiks.IO.Client.Send;
using KS.Fiks.IO.Crypto.Asic;
using Moq;
using RabbitMQ.Client;

namespace KS.Fiks.IO.Client.Tests.Amqp
{
    public class AmqpReceiveConsumerFixture
    {
        private const string DokumentlagerHeaderName = "dokumentlager-id";

        private Mock<IReadOnlyBasicProperties> _defaultProperties;

        private bool _shouldUseDokumentlager = false;

        public AmqpReceiveConsumerFixture()
        {
            ChannelMock = new Mock<IChannel>();
            DokumentlagerHandler = new Mock<IDokumentlagerHandler>();
            FileWriterMock = new Mock<IFileWriter>();
            AsicDecrypterMock = new Mock<IAsicDecrypter>();
            SendHandlerMock = new Mock<ISendHandler>();
            AmqpWatcher = new Mock<IAmqpWatcher>();
            DefaultMetadata = new MottattMeldingMetadata(
                Guid.NewGuid(),
                "TestType",
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                TimeSpan.FromDays(2),
                null);
        }

        public Stream DokumentlagerOutStream => new MemoryStream();

        public Mock<IChannel> ChannelMock { get; }

        public MottattMeldingMetadata DefaultMetadata { get; }

        public IReadOnlyBasicProperties DefaultProperties => _defaultProperties.Object;

        public AmqpReceiveConsumerFixture WithDokumentlagerHeader()
        {
            _shouldUseDokumentlager = true;
            return this;
        }

        public AmqpReceiveConsumerFixture WithoutDokumentlagerHeader()
        {
            _shouldUseDokumentlager = false;
            return this;
        }

        internal Mock<IFileWriter> FileWriterMock { get; }

        internal Mock<IAsicDecrypter> AsicDecrypterMock { get; }

        internal Mock<ISendHandler> SendHandlerMock { get; }

        internal Mock<IDokumentlagerHandler> DokumentlagerHandler { get; }

        internal Mock<IAmqpWatcher> AmqpWatcher { get; }

        internal Task<AmqpReceiveConsumer> CreateSutAsync()
        {
            SetProperties();
            SetupMocks();
            return Task.FromResult(new AmqpReceiveConsumer(
                ChannelMock.Object,
                DokumentlagerHandler.Object,
                FileWriterMock.Object,
                AsicDecrypterMock.Object,
                SendHandlerMock.Object,
                AmqpWatcher.Object,
                DefaultMetadata.MottakerKontoId));
        }

        private void SetProperties()
        {
            _defaultProperties = new Mock<IReadOnlyBasicProperties>();

            var headers = new Dictionary<string, object>
            {
                {"avsender-id", Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()) },
                {"melding-id", Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()) },
                {"type", Encoding.UTF8.GetBytes("messageType") },
                {"svar-til", Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()) }
            };

            if (_shouldUseDokumentlager)
            {
                headers.Add(DokumentlagerHeaderName, Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()));
            }

            _defaultProperties.Setup(_ => _.Headers).Returns(headers);
            _defaultProperties.Setup(_ => _.Expiration).Returns("100");
        }

        private void SetupMocks()
        {
            ChannelMock.Setup(c => c.IsOpen).Returns(true);

            ChannelMock.Setup(c => c.BasicAckAsync(It.IsAny<ulong>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .Returns(ValueTask.CompletedTask);

            ChannelMock.Setup(c => c.BasicNackAsync(It.IsAny<ulong>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .Returns(ValueTask.CompletedTask);

            FileWriterMock.Setup(_ => _.WriteAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            AsicDecrypterMock.Setup(_ => _.Decrypt(It.IsAny<Task<Stream>>()))
                .Returns((Task<Stream> inStream) => inStream);

            DokumentlagerHandler.Setup(_ => _.Download(It.IsAny<Guid>()))
                .Returns(Task.FromResult(DokumentlagerOutStream));
        }
    }
}