using Sardanapal.ViewModel.Models;
using Sardanapal.ViewModel.Response;

namespace Sardanapal.Contract.IService;

public interface IPanelService<TKey, TSearchVM, TVM, TNewVM, TEditableVM>
    : ICrudService<TKey, TSearchVM, TVM, TNewVM, TEditableVM>
    where TKey : IEquatable<TKey>, IComparable<TKey>
    where TSearchVM : class, new()
    where TVM : class, new()
    where TNewVM : class, new()
    where TEditableVM : class, new()
{
    Task<IResponse<GridVM<TKey, SelectOptionVM<TKey, object>>>> GetDictionary(GridSearchModelVM<TKey, TSearchVM> SearchModel = null, CancellationToken ct = default);
}
