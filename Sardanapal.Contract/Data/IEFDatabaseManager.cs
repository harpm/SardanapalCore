// Licensed under the MIT license.

using Microsoft.EntityFrameworkCore.Storage;

namespace Sardanapal.Contract.Data;

public interface IEFDatabaseManager
{
    IDbContextTransaction CreatTransaction();
    Task<IDbContextTransaction> CreatTransactionAsync(CancellationToken ct = default);
    int SaveChanges();
    Task<int> SaveChangesAsync(CancellationToken ct = default);

}
