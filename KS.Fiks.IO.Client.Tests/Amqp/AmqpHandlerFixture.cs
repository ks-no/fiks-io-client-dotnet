using KS.Fiks.IO.Client.Amqp;
using Moq;
using RabbitMQ.Client;

namespace KS.Fiks.IO.Client.Tests.Amqp
{
    public class AmqpHandlerFixture
    {
        private bool _connectionFactoryShouldThrow = false;
        private bool _connectionShouldThrow = false;

        public AmqpHandlerFixture()
        {
            ConnectionFactoryMock = new Mock<IConnectionFactory>();
            ConnectionMock = new Mock<IConnection>();
            ModelMock = new Mock<IModel>();
            AmqpReceiveConsumerMock = new Mock<IAmqpReceiveConsumer>();
            AmqpConsumerFactoryMock = new Mock<IAmqpConsumerFactory>();
        }

        public Mock<IConnectionFactory> ConnectionFactoryMock { get; }

        public Mock<IConnection> ConnectionMock { get; }

        public Mock<IAmqpConsumerFactory> AmqpConsumerFactoryMock { get; }

        public Mock<IAmqpReceiveConsumer> AmqpReceiveConsumerMock { get; }

        public Mock<IModel> ModelMock { get; }

        public AmqpHandler CreateSut()
        {
            SetupMocks();
            return new AmqpHandler(ConnectionFactoryMock.Object, AmqpConsumerFactoryMock.Object);
        }

        public AmqpHandlerFixture WhereConnectionfactoryThrowsException()
        {
            _connectionFactoryShouldThrow = true;
            return this;
        }

        public AmqpHandlerFixture WhereConnectionThrowsException()
        {
            _connectionShouldThrow = true;
            return this;
        }

        private void SetupMocks()
        {
            if (_connectionFactoryShouldThrow)
            {
                ConnectionFactoryMock.Setup(_ => _.CreateConnection()).Throws<ProtocolViolationException>();
            }
            else
            {
                ConnectionFactoryMock.Setup(_ => _.CreateConnection()).Returns(ConnectionMock.Object);
            }

            if (_connectionShouldThrow)
            {
                ConnectionMock.Setup(_ => _.CreateModel()).Throws<ProtocolViolationException>();
            }
            else
            {
                ConnectionMock.Setup(_ => _.CreateModel()).Returns(ModelMock.Object);
            }

            AmqpConsumerFactoryMock.Setup(_ => _.CreateReceiveConsumer(It.IsAny<IModel>()))
                                   .Returns(AmqpReceiveConsumerMock.Object);
        }
    }
}