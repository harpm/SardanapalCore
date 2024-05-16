using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sardanapal.Domain.Model;

namespace Sardanapal.Domain;

public abstract class FluentModelConfig<T>
    where T : class, IDomainModel
{
    public abstract void OnModelBuild(EntityTypeBuilder<T> entity);
}