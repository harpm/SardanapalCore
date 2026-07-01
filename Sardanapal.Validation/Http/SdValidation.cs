using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Sardanapal.Contract.IService;
using Sardanapal.ViewModel.Response;

namespace Sardanapal.Validation.Http;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class SdValidation : ActionFilterAttribute
{
    public SdValidation()
    {

    }

    public override void OnActionExecuting(ActionExecutingContext action)
    {
        base.OnActionExecuting(action);

        try
        {
            ILogger logger = action.HttpContext.RequestServices.GetService(typeof(ILogger<SdValidation>)) as ILogger<SdValidation>;
            IValidationService validationService = action.HttpContext.RequestServices.GetService(typeof(IValidationService)) as IValidationService;
            if (validationService == null)
                throw new NullReferenceException(string.Format("Requiered Service {0} cannot be resolved."
                    , nameof(IValidationService)));

            if (!validationService.IsProceeded)
            {
                var parameters = action.ActionDescriptor.Parameters;

                if (parameters != null && parameters.Any())
                {
                    var paramTypes = new List<Type>();
                    var paramValues = new List<object>();

                    foreach (var parameter in parameters)
                    {
                        if (parameter.ParameterType == typeof(CancellationToken))
                        {
                            continue;
                        }

                        if (!action.ActionArguments.TryGetValue(parameter.Name, out object value))
                        {
                            continue;
                        }

                        paramTypes.Add(parameter.ParameterType);
                        paramValues.Add(value);
                    }

                    if (paramTypes.Count > 0)
                    {
                        validationService.ValidateParams(paramTypes.ToArray(), paramValues.ToArray())
                            .GetAwaiter().GetResult();
                    }
                }
            }
            if (!validationService.IsValid)
            {
                logger?.LogDebug($"Messages: {string.Join(", ", validationService.Messages)}");

                IResponse<object> response = new Response<object>(nameof(SdValidation), logger)
                {
                    StatusCode = StatusCode.Exception,
                    UserMessage = string.Join("\r\n", validationService.Messages.ToArray())
                };
                action.Result = new BadRequestObjectResult(response);
            }
        }
        catch (Exception ex)
        {

            action.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            action.Result = new BadRequestResult();
        }
    }
}
