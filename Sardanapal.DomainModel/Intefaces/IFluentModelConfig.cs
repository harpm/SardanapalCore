using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sardanapal.DomainModel.Domain;

namespace Sardanapal.DomainModel;

public abstract class FluentModelConfig<T>
    where T : class, IDomainModel
{
    public abstract void OnModelBuild(EntityTypeBuilder<T> entity);
}