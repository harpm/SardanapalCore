
using Sardanapal.Contract.IModel;
using Sardanapal.ViewModel.Response;

namespace Sardanapal.Contract.IService;

public interface ICacheService<TModel, TKey, TSearchVM, TVM, TNewVM, TEditableVM>
    : IPanelService<TKey, TSearchVM, TVM, TNewVM, TEditableVM>
    where TModel : IBaseEntityModel<TKey>, new()
    where TKey : IEquatable<TKey>, IComparable<TKey>
    where TSearchVM : class, new()
    where TVM : class, ICachModel<TKey>, new()
    where TNewVM : class, ICachModel<TKey>, new()
    where TEditableVM : class, ICachModel<TKey>, new()
{
    Task<IResponse<TKey>> Add(TModel model, CancellationToken ct = default);
}