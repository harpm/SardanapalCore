using Microsoft.AspNetCore.Http;
using Sardanapal.Contract.IService;

namespace Sardanapal.Http.Service.Services;

public abstract class RequestService<TUserKey> : IRequestService<TUserKey>
    where TUserKey : IComparable<TUserKey>, IEquatable<TUserKey>
{
    public string IP { get; set; }

    public RequestService(IHttpContextAccessor _http)
    {
        IP = _http.HttpContext.Connection.RemoteIpAddress.ToString();
    }

    public abstract TUserKey GetUserId();
}
