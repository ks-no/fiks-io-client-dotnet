using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using KS.Fiks.IO.Client.Exceptions;
using KS.Fiks.IO.Client.Models;
using Microsoft.Extensions.Logging;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shouldly;
using Xunit;

namespace KS.Fiks.IO.Client.Tests.Amqp
{
    public class AmqpHandlerTests
    {
        private readonly AmqpHandlerFixture _fixture;

        public AmqpHandlerTests()
        {
            _fixture = new AmqpHandlerFixture();
        }

        [Fact]
        public async Task CreatesConnectionAndChannelWhenConstructed()
        {
            var sut = await _fixture.CreateSutAsync().ConfigureAwait(false);

            _fixture.ConnectionFactoryMock.Verify(
                factory => factory.CreateConnectionAsync(
                    It.IsAny<IList<AmqpTcpEndpoint>>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _fixture.ConnectionMock.Verify(
                connection => connection.CreateChannelAsync(
                    It.IsAny<CreateChannelOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task ThrowsExceptionWhenConnectionFactoryThrows()
        {
            await Assert.ThrowsAsync<FiksIOAmqpConnectionFailedException>(() =>
                _fixture.WhereConnectionFactoryThrowsException().CreateSutAsync());
        }

        [Fact]
        public async Task ThrowsExceptionWhenConnectionThrows()
        {
            await Assert.ThrowsAsync<FiksIOAmqpConnectionFailedException>(() =>
                _fixture.WhereConnectionThrowsException().CreateSutAsync());
        }

        [Fact]
        public async Task AddReceivedListenerCreatesNewConsumer()
        {
            var sut = await _fixture.CreateSutAsync();

            var handler = new Func<MottattMeldingArgs, Task>(_ => Task.CompletedTask);
            var cancelledHandler = new Func<ConsumerEventArgs, Task>(_ => Task.CompletedTask);

            await sut.AddMessageReceivedHandlerAsync(handler, cancelledHandler);

            _fixture.ConnectionMock.Verify(
                conn => conn.CreateChannelAsync(It.IsAny<CreateChannelOptions>(), It.IsAny<CancellationToken>()),
                Times.Once);

            _fixture.AmqpConsumerFactoryMock.Verify(
                consumerFactory => consumerFactory.CreateReceiveConsumer(It.IsAny<IChannel>()),
                Times.Once);
        }

        [Fact]
        public async Task AddReceivedListenerAddsHandlerToReceivedEvent()
        {
            var sut = await _fixture.CreateSutAsync();
            var counter = 0;

            Func<MottattMeldingArgs, Task> handler = args =>
            {
                counter++;
                return Task.CompletedTask;
            };

            await sut.AddMessageReceivedHandlerAsync(handler, null);

            await _fixture.AmqpReceiveConsumerMock.RaiseAsync(
                consumer => consumer.ReceivedAsync += null,
                new MottattMeldingArgs(null, null));

            counter.ShouldBe(1);
        }

        [Fact]
        public async Task AddReceivedListenerAddsHandlerToListenOnCanceledEventAsync()
        {
            var sut = await _fixture.CreateSutAsync();
            var counter = 0;
            Func<ConsumerEventArgs, Task> asyncHandler = args =>
            {
                counter++;
                return Task.CompletedTask;
            };

            await sut.AddMessageReceivedHandlerAsync(null, asyncHandler);

            await _fixture.AmqpReceiveConsumerMock.RaiseAsync(
                consumer => consumer.ConsumerCancelledAsync += null,
                new ConsumerEventArgs(new[] { "TestConsumerTag" }));

            counter.ShouldBe(1);
        }

        [Fact]
        public async Task IsOpenAsyncReturnsTrueWhenConnectionAndChannelAreOpen()
        {
            _fixture.ConnectionMock.Setup(conn => conn.IsOpen).Returns(true);
            _fixture.ChannelMock.Setup(channel => channel.IsOpen).Returns(true);

            var sut = await _fixture.CreateSutAsync();

            var result = await sut.IsOpenAsync();

            result.ShouldBe(true);
        }

        [Fact]
        public async Task IsOpenAsyncReturnsFalseWhenConnectionIsClosed()
        {
            _fixture.ConnectionMock.Setup(conn => conn.IsOpen).Returns(false);
            _fixture.ChannelMock.Setup(channel => channel.IsOpen).Returns(true);

            var sut = await _fixture.CreateSutAsync();

            var result = await sut.IsOpenAsync();

            result.ShouldBe(false);
        }

        [Fact]
        public async Task IsOpenAsyncReturnsFalseWhenChannelIsClosed()
        {
            _fixture.ConnectionMock.Setup(conn => conn.IsOpen).Returns(true);
            _fixture.ChannelMock.Setup(channel => channel.IsOpen).Returns(false);

            var sut = await _fixture.CreateSutAsync();

            var result = await sut.IsOpenAsync();

            result.ShouldBe(false);
        }

        [Fact]
        public async Task DisposeAsyncDisposesChannelAndConnection()
        {
            var sut = await _fixture.CreateSutAsync();

            _fixture.ChannelMock.Setup(channel => channel.DisposeAsync()).Returns(ValueTask.CompletedTask);
            _fixture.ConnectionMock.Setup(connection => connection.DisposeAsync()).Returns(ValueTask.CompletedTask);

            await sut.DisposeAsync();

            _fixture.ChannelMock.Verify(channel => channel.DisposeAsync(), Times.Once);
            _fixture.ConnectionMock.Verify(connection => connection.DisposeAsync(), Times.Once);
        }

        [Fact]
        public async Task DisposeAsync_UnsubscribesAllEvents()
        {
            var sut = await _fixture.CreateSutAsync();
            var connectionMock = _fixture.ConnectionMock;

            Func<MottattMeldingArgs, Task> receivedHandler = _ => Task.CompletedTask;
            Func<ConsumerEventArgs, Task> cancelledHandler = _ => Task.CompletedTask;

            await sut.AddMessageReceivedHandlerAsync(receivedHandler, cancelledHandler);

            _fixture.AmqpReceiveConsumerMock.VerifyAdd(
                consumer => consumer.ReceivedAsync += It.IsAny<Func<MottattMeldingArgs, Task>>(),
                Times.Once);

            _fixture.AmqpReceiveConsumerMock.VerifyAdd(
                consumer => consumer.ConsumerCancelledAsync += It.IsAny<Func<ConsumerEventArgs, Task>>(),
                Times.Once);

            _fixture.ConnectionMock.Setup(c => c.IsOpen).Returns(true);

            connectionMock.SetupRemove(connection =>
                connection.ConnectionShutdownAsync -= It.IsAny<AsyncEventHandler<ShutdownEventArgs>>());

            connectionMock.SetupRemove(connection =>
                connection.ConnectionBlockedAsync -= It.IsAny<AsyncEventHandler<ConnectionBlockedEventArgs>>());

            connectionMock.SetupRemove(connection =>
                connection.ConnectionUnblockedAsync -= It.IsAny<AsyncEventHandler<AsyncEventArgs>>());

            connectionMock.SetupRemove(connection =>
                connection.RecoverySucceededAsync -= It.IsAny<AsyncEventHandler<AsyncEventArgs>>());

            connectionMock.SetupRemove(connection =>
                connection.RecoveringConsumerAsync -= It.IsAny<AsyncEventHandler<RecoveringConsumerEventArgs>>());

            connectionMock.SetupRemove(connection =>
                connection.ConnectionRecoveryErrorAsync -=
                    It.IsAny<AsyncEventHandler<ConnectionRecoveryErrorEventArgs>>());

            await sut.DisposeAsync();

            _fixture.AmqpReceiveConsumerMock.VerifyRemove(
                consumer => consumer.ReceivedAsync -= receivedHandler,
                Times.Once);

            _fixture.AmqpReceiveConsumerMock.VerifyRemove(
                consumer => consumer.ConsumerCancelledAsync -= cancelledHandler,
                Times.Once);

            connectionMock.VerifyRemove(
                connection => connection.ConnectionShutdownAsync -= It.IsAny<AsyncEventHandler<ShutdownEventArgs>>(),
                Times.AtLeastOnce);

            connectionMock.VerifyRemove(
                connection => connection.ConnectionBlockedAsync -=
                    It.IsAny<AsyncEventHandler<ConnectionBlockedEventArgs>>(),
                Times.AtLeastOnce);

            connectionMock.VerifyRemove(
                connection => connection.ConnectionUnblockedAsync -= It.IsAny<AsyncEventHandler<AsyncEventArgs>>(),
                Times.AtLeastOnce);

            connectionMock.VerifyRemove(
                connection => connection.RecoverySucceededAsync -= It.IsAny<AsyncEventHandler<AsyncEventArgs>>(),
                Times.AtLeastOnce);

            connectionMock.VerifyRemove(
                connection => connection.RecoveringConsumerAsync -=
                    It.IsAny<AsyncEventHandler<RecoveringConsumerEventArgs>>(),
                Times.AtLeastOnce);

            connectionMock.VerifyRemove(
                connection => connection.ConnectionRecoveryErrorAsync -=
                    It.IsAny<AsyncEventHandler<ConnectionRecoveryErrorEventArgs>>(),
                Times.AtLeastOnce);

            _fixture.ChannelMock.Verify(c => c.DisposeAsync(), Times.Once);
            _fixture.ConnectionMock.Verify(c => c.DisposeAsync(), Times.Once); 
        }

        [Fact]
        public async Task DisposeAsync_HandlesObjectDisposedExceptionWhenUnsubscribingConnectionEvents()
        {
            var sut = await _fixture.CreateSutAsync();

            _fixture.ConnectionMock.Setup(c => c.IsOpen).Throws(new ObjectDisposedException("MockedObject"));
            _fixture.ConnectionMock.Setup(c => c.DisposeAsync()).Returns(ValueTask.CompletedTask);
            _fixture.ChannelMock.Setup(c => c.DisposeAsync()).Returns(ValueTask.CompletedTask);

            var exceptionCaught = false;

            try
            {
                await sut.DisposeAsync();
            }
            catch
            {
                exceptionCaught = true;
            }

            exceptionCaught.ShouldBeFalse("DisposeAsync should handle ObjectDisposedException internally.");

            _fixture.ConnectionMock.Verify(c => c.DisposeAsync(), Times.Once);
            _fixture.ChannelMock.Verify(c => c.DisposeAsync(), Times.Once);
        }

        [Fact]
        public async Task DisposeAsync_CanBeCalledMultipleTimesSafely()
        {
            var sut = await _fixture.CreateSutAsync();

            _fixture.ConnectionMock.Setup(c => c.IsOpen).Returns(true);
            _fixture.ChannelMock.Setup(c => c.DisposeAsync()).Returns(ValueTask.CompletedTask);
            _fixture.ConnectionMock.Setup(c => c.DisposeAsync()).Returns(ValueTask.CompletedTask);

            await sut.DisposeAsync();
            await sut.DisposeAsync();

            _fixture.ChannelMock.Verify(c => c.DisposeAsync(), Times.Once);
            _fixture.ConnectionMock.Verify(c => c.DisposeAsync(), Times.Once);
        }

        [Fact]
        public async Task DisposeAsync_HandlesNullConnectionGracefully()
        {
            var sut = await _fixture.CreateSutAsync();

            _fixture.SetConnectionToNull();
            _fixture.ChannelMock.Setup(c => c.DisposeAsync()).Returns(ValueTask.CompletedTask);

            var exceptionCaught = false;
            try
            {
                await sut.DisposeAsync();
            }
            catch
            {
                exceptionCaught = true;
            }

            exceptionCaught.ShouldBeFalse("DisposeAsync should handle null connection gracefully.");

            _fixture.ChannelMock.Verify(c => c.DisposeAsync(), Times.Once);
        }

        [Fact]
        public async Task DisposeAsync_SkipsEventUnsubscriptionWhenConnectionIsClosed()
        {
            var sut = await _fixture.CreateSutAsync();

            _fixture.ConnectionMock.Setup(c => c.IsOpen).Returns(false);
            _fixture.ChannelMock.Setup(c => c.DisposeAsync()).Returns(ValueTask.CompletedTask);
            _fixture.ConnectionMock.Setup(c => c.DisposeAsync()).Returns(ValueTask.CompletedTask);

            await sut.DisposeAsync();

            _fixture.ConnectionMock.VerifyRemove(
                c => c.ConnectionShutdownAsync -= It.IsAny<AsyncEventHandler<ShutdownEventArgs>>(),
                Times.Never);
        }

        [Fact]
        public async Task DisposeAsync_HandlesObjectDisposedExceptionWhenUnsubscribingEvents()
        {
            var sut = await _fixture.CreateSutAsync();

            _fixture.ConnectionMock.Setup(c => c.IsOpen).Returns(true);
            _fixture.ConnectionMock
                .SetupRemove(c => c.ConnectionShutdownAsync -= It.IsAny<AsyncEventHandler<ShutdownEventArgs>>())
                .Throws(new ObjectDisposedException("MockedConnection"));
            _fixture.ChannelMock.Setup(c => c.DisposeAsync()).Returns(ValueTask.CompletedTask);
            _fixture.ConnectionMock.Setup(c => c.DisposeAsync()).Returns(ValueTask.CompletedTask);

            var exceptionCaught = false;

            try
            {
                await sut.DisposeAsync();
            }
            catch
            {
                exceptionCaught = true;
            }

            exceptionCaught.ShouldBeFalse("DisposeAsync should handle ObjectDisposedException internally.");
        }
    }
}