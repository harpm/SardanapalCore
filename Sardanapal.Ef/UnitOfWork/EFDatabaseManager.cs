// Licensed under the MIT license.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Sardanapal.Contract.Data;

namespace Sardanapal.Ef.UnitOfWork;

public class EFDatabaseManager<TUnitOfWork> : IEFDatabaseManager
    where TUnitOfWork : DbContext, ISdUnitOfWork
{
    protected TUnitOfWork _context;
    public EFDatabaseManager(TUnitOfWork dbContext)
    {
        _context = dbContext;
    }

    public virtual IDbContextTransaction CreatTransaction()
    {
        return _context.Database.BeginTransaction();
    }

    public virtual Task<IDbContextTransaction> CreatTransactionAsync(CancellationToken ct = default)
    {
        return _context.Database.BeginTransactionAsync(ct);
    }

    public virtual int SaveChanges()
    {
        return _context.SaveChanges();
    }

    public virtual Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return _context.SaveChangesAsync(ct);
    }
}
