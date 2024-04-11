using Sardanapal.Interface.IService;

namespace Sardanapal.RedisCache.Services;

public interface ICacheService<TKey, TSearchVM, TVM, TNewVM, TEditableVM>
    : IPanelService<TKey, TSearchVM, TVM, TNewVM, TEditableVM>
    where TKey : IEquatable<TKey>, IComparable<TKey>
    where TSearchVM : class, new()
    where TVM : class, new()
    where TNewVM : class, new()
    where TEditableVM : class, new()
{

}