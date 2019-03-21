using System.Collections.Generic;
using KS.Fiks.IO.Client.Amqp;
using KS.Fiks.IO.Client.Configuration;
using Moq;
using RabbitMQ.Client;

namespace KS.Fiks.IO.Client.Tests.Amqp
{
    public class AmqpHandlerFixture
    {
        private bool _connectionFactoryShouldThrow = false;
        private bool _connectionShouldThrow = false;
        private string _accountId = "testId";

        public AmqpHandlerFixture()
        {
            ConnectionFactoryMock = new Mock<IConnectionFactory>();
            ConnectionMock = new Mock<IConnection>();
            ModelMock = new Mock<IModel>();
            AmqpReceiveConsumerMock = new Mock<IAmqpReceiveConsumer>();
            AmqpConsumerFactoryMock = new Mock<IAmqpConsumerFactory>();
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

        public Mock<IModel> ModelMock { get; }

        public Mock<IConnectionFactory> ConnectionFactoryMock { get; }

        public Mock<IConnection> ConnectionMock { get; }

        internal Mock<IAmqpConsumerFactory> AmqpConsumerFactoryMock { get; }

        internal Mock<IAmqpReceiveConsumer> AmqpReceiveConsumerMock { get; }

        internal AmqpHandler CreateSut()
        {
            SetupMocks();
            return new AmqpHandler(
                CreateConfiguration(),
                _accountId,
                ConnectionFactoryMock.Object,
                AmqpConsumerFactoryMock.Object);
        }

        private void SetupMocks()
        {
            if (_connectionFactoryShouldThrow)
            {
                ConnectionFactoryMock.Setup(_ => _.CreateConnection(It.IsAny<IList<AmqpTcpEndpoint>>()))
                                     .Throws<ProtocolViolationException>();
            }
            else
            {
                ConnectionFactoryMock.Setup(_ => _.CreateConnection(It.IsAny<IList<AmqpTcpEndpoint>>()))
                                     .Returns(ConnectionMock.Object);
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

        private AmqpConfiguration CreateConfiguration()
        {
            return new AmqpConfiguration("api.fiks.test.ks.no");
        }
    }
}