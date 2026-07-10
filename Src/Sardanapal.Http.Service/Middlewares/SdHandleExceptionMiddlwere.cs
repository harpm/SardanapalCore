
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Sardanapal.Localization;
using Sardanapal.ViewModel.Response;

namespace Sardanapal.Http.Service.Middlewares;

public class SdHandleExceptionMiddlwere
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SdHandleExceptionMiddlwere> _logger;
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public SdHandleExceptionMiddlwere(RequestDelegate next, ILogger<SdHandleExceptionMiddlwere> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // Client cancelled the request; nothing useful to return.
            _logger.LogWarning("Request cancelled by client: {Method} {Path}",
                context.Request.Method, context.Request.Path);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        _logger.LogError(ex, "Unhandled exception for {Method} {Path}",
            context.Request.Method, context.Request.Path);

        if (context.Response.HasStarted)
        {
            throw new InvalidOperationException(
                "The response has already started; the exception response cannot be written.", ex);
        }

        var response = new Response<object>(nameof(SdHandleExceptionMiddlwere), _logger);
        response.Set(StatusCode.Exception, ex, Messages.InternalError);

        context.Response.Clear();
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        context.Response.ContentType = "application/json; charset=utf-8";

        await JsonSerializer.SerializeAsync(context.Response.Body, response, _jsonOptions);
    }
}
