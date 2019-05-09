using System;
using System.Collections.Generic;
using KS.Fiks.IO.Client.Amqp;
using KS.Fiks.IO.Client.Configuration;
using KS.Fiks.IO.Client.Dokumentlager;
using KS.Fiks.IO.Client.Send;
using Ks.Fiks.Maskinporten.Client;
using Moq;
using RabbitMQ.Client;

namespace KS.Fiks.IO.Client.Tests.Amqp
{
    public class AmqpHandlerFixture
    {
        private bool _connectionFactoryShouldThrow = false;
        private bool _connectionShouldThrow = false;
        private Guid _accountId = Guid.NewGuid();
        private string _token = "testtoken";
        private Guid _integrationId = Guid.NewGuid();
        private string _integrationPassword = "defaultPassword";

        public AmqpHandlerFixture()
        {
            ConnectionFactoryMock = new Mock<IConnectionFactory>();
            ConnectionMock = new Mock<IConnection>();
            ModelMock = new Mock<IModel>();
            AmqpReceiveConsumerMock = new Mock<IAmqpReceiveConsumer>();
            AmqpConsumerFactoryMock = new Mock<IAmqpConsumerFactory>();
            MaskinportenClientMock = new Mock<IMaskinportenClient>();
            SendHandlerMock = new Mock<ISendHandler>();
            DokumentlagerHandlerMock = new Mock<IDokumentlagerHandler>();
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

        public AmqpHandlerFixture WithMaskinportenToken(string token)
        {
            _token = token;
            return this;
        }

        public AmqpHandlerFixture WithIntegrationPassword(string password)
        {
            _integrationPassword = password;
            return this;
        }

        public AmqpHandlerFixture WithIntegrationId(Guid id)
        {
            _integrationId = id;
            return this;
        }

        public Mock<IModel> ModelMock { get; }

        public Mock<IConnectionFactory> ConnectionFactoryMock { get; }

        public Mock<IConnection> ConnectionMock { get; }

        internal Mock<IAmqpConsumerFactory> AmqpConsumerFactoryMock { get; }

        internal Mock<IAmqpReceiveConsumer> AmqpReceiveConsumerMock { get; }

        internal Mock<IMaskinportenClient> MaskinportenClientMock { get; }

        internal Mock<IDokumentlagerHandler> DokumentlagerHandlerMock { get; }

        internal Mock<ISendHandler> SendHandlerMock { get; }

        internal AmqpHandler CreateSut()
        {
            SetupMocks();
            return new AmqpHandler(
                MaskinportenClientMock.Object,
                SendHandlerMock.Object,
                DokumentlagerHandlerMock.Object,
                CreateConfiguration(),
                CreateIntegrationConfiguration(),
                new AccountConfiguration(_accountId, "dummy"),
                ConnectionFactoryMock.Object,
                AmqpConsumerFactoryMock.Object);
        }

        private static AmqpConfiguration CreateConfiguration()
        {
            return new AmqpConfiguration("api.fiks.test.ks.no");
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

            ConnectionFactoryMock.SetupSet(_ => _.Password = It.IsAny<string>());
            ConnectionFactoryMock.SetupSet(_ => _.UserName = It.IsAny<string>());

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

            MaskinportenClientMock.Setup(_ => _.GetAccessToken(It.IsAny<string>()))
                                  .ReturnsAsync(new MaskinportenToken(_token, 100));
        }

        private IntegrationConfiguration CreateIntegrationConfiguration()
        {
            return new IntegrationConfiguration(_integrationId, _integrationPassword);
        }
    }
}