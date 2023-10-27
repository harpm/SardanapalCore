using Sardanapal.ModelBase.Model.Types;
using Sardanapal.Share.Extensions;

namespace Sardanapal.RedisCache.Models;

public interface IChacheResponse<T>
{
    public string ServiceName { get; set; }
    public CacheStatusCode Status { get; set; }
    public string[] Messages { get; set; }
    public T Value { get; set; }

    public void Set(CacheStatusCode status);
    public void Set(CacheStatusCode status, T value);
    public void Set(CacheStatusCode status, Exception ex);
}

public class CacheResponse<T> : IChacheResponse<T>
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
    public CacheStatusCode Status { get; set; }
    public string[] Messages { get; set; }
    public T Value { get; set; }

    public void Set(CacheStatusCode status)
    {
        Status = status;
    }

    public void Set(CacheStatusCode status, T value)
    {
        Status = status;
        Value = value;
    }

    public void Set(CacheStatusCode status, Exception ex)
    {
        Status = status;
        Messages = ex.GetHirachicalMessages();
    }
}

public enum CacheStatusCode : byte
{
    Succeeded,
    NotExist,
    Failed,
    Exception
}