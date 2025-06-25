
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.Extensions.Logging;

namespace Sardanapal.Http.Service.Middlewares;

public class SdHandleExceptionMiddlwere
{
    private readonly RequestDelegate _next;
    
    public SdHandleExceptionMiddlwere(RequestDelegate next)
    {
        _next = next;
    }

    protected virtual Task ProcessResponse(HttpContext context)
    {
        var logger = context?.RequestServices?.GetService(typeof(ILogger));

        if (logger == null) throw new NullReferenceException(nameof(logger));

        var metadata = context?.GetEndpoint()?.Metadata;
        if (metadata != null)
        {
            var action = metadata.GetMetadata<ActionDescriptor>();
            // Get action return type
            // check if it is assignable to IResponse
            // check if its status is exception
            // change the status code of the response to 500 (Internal error)
        }

        return Task.CompletedTask;
    }
}
