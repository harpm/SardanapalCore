
using RabbitMQ.Client;
using Sardanapal.Contract.IService;

namespace Sardanapal.RMQ.Services;

public class RabbitMQPersistentConnection : IRabbitMQPersistentConnection
{
    private readonly IConnectionFactory _connectionFactory;
    private IConnection _connection;
    private bool _disposed;

    public RabbitMQPersistentConnection(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public bool IsConnected => _connection != null && _connection.IsOpen && !_disposed;

    public Task<IChannel> CreateModel()
    {
        if (!IsConnected)
            throw new InvalidOperationException("No RabbitMQ connections are available.");
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
