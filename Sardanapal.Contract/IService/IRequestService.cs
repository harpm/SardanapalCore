
using Sardanapal.ViewModel.Response;

namespace Sardanapal.Contract.IService;

public interface IRequestService
{
    string IP { get; set; }
}

public interface IRequestService<TUserKey>
    where TUserKey : IComparable<TUserKey>, IEquatable<TUserKey>
{
    public abstract IResponse<TUserKey> GetUserId(CancellationToken ct = default);
    public abstract Task<IResponse<TUserKey>> GetUserIdAsync(CancellationToken ct = default);
}
