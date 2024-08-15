
namespace Sardanapal.Contract.IModel;

public interface ILogicalEntityModel : IDomainModel
{
    bool IsDeleted { get; set; }
}