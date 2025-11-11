using System.Reflection;
using Sardanapal.Domain.Attributes;
using Sardanapal.ViewModel.Models;
using Sardanapal.Share.Extensions;
using Sardanapal.Contract.IModel;

namespace Sardanapal.Ef.Helper;

public static class QueryHelper
{
    public static IQueryable<TEntity> Search<TKey, TEntity>(this IQueryable<TEntity> query, GridSearchModelVM<TKey> searchModel = null)
        where TKey : IComparable<TKey>, IEquatable<TKey>
        where TEntity : IBaseEntityModel<TKey>
    {
        if (searchModel == null)
            return query;

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
            //Expression<Func<TEntity, object>> propertySelectorExpr = Expression.Lambda<Func<TEntity, object>>(propertyExpr);

            if (searchModel.SortAsccending)
            {
                query = query.OrderBy(x => x.GetType().GetProperty(searchModel.SortId).GetValue(x));
                //query = query.OrderBy(propertySelectorExpr);
            }
            else
            {
                query = query.OrderByDescending(x => x.GetType().GetProperty(searchModel.SortId).GetValue(x));
                //query = query.OrderByDescending(propertySelectorExpr);
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
}
