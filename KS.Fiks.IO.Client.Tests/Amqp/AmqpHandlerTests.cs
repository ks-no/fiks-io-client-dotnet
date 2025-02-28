using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KS.Fiks.IO.Client.Exceptions;
using KS.Fiks.IO.Client.Models;
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
        public void CreatesModelWhenConstructed()
        {
            var sut = _fixture.CreateSutAsync();

            _fixture.ConnectionFactoryMock.Verify(
                factory =>
                    factory.CreateConnectionAsync(
                        It.IsAny<IList<AmqpTcpEndpoint>>(),
                        It.IsAny<string>(),
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

            _fixture.AmqpConsumerFactoryMock.Verify(
                consumerFactory =>
                    consumerFactory.CreateReceiveConsumer(It.IsAny<IChannel>()),
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
    }
}