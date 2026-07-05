
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Sardanapal.Http.Service.Middlewares;

public class SdLoggingMiddlwere
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SdLoggingMiddlwere> _logger;

    public SdLoggingMiddlwere(RequestDelegate next, ILogger<SdLoggingMiddlwere> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var request = context.Request;
        _logger.LogInformation("Request started: {Method} {Scheme}://{Host}{Path}{QueryString}",
            request.Method,
            request.Scheme,
            request.Host,
            request.Path,
            request.QueryString);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogInformation("Request finished: {Method} {Path} responded {StatusCode} in {ElapsedMs} ms",
                request.Method,
                request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds);
        }
    }
}
