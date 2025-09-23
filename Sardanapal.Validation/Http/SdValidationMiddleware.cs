
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.Extensions.Logging;
using Sardanapal.Contract.IService;

namespace Sardanapal.Validation.Http;

public class SdValidationMiddleware
{
    private readonly RequestDelegate _next;

    public SdValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public virtual async Task InvokeAsync(HttpContext context)
    {
        ILogger logger = context?.RequestServices?.GetService(typeof(ILogger<SdValidation>)) as ILogger<SdValidation>;
        IValidationService validationService = context?.RequestServices?.GetService(typeof(IValidationService)) as IValidationService;

        var metadata = context?.GetEndpoint()?.Metadata;

        // TODO: this part needs tests
        var controller = metadata?.GetMetadata<Controller>();
        var tryUpdateModelAsyncMethodInfo = controller?.GetType().GetMethod(nameof(Controller.TryUpdateModelAsync), BindingFlags.Public);

        var action = metadata?.GetMetadata<ActionDescriptor>();

        if (validationService == null || logger == null)
        {
            throw new NullReferenceException(string.Format("Requiered Service {0} cannot be resolved."
                , nameof(IValidationService)));
        }

        if (!validationService.IsProceeded)
        {
            var parameters = action?.Parameters;

            if (parameters != null && parameters.Any())
            {
                for (var i = 0; i < parameters.Count; i++)
                {
                    if (parameters[i].ParameterType == typeof(CancellationToken))
                    {
                        continue;
                    }

                    validationService.ValidateParams(parameters.Select(p => p.ParameterType).ToArray()
                        , action.Parameters.Select(p =>
                        {
                            var instance = Activator.CreateInstance(p.ParameterType);

                            tryUpdateModelAsyncMethodInfo?.MakeGenericMethod(p.ParameterType).Invoke(controller, [instance]);

                            return instance ?? new();
                        }).ToArray());

                }
            }
        }

        await _next(context);
    }
}
