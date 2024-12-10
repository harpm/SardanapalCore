
namespace Sardanapal.Share.EventArgModels;


public class EventSourceEventArgs<TKey, TModel> : EventArgs
{
    public required TKey Id { get; set; }
    public required TModel Model { get; set; }
}