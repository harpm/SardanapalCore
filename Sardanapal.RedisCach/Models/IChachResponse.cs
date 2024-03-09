using Sardanapal.Share.Extensions;
using Sardanapal.ViewModel.Response;

namespace Sardanapal.RedisCache.Models;

public interface IChacheResponse<TValue>
{
    public string ServiceName { get; set; }
    public StatusCode Status { get; set; }
    public string[] Messages { get; set; }
    public TValue Value { get; set; }

    public void Set(StatusCode status);
    public void Set(StatusCode status, TValue value);
    public void Set(StatusCode status, Exception ex);
}

public class CacheResponse<TValue> : IChacheResponse<TValue>
{
    public CacheResponse()
    {

    }

    public CacheResponse(string serviceName)
    {
        ServiceName = serviceName;
    }

    public CacheResponse(string serviceName, OperationType operation)
    {
        ServiceName = serviceName;
        Operation = operation;
    }

    public string ServiceName { get; set; }
    public OperationType Operation { get; set; }
    public StatusCode Status { get; set; }
    public string[] Messages { get; set; }
    public TValue Value { get; set; }

    public void Set(StatusCode status)
    {
        Status = status;
    }

    public void Set(StatusCode status, TValue value)
    {
        Status = status;
        Value = value;
    }

    public void Set(StatusCode status, Exception ex)
    {
        Status = status;
        Messages = ex.GetHirachicalMessages();
    }
}