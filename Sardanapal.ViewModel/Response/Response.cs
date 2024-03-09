using Sardanapal.Share.Extensions;

namespace Sardanapal.ViewModel.Response;

public interface IResponse<TValue>
{
    string ServiceName { get; set; }
    TValue Data { get; set; }
    OperationType OperationType { get; set; }
    StatusCode StatusCode { get; set; }
    string[] DeveloperMessage { get; set; }
    string UserMessage { get; set; }

    void Set(StatusCode statusCode);
    void Set(StatusCode statusCode, TValue data);
    void Set(StatusCode statusCode, Exception exception);
    IResponse<T> ConvertTo<T>() where T : class;
}

public class Response<TValue> : IResponse<TValue>
{
    public string ServiceName { get; set; }
    public TValue Data { get; set; }
    public OperationType OperationType { get; set; }
    public StatusCode StatusCode { get; set; }
    public string[] DeveloperMessage { get; set; }
    public string UserMessage { get; set; }

    public Response()
    {
        ServiceName = "Default";
    }

    public Response(string serviceName)
    {
        this.ServiceName = serviceName;
    }

    public Response(string serviceName, OperationType operationType)
    {
        this.ServiceName = serviceName;
        this.OperationType = operationType;
    }

    public virtual void Set(StatusCode statusCode)
    {
        StatusCode = statusCode;
    }

    public virtual void Set(StatusCode statusCode, TValue data)
    {
        Set(statusCode);
        this.Data = data;
    }

    public virtual void Set(StatusCode statusCode, Exception exception)
    {
        Set(statusCode);
        DeveloperMessage = exception.GetHirachicalMessages();
    }

    public virtual IResponse<T> ConvertTo<T>() where T : class
    {
        return Create<T>(StatusCode, ServiceName, OperationType, DeveloperMessage, UserMessage);
    }

    public static Response<T> Create<T>(string serviceName, OperationType operationType) where T : class
    {
        return new Response<T>(serviceName, operationType);
    }

    public static Response<T> Create<T>(StatusCode status, string serviceName, OperationType operationType, string[] developerMessages, string userMessage)
        where T : class
    {
        return new Response<T>(serviceName, operationType)
        {
            StatusCode = status,
            DeveloperMessage = developerMessages,
            UserMessage = userMessage
        };
    }
}

public class Response : Response<bool>
{
    public Response() : base()
    {

    }

    public Response(string serviceName) : base(serviceName)
    {

    }

    public Response(string serviceName, OperationType operationType) : base(serviceName, operationType)
    {

    }
}