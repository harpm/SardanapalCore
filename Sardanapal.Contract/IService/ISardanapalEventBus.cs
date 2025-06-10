
using Sardanapal.Share.EventArgModels;

namespace Sardanapal.Contract.IService;

public interface ISardanapalEventBus
{
    Task Publish(IntegrationEvent e);
    Task Subscribe<T, TH>(string eventType)
        where T : IntegrationEvent
        where TH : IIntegrationEventHandler<T>, new();
}
