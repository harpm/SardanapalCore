
using Sardanapal.ViewModel.Models;
using Sardanapal.ViewModel.Response;

namespace Sardanapal.Contract.IService.ICrud;

public interface IReadService<TKey, TSearchVM, TVM>
    where TKey : IEquatable<TKey>, IComparable<TKey>
    where TSearchVM : class, new()
    where TVM : class, new()
{
    Task<IResponse<TVM>> Get(TKey Id, CancellationToken ct = default);
    Task<IResponse<GridVM<TKey, T>>> GetAll<T>(GridSearchModelVM<TKey, TSearchVM> SearchModel = null, CancellationToken ct = default) where T : class;

}
