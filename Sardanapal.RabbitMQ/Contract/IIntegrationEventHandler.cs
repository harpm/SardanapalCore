
using Sardanapal.Share.EventArgModels;

namespace Sardanapal.RMQ;

public interface IIntegrationEventHandler<in TIntegrationEvent>
    where TIntegrationEvent : IntegrationEvent
{
    Task Handle(TIntegrationEvent e);
}
