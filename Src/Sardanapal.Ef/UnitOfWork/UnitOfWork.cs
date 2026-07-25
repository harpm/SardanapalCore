
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sardanapal.Contract.IModel;
using Sardanapal.Domain.Config;

namespace Sardanapal.Ef.UnitOfWork;

public abstract class SardanapalUnitOfWork : DbContext, ISdUnitOfWork
{
    public SardanapalUnitOfWork(DbContextOptions opt)
        : base(opt)
    {
        base.SavingChanges += SetBaseValues;
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
            this.GetType().GetMethod(nameof(ApplyFluentConfigs))?.MakeGenericMethod(t).Invoke(this, [entity]);

            if (typeof(ILogicalEntityModel).IsAssignableFrom(t))
            {
                this.GetType().GetMethod(nameof(ApplySoftDeleteFilter))?.MakeGenericMethod(t).Invoke(this, [entity]);
            }
        }

        base.OnModelCreating(builder);
    }

    public virtual void ApplySoftDeleteFilter<T>(EntityTypeBuilder<T> entity)
        where T : class, ILogicalEntityModel
    {
        entity.HasQueryFilter(e => !((ILogicalEntityModel)e).IsDeleted);
    }

    public virtual Type[] GetDomainModels()
    {
        return GetType().Assembly.GetTypes()
            .Where(x => x.IsAssignableTo(typeof(IDomainModel)) && x.IsClass && !x.IsAbstract)
            .ToArray();
    }

    public virtual void ApplyFluentConfigs<T>(EntityTypeBuilder<T> entity)
        where T : class, IDomainModel
    {
        var FluentType = typeof(T).Assembly.GetTypes()
            .Where(x => x.IsAssignableTo(typeof(FluentModelConfig<>).MakeGenericType(typeof(T))))
            .FirstOrDefault();

        if (FluentType != null)
        {
            var fluentConfig = FluentType.GetConstructor(Array.Empty<Type>())?.Invoke(null) as FluentModelConfig<T>;

            if (fluentConfig == null) throw new InvalidOperationException(FluentType.FullName);

            var OnModelBuild = FluentType.GetMethod(nameof(FluentModelConfig<T>.OnModelBuild));
            OnModelBuild?.Invoke(fluentConfig, [entity]);
        }
    }

    protected virtual void SetBaseValues(object? sender, SavingChangesEventArgs e)
    {
        object currentUserKey = GetCurrentUserKey();

        foreach (var entry in ChangeTracker.Entries().ToArray())
        {
            var entityType = entry.Entity.GetType();
            var originalState = entry.State;

            if (typeof(ILogicalEntityModel).IsAssignableFrom(entityType) && originalState == EntityState.Deleted)
            {
                ILogicalEntityModel logicalEntity = (ILogicalEntityModel)entry.Entity;
                logicalEntity.IsDeleted = true;
                entry.State = EntityState.Modified;
            }

            var auditInterface = entityType.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType
                    && i.GetGenericTypeDefinition() == typeof(IEntityModel<,>));

            if (auditInterface != null)
            {
                var nowUtc = DateTime.UtcNow;
                var createdOnProp = entityType.GetProperty(nameof(IEntityModel<int, int>.CreatedOnUtc));
                var modifiedOnProp = entityType.GetProperty(nameof(IEntityModel<int, int>.ModifiedOnUtc));
                var createByProp = entityType.GetProperty(nameof(IEntityModel<int, int>.CreateBy));
                var modifiedByProp = entityType.GetProperty(nameof(IEntityModel<int, int>.ModifiedBy));

                if (originalState == EntityState.Added)
                {
                    createdOnProp?.SetValue(entry.Entity, nowUtc);
                    modifiedOnProp?.SetValue(entry.Entity, nowUtc);
                    if (currentUserKey != null)
                    {
                        createByProp?.SetValue(entry.Entity, currentUserKey);
                        modifiedByProp?.SetValue(entry.Entity, currentUserKey);
                    }
                }
                else if (entry.State == EntityState.Modified)
                {
                    modifiedOnProp?.SetValue(entry.Entity, nowUtc);
                    if (currentUserKey != null)
                    {
                        modifiedByProp?.SetValue(entry.Entity, currentUserKey);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Returns the current user key used to fill audit fields
    /// (CreateBy/ModifiedBy). Default is null (no user context).
    /// Override in a derived DbContext to supply the authenticated user.
    /// </summary>
    protected virtual object GetCurrentUserKey()
    {
        return null;
    }

    public override void Dispose()
    {
        this.SavingChanges -= SetBaseValues;
        base.Dispose();
    }

    public override ValueTask DisposeAsync()
    {
        this.SavingChanges -= SetBaseValues;
        return base.DisposeAsync();
    }
}
