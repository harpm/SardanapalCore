using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sardanapal.DomainModel.Domain;
using System.Reflection;

namespace Sardanapal.DomainModel.UnitOfWork;

public interface ISardanapalUnitOfWork
{
    Type[] GetDomainModels();
    void ApplyFluentConfigs<T>(EntityTypeBuilder entity) where T : class, IDomainModel;
}

public abstract class SardanapalUnitOfWork<TUOW> : DbContext, ISardanapalUnitOfWork
    where TUOW : DbContext, ISardanapalUnitOfWork
{
    public SardanapalUnitOfWork(DbContextOptions<TUOW> opt)
        : base(opt)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        foreach (Type t in GetDomainModels())
        {
            var entity = builder.GetType().GetMethods()
                .Where(x => x.IsGenericMethod && x.Name == nameof(ModelBuilder.Entity))
                .First()
                .MakeGenericMethod(t)
                .Invoke(builder, null);
            this.GetType().GetMethod(nameof(ApplyFluentConfigs))?.MakeGenericMethod(t).Invoke(this, new[] { entity });
        }

        base.OnModelCreating(builder);
    }

    public virtual Type[] GetDomainModels()
    {
        return Assembly.GetExecutingAssembly().GetTypes()
            .Where(x => x.IsAssignableTo(typeof(IDomainModel)))
            .ToArray();
    }

    public virtual void ApplyFluentConfigs<T>(EntityTypeBuilder entity)
        where T : class, IDomainModel
    {
        var FluentType = typeof(T).Assembly.GetTypes()
            .Where(x => x.IsAssignableTo(typeof(FluentModelConfig<>).MakeGenericType(typeof(T))))
            .FirstOrDefault();

        if (FluentType != null)
        {
            var fluentConfig = FluentType.GetConstructor(null).Invoke(null) as FluentModelConfig<T>;

            var OnModelBuild = FluentType.GetMethod("OnModelBuild");
            OnModelBuild.Invoke(null, new object[] { entity });
        }
    }

    protected virtual void SetBaseValues()
    {
        var EntityModels = ChangeTracker
            .Entries()
            .Where(e => typeof(ILogicalEntityModel).IsAssignableFrom(e.Entity.GetType()) && (e.State == EntityState.Deleted))
            .ToArray();

        foreach (var model in EntityModels)
        {
            ILogicalEntityModel entity = (ILogicalEntityModel)model.Entity;

            entity.IsDeleted = true;
            model.State = EntityState.Modified;
        }
    }
}