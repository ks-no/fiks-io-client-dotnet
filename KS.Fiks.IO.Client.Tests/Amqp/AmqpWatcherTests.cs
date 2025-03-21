using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KS.Fiks.IO.Client.Amqp;
using Microsoft.Extensions.Logging;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Xunit;

namespace KS.Fiks.IO.Client.Tests.Amqp
{
    public class DefaultAmqpWatcherTests
    {
        private readonly Mock<ILogger<DefaultAmqpWatcher>> _loggerMock;
        private readonly DefaultAmqpWatcher _watcher;

        public DefaultAmqpWatcherTests()
        {
            _loggerMock = new Mock<ILogger<DefaultAmqpWatcher>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();

            loggerFactoryMock
                .Setup(factory => factory.CreateLogger(It.IsAny<string>()))
                .Returns(_loggerMock.Object);

            _watcher = new DefaultAmqpWatcher(loggerFactoryMock.Object);
        }

        [Fact]
        public async Task HandleConnectionBlockedLogsWarning()
        {
            var eventArgs = new ConnectionBlockedEventArgs("Test reason");

            await _watcher.HandleConnectionBlocked(this, eventArgs);

            MockLoggerAssertions.VerifyLog(_loggerMock, LogLevel.Warning, "RabbitMQ Connection Blocked: Test reason", Times.Once());
        }

        [Fact]
        public async Task HandleConnectionUnblockedLogsInformation()
        {
            await _watcher.HandleConnectionUnblocked(this, new AsyncEventArgs());

            MockLoggerAssertions.VerifyLog(_loggerMock, LogLevel.Information, "RabbitMQ Connection Unblocked", Times.Once());
        }

        [Fact]
        public async Task HandleConnectionShutdownLogsWarning()
        {
            var eventArgs = new ShutdownEventArgs(ShutdownInitiator.Peer, 0, "Shutdown occurred");

            await _watcher.HandleConnectionShutdown(this, eventArgs);

            MockLoggerAssertions.VerifyLog(_loggerMock, LogLevel.Warning, "RabbitMQ Connection Shutdown: Shutdown occurred", Times.Once());
        }

        [Fact]
        public async Task HandleConnectionRecoveryErrorLogsError()
        {
            var exception = new Exception("Recovery error");

            await _watcher.HandleConnectionRecoveryError(this, new ConnectionRecoveryErrorEventArgs(exception));

            MockLoggerAssertions.VerifyLog(_loggerMock, LogLevel.Error, "RabbitMQ Connection Recovery Failed", exception, Times.Once());
        }

        [Fact]
        public async Task HandleRecoverySucceededLogsInformation()
        {
            await _watcher.HandleRecoverySucceeded(this, new AsyncEventArgs());

            MockLoggerAssertions.VerifyLog(_loggerMock, LogLevel.Information, "RabbitMQ Connection Recovery Succeeded", Times.Once());
        }

        [Fact]
        public async Task HandleRecoveringConsumerLogsInformation()
        {
            var consumerTag = "test-consumer";
            var consumerArguments = new Dictionary<string, object>();
            var eventArgs = new RecoveringConsumerEventArgs(consumerTag, consumerArguments, CancellationToken.None);

            await _watcher.HandleRecoveringConsumer(this, eventArgs);

            MockLoggerAssertions.VerifyLog(_loggerMock, LogLevel.Information, $"RabbitMQ Recovering Consumer: {consumerTag}", Times.Once());
        }

        [Fact]
        public async Task HandleChannelShutdownLogsWarning()
        {
            var eventArgs = new ShutdownEventArgs(ShutdownInitiator.Library, 404, "Channel shutdown");

            await _watcher.HandleChannelShutdown(this, eventArgs);

            MockLoggerAssertions.VerifyLog(_loggerMock, LogLevel.Warning, "RabbitMQ Channel Shutdown: Channel shutdown", Times.Once());
        }

        [Fact]
        public async Task HandleBasicChannelCancelLogsWarning()
        {
            await _watcher.HandleBasicChannelCancel("test-consumer");

            MockLoggerAssertions.VerifyLog(_loggerMock, LogLevel.Warning, "RabbitMQ Consumer Cancelled: test-consumer", Times.Once());
        }

        [Fact]
        public async Task HandleBasicChannelCancelOkLogsInformation()
        {
            await _watcher.HandleBasicChannelCancelOk("test-consumer");

            MockLoggerAssertions.VerifyLog(_loggerMock, LogLevel.Information, "RabbitMQ Consumer Cancellation Acknowledged: test-consumer", Times.Once());
        }

        [Fact]
        public async Task HandleBasicChannelConsumeOkLogsInformation()
        {
            await _watcher.HandleBasicChannelConsumeOk("test-consumer");

            MockLoggerAssertions.VerifyLog(_loggerMock, LogLevel.Information, "RabbitMQ Consumer Successfully Started: test-consumer", Times.Once());
        }
    }

    public static class MockLoggerAssertions
    {
        public static void VerifyLog<T>(Mock<ILogger<T>> mockLogger, LogLevel logLevel, string expectedMessage, Times times)
        {
            mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(lvl => lvl == logLevel),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(expectedMessage, StringComparison.OrdinalIgnoreCase)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                times);
        }

        public static void VerifyLog<T>(
            Mock<ILogger<T>> mockLogger,
            LogLevel logLevel,
            string expectedMessage,
            Exception expectedException,
            Times times)
        {
            mockLogger.Verify(
                x => x.Log(
                    logLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(expectedMessage, StringComparison.OrdinalIgnoreCase)),
                    It.Is<Exception>(ex => ex == expectedException),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                times);
        }
    }
}