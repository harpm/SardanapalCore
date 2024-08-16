
namespace Sardanapal.Contract.IService;

public interface IValidationService
{
    bool IsProceeded { get; }
    bool IsValid { get; }
    List<string> Messages { get; }
    void ValidateParams(Type[] paramTypes, object[] paramValues);
}
