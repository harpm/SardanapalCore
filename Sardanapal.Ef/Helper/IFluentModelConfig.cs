using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sardanapal.DomainModel.Domain;

namespace Sardanapal.Ef.Helper;

public abstract class FluentModelConfig<T>
    where T : class, IDomainModel
{
    public abstract void OnModelBuild(EntityTypeBuilder<T> entity);
}