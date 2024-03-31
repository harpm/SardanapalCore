using Sardanapal.InterfacePanel.Service;

namespace Sardanapal.RedisCache.Services;

public interface ICacheService<TSearchVM, TVM, TNewVM, TEditableVM>
    : ICrudService<Guid, TSearchVM, TVM, TNewVM, TEditableVM>
    where TSearchVM : class, new()
    where TVM : class, new()
    where TNewVM : class, new()
    where TEditableVM : class, new()
{

}