#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KS.Fiks.IO.Client.Amqp;
using KS.Fiks.IO.Client.Configuration;
using KS.Fiks.IO.Client.Dokumentlager;
using KS.Fiks.IO.Client.Models;
using KS.Fiks.IO.Client.Send;
using KS.Fiks.IO.Send.Client.Configuration;
using Ks.Fiks.Maskinporten.Client;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace KS.Fiks.IO.Client.Tests.Amqp
{
    public class AmqpHandlerFixture
    {
        private readonly Guid _accountId = Guid.NewGuid();
        private bool _connectionFactoryShouldThrow;
        private bool _connectionShouldThrow;
        private string _token = "testtoken";
        private Guid _integrationId = Guid.NewGuid();
        private string _integrationPassword = "defaultPassword";

        public AmqpHandlerFixture WhereConnectionFactoryThrowsException()
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

        public Mock<IConnectionFactory> ConnectionFactoryMock { get; } = new Mock<IConnectionFactory>();

        public Mock<IConnection> ConnectionMock { get; } = new Mock<IConnection>();

        internal Mock<IAmqpConsumerFactory> AmqpConsumerFactoryMock { get; } = new Mock<IAmqpConsumerFactory>();

        internal Mock<IAmqpReceiveConsumer> AmqpReceiveConsumerMock { get; } = new Mock<IAmqpReceiveConsumer>();

        internal Mock<IMaskinportenClient> MaskinportenClientMock { get; } = new Mock<IMaskinportenClient>();

        internal Mock<IDokumentlagerHandler> DokumentlagerHandlerMock { get; } = new Mock<IDokumentlagerHandler>();

        internal Mock<ISendHandler> SendHandlerMock { get; } = new Mock<ISendHandler>();

        internal async Task<IAmqpHandler> CreateSutAsync()
        {
            SetupMocks();
            var amqpConfiguration = CreateConfiguration();
            var amqpHandler = await AmqpHandler.CreateAsync(
                MaskinportenClientMock.Object,
                SendHandlerMock.Object,
                DokumentlagerHandlerMock.Object,
                amqpConfiguration,
                CreateIntegrationConfiguration(),
                new KontoConfiguration(_accountId, "dummy"),
                null,
                ConnectionFactoryMock.Object,
                AmqpConsumerFactoryMock.Object).ConfigureAwait(false);

            return amqpHandler;
        }

        private static AmqpConfiguration CreateConfiguration()
        {
            return new AmqpConfiguration("api.fiks.test.ks.no");
        }

        private void SetupMocks()
        {
            ConnectionMock
                .Setup(connection => connection.CreateChannelAsync(It.IsAny<CreateChannelOptions?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Mock<IChannel>().Object);
            if (_connectionFactoryShouldThrow)
            {
                ConnectionFactoryMock
                    .Setup(factory => factory.CreateConnectionAsync(
                        It.IsAny<IList<AmqpTcpEndpoint>>(),
                        It.IsAny<string>(),
                        It.IsAny<CancellationToken>()))
                    .Throws<ProtocolViolationException>();
            }
            else
            {
                ConnectionFactoryMock
                    .Setup(factory => factory.CreateConnectionAsync(
                        It.IsAny<IList<AmqpTcpEndpoint>>(),
                        It.IsAny<string>(),
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync(ConnectionMock.Object);
            }

            ConnectionFactoryMock.SetupSet(connection => connection.Password = It.IsAny<string>());
            ConnectionFactoryMock.SetupSet(connection => connection.UserName = It.IsAny<string>());

            if (_connectionShouldThrow)
            {
                ConnectionMock
                    .Setup(connection => connection.CreateChannelAsync(It.IsAny<CreateChannelOptions?>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new ProtocolViolationException("Simulated channel creation failure"));
            }
            else
            {
                ConnectionMock
                    .Setup(connection => connection.CreateChannelAsync(It.IsAny<CreateChannelOptions?>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new Mock<IChannel>().Object);
            }

            AmqpConsumerFactoryMock
                .Setup(consumerFactory => consumerFactory.CreateReceiveConsumer(It.IsAny<IChannel>()))
                .Returns(AmqpReceiveConsumerMock.Object);

            MaskinportenClientMock
                .Setup(maskinportenClient => maskinportenClient.GetAccessToken(It.IsAny<string>()))
                .ReturnsAsync(new MaskinportenToken(_token, 100));

            // Mock the async ConsumerCancelledAsync invocation
            AmqpReceiveConsumerMock
                .SetupAdd(amqpReceiveConsumer => amqpReceiveConsumer.ConsumerCancelledAsync += It.IsAny<Func<ConsumerEventArgs, Task>>());

            // Mock the async ReceivedAsync event subscription
            AmqpReceiveConsumerMock
                .SetupAdd(amqpReceiveConsumer => amqpReceiveConsumer.ReceivedAsync += It.IsAny<Func<MottattMeldingArgs, Task>>());
        }

        private IntegrasjonConfiguration CreateIntegrationConfiguration()
        {
            return new IntegrasjonConfiguration(_integrationId, _integrationPassword);
        }
    }
}