
using Sardanapal.Share.EventArgModels;

namespace Sardanapal.Contract.IService;

public interface IIntegrationEventHandler<in TIntegrationEvent>
    where TIntegrationEvent : IntegrationEvent
{
    Task Handle(TIntegrationEvent e);
}
