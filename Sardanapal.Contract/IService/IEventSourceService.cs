using Sardanapal.Share.EventArgModels;
using Sardanapal.ViewModel.Response;

namespace Sardanapal.Contract.IService;

public delegate void ESHandleEvent<TKey, TModel>(object source, EventSourceEventArgs<TKey, TModel> args)
    where TKey : IEquatable<TKey>, IComparable<TKey>
    where TModel : new();

public interface IEventSourceService<TKey, TModel>
    where TKey : IEquatable<TKey>, IComparable<TKey>
    where TModel : new()
{
    public Task<IResponse<TKey>> Enqueue(OperationType queue, TModel model);
    public Task<IResponse<bool>> RegisterTopic(OperationType queueName, ESHandleEvent<TKey, TModel> handler);
    public Task<IResponse<TModel>> Dequeue(OperationType queue);
}