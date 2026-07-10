// EFRepositoryBase and EF services require a DbContext, so there is no pure unit
// (DB-free) test surface. All EF tests run against the EF Core InMemory provider
// under Integration/.
namespace Sardanapal.Ef.Tests.Unit;
