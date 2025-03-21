using System;
using KS.Fiks.IO.Client.Amqp;
using KS.Fiks.IO.Client.Configuration;
using KS.Fiks.IO.Client.Dokumentlager;
using KS.Fiks.IO.Client.Send;
using Moq;
using RabbitMQ.Client;
using Xunit;

namespace KS.Fiks.IO.Client.Tests.Amqp
{
    public class AmqpConsumerFactoryTests
    {
        private readonly Mock<IChannel> _channelMock;
        private readonly AmqpConsumerFactory _factory;

        public AmqpConsumerFactoryTests()
        {
            var sendHandlerMock = new Mock<ISendHandler>();
            var dokumentlagerHandlerMock = new Mock<IDokumentlagerHandler>();
            var amqpWatcherMock = new Mock<IAmqpWatcher>();
            _channelMock = new Mock<IChannel>();

            var kontoConfiguration = new KontoConfiguration(Guid.NewGuid(), "private-key");

            _factory = new AmqpConsumerFactory(
                sendHandlerMock.Object,
                dokumentlagerHandlerMock.Object,
                amqpWatcherMock.Object,
                kontoConfiguration);
        }

        [Fact]
        public void CreateReceiveConsumer_ReturnsAmqpReceiveConsumer()
        {
            var consumer = _factory.CreateReceiveConsumer(_channelMock.Object);

            Assert.NotNull(consumer);
            Assert.IsType<AmqpReceiveConsumer>(consumer);
        }
    }
}