
namespace Sardanapal.ViewModel.Models;

public abstract class GridSearchModelVM
{
    public string SortId { get; set; }
    public bool SortAsccending { get; set; } = true;
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
}

public class GridSearchModelVM<TSearchVM> : GridSearchModelVM
    where TSearchVM : class
{
    public TSearchVM? Fields { get; set; }
}