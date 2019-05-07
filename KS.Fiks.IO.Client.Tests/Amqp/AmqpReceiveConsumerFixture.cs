using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using KS.Fiks.IO.Client.Amqp;
using KS.Fiks.IO.Client.Asic;
using KS.Fiks.IO.Client.FileIO;
using KS.Fiks.IO.Client.Models;
using KS.Fiks.IO.Client.Send;
using Moq;
using RabbitMQ.Client;

namespace KS.Fiks.IO.Client.Tests.Amqp
{
    public class AmqpReceiveConsumerFixture
    {
        private Mock<IBasicProperties> _defaultProperties;

        public AmqpReceiveConsumerFixture()
        {
            ModelMock = new Mock<IModel>();
            FileWriterMock = new Mock<IFileWriter>();
            AsicDecrypterMock = new Mock<IAsicDecrypter>();
            SendHandlerMock = new Mock<ISendHandler>();
            SetDefaultProperties();
            DefaultMetadata = new ReceivedMessageMetadata(
                Guid.NewGuid(),
                "TestType",
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                TimeSpan.FromDays(2));
        }

        public Mock<IModel> ModelMock { get; }

        public ReceivedMessageMetadata DefaultMetadata { get; }

        public IBasicProperties DefaultProperties => _defaultProperties.Object;

        internal Mock<IFileWriter> FileWriterMock { get; }

        internal Mock<IAsicDecrypter> AsicDecrypterMock { get; }

        internal Mock<ISendHandler> SendHandlerMock { get; }

        internal AmqpReceiveConsumer CreateSut()
        {
            SetupMocks();
            return new AmqpReceiveConsumer(
                ModelMock.Object,
                FileWriterMock.Object,
                AsicDecrypterMock.Object,
                SendHandlerMock.Object,
                DefaultMetadata.ReceiverAccountId);
        }

        private void SetDefaultProperties()
        {
            _defaultProperties = new Mock<IBasicProperties>();
            var headers = new Dictionary<string, object>
            {
                {"avsender-id", Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()) },
                {"melding-id", Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()) },
                {"type", Encoding.UTF8.GetBytes("messageType") },
                {"svar-til", Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()) }
            };

            _defaultProperties.Setup(_ => _.Headers).Returns(headers);
            _defaultProperties.Setup(_ => _.Expiration)
                              .Returns(value: "100");
        }

        private void SetupMocks()
        {
            FileWriterMock.Setup(_ => _.Write(It.IsAny<string>(), It.IsAny<byte[]>()));
            FileWriterMock.Setup(_ => _.Write(It.IsAny<string>(), It.IsAny<Stream>()));
            AsicDecrypterMock.Setup(_ => _.Decrypt(It.IsAny<byte[]>()))
                                .Returns((byte[] inStream) => (Stream)new MemoryStream(inStream));
            AsicDecrypterMock.Setup(_ => _.Decrypt(It.IsAny<Stream>()))
                                .Returns((Stream inStream) => inStream);
        }
    }
}