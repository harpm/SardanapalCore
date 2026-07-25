// Licensed under the MIT license.

using System.Reflection;
using Sardanapal.Contract.IModel;
using Sardanapal.ViewModel.Models;
using Sardanapal.Share.Extensions;
using Sardanapal.Domain.Attributes;
using System.Linq.Expressions;

namespace Sardanapal.Service.Utilities;

public static class EnumerableHelper
{
    public static IEnumerable<TEntity> Search<TKey, TEntity>(this IEnumerable<TEntity> query, GridSearchModelVM<TKey> searchModel = null)
        where TKey : IComparable<TKey>, IEquatable<TKey>
        where TEntity : IBaseEntityModel<TKey>
    {
        if (searchModel == null)
            return query;

        string sortId = searchModel.SortId;
        if (string.IsNullOrWhiteSpace(sortId))
        {
            var opt = (EntityOptions)typeof(TEntity).GetCustomAttribute(typeof(EntityOptions));
            if (opt != null && !string.IsNullOrWhiteSpace(opt.OrderBy))
            {
                sortId = opt.OrderBy;
            }
        }

        bool orderedById = false;
        if (!string.IsNullOrWhiteSpace(sortId))
        {
            var property = typeof(TEntity).GetProperty(sortId);
            if (property != null)
            {
                orderedById = string.Equals(sortId, nameof(IBaseEntityModel<TKey>.Id), StringComparison.OrdinalIgnoreCase);

                var paramExpr = Expression.Parameter(typeof(TEntity), "x");
                var propertyExpr = Expression.PropertyOrField(paramExpr, sortId);
                Func<TEntity, object> propertySelectorFunc = Expression.Lambda<Func<TEntity, object>>(propertyExpr, paramExpr).Compile();

                if (searchModel.SortAscending)
                {
                    query = query.OrderBy(propertySelectorFunc);
                }
                else
                {
                    query = query.OrderByDescending(propertySelectorFunc);
                }
            }
        }

        if (searchModel.PageSize > 0)
        {
            // Keyset paging (Id > LastIdentifier) is only valid when ordering by Id;
            // for any other sort column it excludes arbitrary rows, so fall back to offset paging.
            if (orderedById && searchModel.LastIdentifier != null)
            {
                query = query.Page(searchModel.PageIndex, searchModel.PageSize, searchModel.LastIdentifier);
            }
            else
            {
                query = query.Page(searchModel.PageIndex, searchModel.PageSize);
            }
        }

        return query;
    }

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
    public static IEnumerable<T> Page<T, TKey>(this IEnumerable<T> query, int pageIndex, int pageSize, TKey lastIdentifier)
        where TKey : IComparable<TKey>, IEquatable<TKey>
        where T : IBaseEntityModel<TKey>
    {
        query = query.Where(x => x.Id.CompareTo(lastIdentifier) > 0);

        return query.Page(pageIndex, pageSize);
    }
}
