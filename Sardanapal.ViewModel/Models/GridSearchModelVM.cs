
namespace Sardanapal.ViewModel.Models;

public interface IGridSearchVM
{
    string DynamicField { get; set; }
    string SortId { get; set; }
    bool SortAsccending { get; set; }
    int PageIndex { get; set; }
    int PageSize { get; set; }
    int TotalCount { get; set; }
}

public abstract class GridSearchModelVM<TKey> : IGridSearchVM
    where TKey : IComparable<TKey>, IEquatable<TKey>
{
    public string DynamicField { get; set; }
    public string SortId { get; set; }
    public bool SortAsccending { get; set; } = true;
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public TKey LastIdentifier { get; set; }
}

public class GridSearchModelVM<TKey, TSearchVM> : GridSearchModelVM<TKey>
    where TKey : IComparable<TKey>, IEquatable<TKey>
    where TSearchVM : class
{
    public TSearchVM? Fields { get; set; }
}