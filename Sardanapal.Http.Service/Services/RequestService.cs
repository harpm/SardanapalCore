using Microsoft.AspNetCore.Http;
using Sardanapal.InterfacePanel.Service;

namespace Sardanapal.Http.Service.Services;

public class RequestService : IRequestService
{
    public string IP { get; set; }

    public RequestService(IHttpContextAccessor _http)
    {
        IP = _http.HttpContext.Connection.RemoteIpAddress.ToString();
    }
}
