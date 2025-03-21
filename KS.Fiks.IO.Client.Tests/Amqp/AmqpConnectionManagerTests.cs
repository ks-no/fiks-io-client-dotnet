using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using KS.Fiks.IO.Client.Amqp;
using KS.Fiks.IO.Client.Configuration;
using Moq;
using RabbitMQ.Client;
using Xunit;

namespace KS.Fiks.IO.Client.Tests.Amqp;

public class AmqpConnectionManagerTests
{
    private readonly Mock<IConnectionFactory> _connectionFactoryMock;
    private readonly AmqpConfiguration _defaultAmqpConfiguration;
    private readonly AmqpConfiguration _customRateLimitConfig;

    public AmqpConnectionManagerTests()
    {
        _connectionFactoryMock = new Mock<IConnectionFactory>();
        var connectionMock = new Mock<IConnection>();

        // Default: 5 tokens, 2s refill per token
        _defaultAmqpConfiguration = new AmqpConfiguration("test.ks.no");

        _customRateLimitConfig = new AmqpConfiguration(
            "test.ks.no",
            rateLimitConfiguration: new RateLimitConfiguration(2, TimeSpan.FromSeconds(2)));

        _connectionFactoryMock
            .Setup(cf => cf.CreateConnectionAsync(It.IsAny<List<AmqpTcpEndpoint>>(), It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(connectionMock.Object);
    }

    [Fact]
    public async Task DefaultRateLimitAllowsUpToFiveConnectionsImmediately()
    {
        var manager = new AmqpConnectionManager(_connectionFactoryMock.Object, _defaultAmqpConfiguration);

        var immediateSuccessCount = 0;
        var immediateFailCount = 0;
        var tasks = new List<Task>();

        for (var i = 0; i < 6; i++)
        {
            var task = manager.CreateConnectionAsync(_defaultAmqpConfiguration);
            tasks.Add(task);

            if (task.IsCompletedSuccessfully)
            {
                immediateSuccessCount++;
            }
            else
            {
                immediateFailCount++;
            }
        }

        await Task.WhenAll(tasks);

        Assert.Equal(5, immediateSuccessCount);
        Assert.Equal(1, immediateFailCount);
    }

    [Fact]
    public async Task CustomRateLimitAllowsTwoConnectionsImmediatelyThenRateLimitHits()
    {
        var manager = new AmqpConnectionManager(_connectionFactoryMock.Object, _customRateLimitConfig);
        var successfulConnections = 0;
        var failedConnections = 0;

        for (var i = 0; i < 2; i++)
        {
            var task = manager.CreateConnectionAsync(_customRateLimitConfig);
            if (task.IsCompletedSuccessfully)
            {
                successfulConnections++;
            }
            else
            {
                failedConnections++;
            }
        }

        Assert.Equal(2, successfulConnections);

        var delayedTasks = new List<Task>();
        for (var i = 0; i < 2; i++)
        {
            var task = manager.CreateConnectionAsync(_customRateLimitConfig);
            delayedTasks.Add(task);

            if (task.IsCompletedSuccessfully)
            {
                successfulConnections++;
            }
            else
            {
                failedConnections++;
            }
        }

        await Task.WhenAll(delayedTasks);

        await Task.Delay(TimeSpan.FromSeconds(5));

        for (var i = 0; i < 2; i++)
        {
            var task = manager.CreateConnectionAsync(_customRateLimitConfig);
            if (task.IsCompletedSuccessfully)
            {
                successfulConnections++;
            }
            else
            {
                failedConnections++;
            }
        }

        Assert.Equal(4, successfulConnections);
        Assert.Equal(2, failedConnections);
    }
}