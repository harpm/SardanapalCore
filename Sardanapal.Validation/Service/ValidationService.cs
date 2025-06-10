
using FluentValidation;
using Sardanapal.Contract.IService;

namespace Sardanapal.Validation.Service;

public class ValidationService : IValidationService
{
    protected virtual IServiceProvider _serviceProvider { get; set; }

    protected List<string> _messages = new List<string>();
    public virtual List<string> Messages
    {
        get
        {
            return _messages;
        }
    }

    protected bool _isValidGenerally = true;

    public virtual bool IsValid
    {
        get => _isValidGenerally;
    }

    protected bool _isProceeded = false;

    public virtual bool IsProceeded
    {
        get => _isProceeded;
    }


    public ValidationService(IServiceProvider sp)
    {
        _serviceProvider = sp;
    }

    public virtual async void ValidateParams(Type[] paramTypes, object[] paramValues, CancellationToken ct = default)
    {
        if (_isProceeded) return;
        else _isProceeded = true;
        if (paramTypes == null || paramValues == null) return;
        if (paramTypes.Length != paramValues.Length)
        {
            _isValidGenerally = false;
            return;
        }
        else
        {
            for (int i = 0; i < paramTypes.Length; i++)
            {
                if (ct.IsCancellationRequested) throw new OperationCanceledException(ct);

                await ValidateEachParam(paramTypes[i], paramValues[i]);
            }
        }
    }

    protected virtual async Task ValidateEachParam(Type paramType, object paramValue)
    {
        var nestedMembers = paramType.GetProperties()
            .Where(m => !m.PropertyType.IsPrimitive
                && m.PropertyType.IsClass)
            .Select(x => new { MType = x.PropertyType, MValue = x.GetValue(paramValue) })
            .Union(paramType.GetFields()
                .Where(m => !m.IsStatic
                    && m.IsPublic
                    && !m.FieldType.IsPrimitive
                    && m.FieldType.IsClass)
                .Select(x => new { MType = x.FieldType, MValue = x.GetValue(paramValue) }))
            .ToArray();

        for (int i = 0; i < nestedMembers.Length; i++)
        {
            if (nestedMembers[i].MType == null)
                continue;

            var nMemberType = nestedMembers[i].MType;
            var nMemberValue = nestedMembers[i].MValue;

            var nValidatorType = typeof(IValidator<>)
            .MakeGenericType(nestedMembers[i].MType);

            var nValidator = (IValidator)_serviceProvider.GetService(nValidatorType);

            if (nValidator != null)
            {
                var nValidationContext = new ValidationContext<object>(nMemberValue);
                var nValidateResult = await nValidator.ValidateAsync(nValidationContext);
                if (nValidateResult == null
                    || !nValidateResult.IsValid)
                {
                    await ValidateEachParam(nMemberType, nMemberValue);
                }
            }
        }

        var validatorType = typeof(IValidator<>)
            .MakeGenericType(paramType);
        var validator = (IValidator)_serviceProvider.GetService(validatorType);

        if (validator != null)
        {
            var validationContext = new ValidationContext<object>(paramValue);

            var validationRes = await validator.ValidateAsync(validationContext);

            Messages.AddRange(validationRes.Errors.Select(e => e.ErrorMessage));

            if (_isValidGenerally && !validationRes.IsValid)
                _isValidGenerally = validationRes.IsValid;
        }
    }
}