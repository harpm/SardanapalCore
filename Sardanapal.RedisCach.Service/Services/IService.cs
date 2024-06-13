using Sardanapal.Domain.Model;
using Sardanapal.Interface.IService;
using Sardanapal.ViewModel.Response;

namespace Sardanapal.RedisCache.Services;

public interface ICacheService<TModel, TKey, TSearchVM, TVM, TNewVM, TEditableVM>
    : IPanelService<TKey, TSearchVM, TVM, TNewVM, TEditableVM>
    where TModel : IBaseEntityModel<TKey>, new()
    where TKey : IEquatable<TKey>, IComparable<TKey>
    where TSearchVM : class, new()
    where TVM : class, new()
    where TNewVM : class, new()
    where TEditableVM : class, new()
{
    Task<IResponse<TKey>> Add(TModel model);
}