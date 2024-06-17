
namespace Sardanapal.ViewModel.Models;

public class GridVM<TKey, T, S>
    where TKey : IComparable<TKey>, IEquatable<TKey>
    where T : class
    where S : class
{
    public List<T> List { get; set; }
    public GridSearchModelVM<TKey, S> SearchModel { get; set; }

    public GridVM()
    {

    }

    public GridVM(GridSearchModelVM<TKey, S> searchModel)
    {
        SearchModel = searchModel;
    }
}