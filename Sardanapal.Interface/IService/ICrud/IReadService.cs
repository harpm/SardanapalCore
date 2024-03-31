
using Sardanapal.ViewModel.Models;
using Sardanapal.ViewModel.Response;

namespace Sardanapal.Interface.IService.ICrud;

public interface IReadService<TKey, TSearchVM, TVM>
    where TKey : IEquatable<TKey>, IComparable<TKey>
    where TSearchVM : class, new()
    where TVM : class, new()
{
    Task<IResponse<TVM>> Get(TKey Id);
    Task<IResponse<GridVM<T, TSearchVM>>> GetAll<T>(GridSearchModelVM<TSearchVM> SearchModel = null) where T : class;

}
