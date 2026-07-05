
using RabbitMQ.Client;

namespace Sardanapal.RMQ;

public interface IRabbitMQPersistentConnection : IDisposable
{
    bool IsConnected { get; }
    Task<bool> TryConnect();
    Task<IChannel> CreateModel();
}
