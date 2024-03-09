using Microsoft.AspNetCore.Http;

namespace Sardanapal.InterfacePanel.Service
{
    public interface IRequestService
    {
        string IP { get; set; }
    }

    public class RequestService : IRequestService
    {
        public string IP { get; set; }

        public RequestService(IHttpContextAccessor _http)
        {
            
        }
    }
}
