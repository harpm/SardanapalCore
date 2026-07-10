
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Sardanapal.Localization;

namespace Sardanapal.RMQ.Services;

public class RabbitMQPersistentConnection : IRabbitMQPersistentConnection
{
    protected readonly ILogger _logger;
    protected readonly IConnectionFactory _connectionFactory;
    protected IConnection _connection;
    protected bool _disposed;

    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    private const int _retryCount = 5;

    public RabbitMQPersistentConnection(IConnectionFactory connectionFactory, ILogger<RabbitMQPersistentConnection> logger)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _logger = logger;
    }

    public bool IsConnected => _connection != null && _connection.IsOpen && !_disposed;

    public async Task<IChannel> CreateModel()
    {
        if (!IsConnected && !await TryConnect())
        {
            _logger.LogCritical(Messages.RabbitMQConnectionIssue);
            throw new InvalidOperationException(Messages.RabbitMQConnectionIssue);
        }
        return await _connection.CreateChannelAsync();
    }

    public async Task<bool> TryConnect()
    {
        if (_disposed) return false;

        await _connectionLock.WaitAsync();
        try
        {
            if (IsConnected) return true;

            for (int i = 0; i < _retryCount; i++)
            {
                try
                {
                    _connection = await _connectionFactory.CreateConnectionAsync();
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "RabbitMQ connection attempt {Attempt}/{RetryCount} failed.", i + 1, _retryCount);
                }

                if (IsConnected)
                {
                    _connection.ConnectionShutdownAsync += OnConnectionShutdownAsync;
                    _logger?.LogInformation("RabbitMQ persistent connection established.");
                    return true;
                }

                if (i < _retryCount - 1)
                {
                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, i)));
                }
            }

            _logger?.LogCritical(Messages.RabbitMQConnectionIssue);
            return false;
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    private async Task OnConnectionShutdownAsync(object sender, ShutdownEventArgs e)
    {
        if (_disposed) return;

        _logger?.LogWarning("RabbitMQ connection shut down ({Reason}). Attempting to reconnect...", e.ReplyText);
        await TryConnect();
    }

    public void Dispose()
    {
        if (_disposed) return;

        try
        {
            if (_connection != null)
            {
                _connection.ConnectionShutdownAsync -= OnConnectionShutdownAsync;
                _connection.Dispose();
            }
        }
        catch { }

        _connectionLock.Dispose();
        _disposed = true;
    }
}
