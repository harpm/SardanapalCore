using Sardanapal.Share.Extensions;

#if !DEBUG
using System.ComponentModel.DataAnnotations.Schema;
#endif

namespace Sardanapal.ViewModel.Response;

public interface IResponse
{
    bool IsSuccess { get; }
    string ServiceName { get; set; }
    OperationType OperationType { get; set; }
    StatusCode StatusCode { get; set; }
    string[] DeveloperMessages { get; set; }
    string UserMessage { get; set; }

    void Set(StatusCode statusCode);
    void Set(StatusCode statusCode, Exception exception);
    void ConvertTo<T>(IResponse target);
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

public record Response<TValue> : IResponse<TValue>
{
    public virtual bool IsSuccess
    {
        get
        {
            return StatusCode == StatusCode.Succeeded;
        }
    }
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

    public virtual void Set(StatusCode statusCode, TValue data, string userMessage)
    {
        Set(statusCode);
        this.Data = data;
        UserMessage = userMessage;
    }

    public virtual void Set(StatusCode statusCode, string userMessage)
    {
        Set(statusCode);
        this.UserMessage = userMessage;
    }

    public virtual void Set(StatusCode statusCode, string[] developerMessages, string userMessage)
    {
        Set(statusCode);
        this.DeveloperMessages = developerMessages;
        this.UserMessage = userMessage;
    }

    public virtual void Set(StatusCode statusCode, Exception exception)
    {
        Set(statusCode);
        this.DeveloperMessages = exception.GetHirachicalMessages();
    }

    public virtual void Set(StatusCode statusCode, Exception exception, string userMessage)
    {
        Set(statusCode);
        this.DeveloperMessages = exception.GetHirachicalMessages();
        this.UserMessage = userMessage;
    }

    /// <summary>
    /// Convert
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public virtual void ConvertTo<T>(IResponse target)
    {
        target.StatusCode = StatusCode;
        target.ServiceName = ServiceName;
        target.OperationType = OperationType;
        target.DeveloperMessages = DeveloperMessages;
        target.UserMessage = UserMessage;
    }

    public IResponse<TValue> Fill(Action body)
    {
        var result = this as IResponse<TValue>;

        try
        {
            body();
        }
        catch (OperationCanceledException ex)
        {
            this.Set(StatusCode.Canceled);
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
        catch (OperationCanceledException ex)
        {
            this.Set(StatusCode.Canceled);
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
        catch (OperationCanceledException ex)
        {
            this.Set(StatusCode.Canceled);
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
        catch (OperationCanceledException ex)
        {
            this.Set(StatusCode.Canceled);
        }
        catch (Exception ex)
        {
            await onError(ex);
        }

        return result;
    }
}

public record Response : Response<bool>
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