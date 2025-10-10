using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Sardanapal.Contract.IService;
using Sardanapal.ViewModel.Response;

namespace Sardanapal.Http.Service.Services;

public abstract class RequestService<TUserKey> : IRequestService<TUserKey>
    where TUserKey : IComparable<TUserKey>, IEquatable<TUserKey>
{
    protected readonly ILogger _logger;

    public string IP { get; set; }

    public RequestService(IHttpContextAccessor _http, ILogger logger)
    {
        IP = _http.HttpContext.Connection.RemoteIpAddress.ToString();
        this._logger = logger;
    }

    public abstract IResponse<TUserKey> GetUserId(CancellationToken ct = default);
    public abstract Task<IResponse<TUserKey>> GetUserIdAsync(CancellationToken ct = default);
}
