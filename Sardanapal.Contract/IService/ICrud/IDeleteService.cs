using Sardanapal.ViewModel.Response;

namespace Sardanapal.Interface.IService.ICrud;

public interface IDeleteService<TKey>
    where TKey : IEquatable<TKey>, IComparable<TKey>
{
    Task<IResponse<bool>> Delete(TKey Id);
}
