
namespace Sardanapal.Contract.IService;

public interface IValidationService
{
    bool IsProceeded { get; }
    bool IsValid { get; }
    List<string> Messages { get; }
    Task ValidateParams(Type[] paramTypes, object[] paramValues, CancellationToken ct = default);
}
