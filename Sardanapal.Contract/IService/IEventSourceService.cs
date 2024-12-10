using Sardanapal.Share.EventArgModels;
using Sardanapal.ViewModel.Response;

namespace Sardanapal.Contract.IService;

public interface IEventSourceService<TKey, TModel>
    where TKey : IEquatable<TKey>, IComparable<TKey>
    where TModel : new()
{
    public delegate void ESHandleEvent(object source, EventSourceEventArgs<TKey, TModel> args);
    public IResponse<TKey> Enqueue(TModel model);
    public IResponse<bool> RegisterTopic(ESHandleEvent handler);
    public IResponse<TModel> Dequeue();
}