
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Sardanapal.Contract.IService;
using Sardanapal.Localization;

namespace Sardanapal.RMQ.Services;

public class RabbitMQPersistentConnection : IRabbitMQPersistentConnection
{
    protected readonly ILogger _logger;
    protected readonly IConnectionFactory _connectionFactory;
    protected IConnection _connection;
    protected bool _disposed;

    public RabbitMQPersistentConnection(IConnectionFactory connectionFactory, ILogger<RabbitMQPersistentConnection> logger)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _logger = logger;
    }

    public bool IsConnected => _connection != null && _connection.IsOpen && !_disposed;

    public Task<IChannel> CreateModel()
    {
        if (!IsConnected)
        {
            _logger.LogCritical(Messages.RabbitMQConnectionIssue);
            throw new InvalidOperationException(Messages.RabbitMQConnectionIssue);
        }
        return _connection.CreateChannelAsync();
    }

    public async Task<bool> TryConnect()
    {
        _connection = await _connectionFactory.CreateConnectionAsync();
        return IsConnected;
    }

    public void Dispose()
    {
        if (_disposed) return;

        try
        {
            _connection?.Dispose();
        }
        catch { }

        _disposed = true;
    }
}
