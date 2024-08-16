
using Sardanapal.ViewModel.Models;
using Sardanapal.ViewModel.Response;

namespace Sardanapal.Contract.IService.ICrud;

public interface IReadService<TKey, TSearchVM, TVM>
    where TKey : IEquatable<TKey>, IComparable<TKey>
    where TSearchVM : class, new()
    where TVM : class, new()
{
    Task<IResponse<TVM>> Get(TKey Id);
    Task<IResponse<GridVM<TKey, T, TSearchVM>>> GetAll<T>(GridSearchModelVM<TKey, TSearchVM> SearchModel = null) where T : class;

}
