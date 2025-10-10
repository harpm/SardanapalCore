using Microsoft.AspNetCore.Http;
using Sardanapal.Contract.IService;
using Sardanapal.ViewModel.Response;

namespace Sardanapal.Http.Service.Services;

public abstract class RequestService<TUserKey> : IRequestService<TUserKey>
    where TUserKey : IComparable<TUserKey>, IEquatable<TUserKey>
{
    public string IP { get; set; }

    public RequestService(IHttpContextAccessor _http)
    {
        IP = _http.HttpContext.Connection.RemoteIpAddress.ToString();
    }

    public abstract IResponse<TUserKey> GetUserId(CancellationToken ct = default);
    public abstract Task<IResponse<TUserKey>> GetUserIdAsync(CancellationToken ct = default);
}
