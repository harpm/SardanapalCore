using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sardanapal.Contract.IModel;

namespace Sardanapal.Ef.UnitOfWork;

public interface ISdUnitOfWork
{
    Type[] GetDomainModels();
    void ApplyFluentConfigs<T>(EntityTypeBuilder<T> entity) where T : class, IDomainModel;
}
