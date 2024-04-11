using Sardanapal.Interface.IService;
using Sardanapal.RedisCach.Models;

namespace Sardanapal.RedisCache.Services;

public interface ICacheService<TKey, TSearchVM, TVM, TNewVM, TEditableVM>
    : IPanelService<TKey, TSearchVM, TVM, TNewVM, TEditableVM>
    where TKey : IEquatable<TKey>, IComparable<TKey>
    where TSearchVM : class, new()
    where TVM : class, ICachModel<TKey>, new()
    where TNewVM : class, ICachModel<TKey>, new()
    where TEditableVM : class, ICachModel<TKey>, new()
{

}