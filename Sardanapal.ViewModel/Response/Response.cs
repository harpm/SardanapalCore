using Microsoft.Extensions.Logging;
using Sardanapal.Localization;
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
    void Set(StatusCode statusCode, string userMessage);
    void Set(StatusCode statusCode, string[] developerMessages, string userMessage);
    void Set(StatusCode statusCode, Exception exception);
    void Set(StatusCode statusCode, Exception exception, string userMessage);
    void Set(StatusCode statusCode, Exception exception, string[] developerMessages, string userMessage);
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
    protected ILogger _logger;

    public string ServiceName { get; set; }
    public TValue Data { get; set; }
    public OperationType OperationType { get; set; }
    public StatusCode StatusCode { get; set; }
    public string[] DeveloperMessages { get; set; }
    public string UserMessage { get; set; }

    public Response(ILogger logger)
    {
        ServiceName = "Default";
        this._logger = logger;
    }

    public Response(string serviceName, ILogger logger) : this(logger)
    {
        this.ServiceName = serviceName;
    }

    public Response(string serviceName, OperationType operationType, ILogger logger) : this(serviceName, logger)
    {
        this.OperationType = operationType;
    }

    public Response(StatusCode statusCode, string serviceName, OperationType operationType, string[] developerMessages, string userMessage, ILogger logger)
        : this(serviceName, operationType, logger)
    {
        this.StatusCode = statusCode;
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

    public virtual void Set(StatusCode statusCode, Exception exception, string[] developerMessages, string userMessage)
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
            this._logger?.LogWarning(ex, Messages.OperationCancelled);
            this.Set(StatusCode.Canceled, [], Messages.OperationCancelled);
        }
        catch (Exception ex)
        {
            this._logger?.LogError(ex, Messages.InternalError);
            this.Set(StatusCode.Exception, ex, Messages.InternalError);
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
            this._logger?.LogWarning(ex, Messages.OperationCancelled);
            this.Set(StatusCode.Canceled, [], Messages.OperationCancelled);
        }
        catch (Exception ex)
        {
            this._logger?.LogError(ex, Messages.InternalError);
            this.Set(StatusCode.Exception, ex, Messages.InternalError);
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
            this._logger?.LogWarning(ex, Messages.OperationCancelled);
            this.Set(StatusCode.Canceled, [], Messages.OperationCancelled);
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
            this._logger?.LogWarning(ex, Messages.OperationCancelled);
            this.Set(StatusCode.Canceled, [], Messages.OperationCancelled);
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
    public Response(ILogger logger) : base(logger)
    {

    }

    public Response(string serviceName, ILogger logger) : base(serviceName, logger)
    {

    }

    public Response(string serviceName, OperationType operationType, ILogger logger) : base(serviceName, operationType, logger)
    {

    }
}
