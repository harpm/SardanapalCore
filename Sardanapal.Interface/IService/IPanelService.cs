using Sardanapal.InterfacePanel.Service;
using Sardanapal.ViewModel.Models;
using Sardanapal.ViewModel.Response;

namespace Sardanapal.Interface.IService;

public interface IPanelService<TKey, TSearchVM, TVM, TNewVM, TEditableVM>
    : ICrudService<TKey, TSearchVM, TVM, TNewVM, TEditableVM>
    where TKey : IEquatable<TKey>, IComparable<TKey>
    where TSearchVM : class, new()
    where TVM : class, new()
    where TNewVM : class, new()
    where TEditableVM : class, new()
{
    Task<IResponse<GridVM<TKey, SelectOptionVM<TKey, object>, TSearchVM>>> GetDictionary(GridSearchModelVM<TKey, TSearchVM> SearchModel = null);
}
