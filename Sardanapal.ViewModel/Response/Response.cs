using Sardanapal.Share.Extensions;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sardanapal.ViewModel.Response;

public interface IResponse
{
    string ServiceName { get; set; }
    OperationType OperationType { get; set; }
    StatusCode StatusCode { get; set; }
    string[] DeveloperMessages { get; set; }
    string UserMessage { get; set; }

    void Set(StatusCode statusCode);
    void Set(StatusCode statusCode, Exception exception);
    IResponse<T> ConvertTo<T>() where T : class;
}

public interface IResponse<TValue> : IResponse
{
    TValue Data { get; set; }
    void Set(StatusCode statusCode, TValue data);
    IResponse<TValue> Fill(Action body);
    Task<IResponse<TValue>> FillAsync(Func<Task> body);

    IResponse<TValue> Fill(Action body, Action<Exception> onError);
    Task<IResponse<TValue>> FillAsync(Func<Task> body, Func<Exception, Task> onError);

}

public class Response<TValue> : IResponse<TValue>
{
    public string ServiceName { get; set; }
    public TValue Data { get; set; }
    public OperationType OperationType { get; set; }
    public StatusCode StatusCode { get; set; }

/// <summary>
/// This field will not be sent to the client when it is in production mode
/// </summary>
#if !DEBUG
    [NotMapped]
#endif
    public string[] DeveloperMessages { get; set; }
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

    public Response(StatusCode statusCode, string serviceName, OperationType operationType, string[] developerMessages, string userMessage)
    {
        this.StatusCode = statusCode;
        this.ServiceName = serviceName;
        this.OperationType = operationType;
        this.DeveloperMessages = developerMessages;
        this.UserMessage = userMessage;
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
        DeveloperMessages = exception.GetHirachicalMessages();
    }

    public virtual IResponse<T> ConvertTo<T>() where T : class
    {
        return new Response<T>(StatusCode, ServiceName, OperationType, DeveloperMessages, UserMessage);
    }

    public IResponse<TValue> Fill(Action body)
    {
        var result = this as IResponse<TValue>;

        try
        {
            body();
        }
        catch (Exception ex)
        {
            this.Set(StatusCode.Exception, ex);
        }

        return result;
    }

    public async Task<IResponse<TValue>> FillAsync(Func<Task> body)
    {
        var result = this as IResponse<TValue>;

        try
        {
            await body();
        }
        catch (Exception ex)
        {
            this.Set(StatusCode.Exception, ex);
        }
        
        return result;
    }

    public IResponse<TValue> Fill(Action body, Action<Exception> onError)
    {
        var result = this as IResponse<TValue>;

        try
        {
            body();
        }
        catch (Exception ex)
        {
            onError(ex);
        }

        return result;
    }

    public async Task<IResponse<TValue>> FillAsync(Func<Task> body, Func<Exception, Task> onError)
    {
        var result = this as IResponse<TValue>;

        try
        {
            await body();
        }
        catch (Exception ex)
        {
            await onError(ex);
        }

        return result;
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