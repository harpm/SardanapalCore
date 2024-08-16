using Microsoft.AspNetCore.Http;
using Sardanapal.Contract.IService;

namespace Sardanapal.Http.Service.Services;

public class RequestService : IRequestService
{
    public string IP { get; set; }

    public RequestService(IHttpContextAccessor _http)
    {
        IP = _http.HttpContext.Connection.RemoteIpAddress.ToString();
    }
}
