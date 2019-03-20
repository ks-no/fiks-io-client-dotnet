using System;
using System.Collections.Generic;
using System.IO;
using KS.Fiks.IO.Client.Amqp;
using KS.Fiks.IO.Client.Encryption;
using KS.Fiks.IO.Client.FileIO;
using KS.Fiks.IO.Client.Models;
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
            PayloadDecrypterMock = new Mock<IPayloadDecrypter>();
            SetDefaultProperties();
        }

        public Mock<IModel> ModelMock { get; }

        public Mock<IFileWriter> FileWriterMock { get; }

        public Mock<IPayloadDecrypter> PayloadDecrypterMock { get; }

        public ReceivedMessageMetadata DefaultMetadata => new ReceivedMessageMetadata
        {
            MessageId = Guid.NewGuid(),
            MessageType = "TestType",
            ReceiverAccountId = Guid.NewGuid(),
            SenderAccountId = Guid.NewGuid(),
            SvarPaMelding = Guid.NewGuid(),
            Ttl = TimeSpan.FromDays(2)
        };

        public IBasicProperties DefaultProperties => _defaultProperties.Object;

        public AmqpReceiveConsumer CreateSut()
        {
            SetupMocks();
            return new AmqpReceiveConsumer(ModelMock.Object, FileWriterMock.Object, PayloadDecrypterMock.Object);
        }

        private void SetDefaultProperties()
        {
            _defaultProperties = new Mock<IBasicProperties>();
            var headers = new Dictionary<string, object>
            {
                {"avsender-id", Guid.NewGuid().ToString() },
                {"melding-id", Guid.NewGuid().ToString() },
                {"type", "messageType"},
                {"svar-til", Guid.NewGuid().ToString() }
            };

            _defaultProperties.Setup(_ => _.Headers).Returns(headers);
            _defaultProperties.Setup(_ => _.Expiration)
                              .Returns(value: "100");
        }

        private void SetupMocks()
        {
            FileWriterMock.Setup(_ => _.Write(It.IsAny<string>(), It.IsAny<byte[]>()));
            FileWriterMock.Setup(_ => _.Write(It.IsAny<string>(), It.IsAny<Stream>()));
            PayloadDecrypterMock.Setup(_ => _.Decrypt(It.IsAny<byte[]>()))
                                .Returns((byte[] inStream) => (Stream)new MemoryStream(inStream));
            PayloadDecrypterMock.Setup(_ => _.Decrypt(It.IsAny<Stream>()))
                                .Returns((Stream inStream) => inStream);
        }
    }
}