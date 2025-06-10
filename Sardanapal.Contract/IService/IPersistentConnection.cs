
using RabbitMQ.Client;

namespace Sardanapal.Contract.IService;

public interface IRabbitMQPersistentConnection : IDisposable
{
    bool IsConnected { get; }
    Task<bool> TryConnect();
    Task<IChannel> CreateModel();
}
