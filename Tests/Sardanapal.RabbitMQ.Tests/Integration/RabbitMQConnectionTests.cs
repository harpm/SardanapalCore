using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using RabbitMQ.Client;
using Sardanapal.RMQ.Services;
using Testcontainers.RabbitMq;
using Xunit;

namespace Sardanapal.RabbitMQ.Tests.Integration;
public class RabbitMQConnectionTests : IAsyncLifetime
{
    private RabbitMqContainer? _container;

    public async Task InitializeAsync()
    {
        _container = new RabbitMqBuilder()
            .WithImage("rabbitmq:3.13-management")
            .Build();
        await _container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        if (_container != null)
        {
            await _container.DisposeAsync();
        }
    }

    [DockerFact]
    public async Task TryConnect_ShouldEstablishConnection_ToRunningBroker()
    {
        // Arrange
        var factory = new ConnectionFactory
        {
            Uri = new Uri($"amqp://guest:guest@{_container!.GetConnectionString()}")
        };
        var logger = Substitute.For<ILogger<RabbitMQPersistentConnection>>();
        var connection = new RabbitMQPersistentConnection(factory, logger);

        // Act
        bool connected = await connection.TryConnect();

        // Assert
        connected.Should().BeTrue();
        connection.IsConnected.Should().BeTrue();
        connection.Dispose();
    }

    [DockerFact]
    public async Task CreateModel_ShouldReturnOpenChannel_WhenConnected()
    {
        // Arrange
        var factory = new ConnectionFactory
        {
            Uri = new Uri($"amqp://guest:guest@{_container!.GetConnectionString()}")
        };
        var logger = Substitute.For<ILogger<RabbitMQPersistentConnection>>();
        using var connection = new RabbitMQPersistentConnection(factory, logger);
        await connection.TryConnect();

        // Act
        IChannel channel = await connection.CreateModel();

        // Assert
        channel.Should().NotBeNull();
        channel.IsOpen.Should().BeTrue();
    }
}
