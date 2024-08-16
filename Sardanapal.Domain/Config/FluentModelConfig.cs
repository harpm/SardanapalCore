using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sardanapal.Contract.IModel;

namespace Sardanapal.Domain.Config;

public abstract class FluentModelConfig<T>
    where T : class, IDomainModel
{
    public abstract void OnModelBuild(EntityTypeBuilder<T> entity);
}