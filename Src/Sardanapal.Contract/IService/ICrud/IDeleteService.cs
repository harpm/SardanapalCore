using Sardanapal.ViewModel.Response;

namespace Sardanapal.Contract.IService.ICrud;

public interface IDeleteService<TKey>
    where TKey : IEquatable<TKey>, IComparable<TKey>
{
    Task<IResponse<bool>> Delete(TKey Id, CancellationToken ct = default);
}
