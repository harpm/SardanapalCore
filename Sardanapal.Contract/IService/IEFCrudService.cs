
using Sardanapal.Contract.IService.ICrud;

namespace Sardanapal.Contract.IService;

public interface IEFCrudService<TKey, TSearchVM, TVM, TNewVM, TEditableVM>
    : ICreateService<TKey, TNewVM>
    , IReadService<TKey, TSearchVM, TVM>
    , IUpdateService<TKey, TEditableVM>
    , IDeleteService<TKey>
    where TKey : IEquatable<TKey>, IComparable<TKey>
    where TSearchVM : class, new()
    where TVM : class, new()
    where TNewVM : class, new()
    where TEditableVM : class, new()
{
}
