using Sardanapal.Contract.IModel;
using Sardanapal.Share.Extensions;

namespace Sardanapal.Ef.Helper;

public static class QueryableExtensions
{
    /// <summary>
    /// This extension is written to have higher perfamance than the one in the share package
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="query"></param>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <param name="lastIdentifier"></param>
    /// <returns></returns>
    public static IQueryable<T> Page<T, TKey>(this IQueryable<T> query, int pageIndex, int pageSize, TKey lastIdentifier)
        where TKey : IComparable<TKey>, IEquatable<TKey>
        where T : IBaseEntityModel<TKey>
    {
        query = query.Where(x => x.Id.CompareTo(lastIdentifier) > 0);

        return query.Page(pageIndex, pageSize);
    }
}
