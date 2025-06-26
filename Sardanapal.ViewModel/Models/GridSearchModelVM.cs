
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

public abstract record GridSearchModelVM<TKey> : IGridSearchVM
    where TKey : IComparable<TKey>, IEquatable<TKey>
{
    public virtual string DynamicField { get; set; }
    public virtual string SortId { get; set; }
    public virtual bool SortAsccending { get; set; } = true;
    public virtual int PageIndex { get; set; }
    public virtual int PageSize { get; set; }
    public virtual int TotalCount { get; set; }
    public virtual TKey LastIdentifier { get; set; }
}

public record GridSearchModelVM<TKey, TSearchVM> : GridSearchModelVM<TKey>
    where TKey : IComparable<TKey>, IEquatable<TKey>
    where TSearchVM : class
{
    public override int PageSize { get; set; } = 25;
    public TSearchVM? Fields { get; set; }
}