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

        //TODO: Add Domain as reference
        if (string.IsNullOrWhiteSpace(searchModel.SortId))
        {
            var opt = (EntityOptions)typeof(TEntity).GetCustomAttribute(typeof(EntityOptions));
            if (opt != null)
            {
                searchModel.SortId = opt.OrderBy;
            }
        }

        if (!string.IsNullOrWhiteSpace(searchModel.SortId))
        {
            //var paramExpr = Expression.Parameter(typeof(TEntity), "x");
            //var propertyExpr = Expression.PropertyOrField(paramExpr, searchModel.SortId);
            //Func<TEntity, object> propertySelectorFund = Expression.Lambda<Func<TEntity, object>>(propertyExpr).Compile();

            if (searchModel.SortAsccending)
            {
                //query = query.OrderBy(propertySelectorFund);
                query = query.OrderBy(x => x.GetType().GetProperty(searchModel.SortId).GetValue(x));
            }
            else
            {
                query = query.OrderByDescending(x => x.GetType().GetProperty(searchModel.SortId).GetValue(x));
            }
        }

        if (searchModel.PageSize > 0)
        {
            if (searchModel.LastIdentifier != null)
            {
                query = query.Page(searchModel.PageIndex, searchModel.PageSize, searchModel.LastIdentifier);
            }
            else
            {
                query = query.Page(searchModel.PageIndex, searchModel.PageIndex);
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
