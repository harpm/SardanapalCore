
namespace Sardanapal.ViewModel.Models;

public record GridVM<TKey, T>
    where TKey : IComparable<TKey>, IEquatable<TKey>
    where T : class
{
    public List<T> List { get; set; }
    public IGridSearchVM SearchModel { get; set; }

    public GridVM()
    {

    }

    public GridVM(GridSearchModelVM<TKey> searchModel)
    {
        SearchModel = searchModel;
    }
}