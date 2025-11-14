using System.Reflection;
using System.Linq.Expressions;
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
            var paramExpr = Expression.Parameter(typeof(TEntity), "x");
            var propertyAccessExpr = Expression.Property(paramExpr, searchModel.SortId);
            Expression<Func<TEntity, object>> propertySelectorExpr = Expression.Lambda<Func<TEntity, object>>(propertyAccessExpr);

            if (searchModel.SortAsccending)
            {
                //query = query.OrderBy(x => x.GetType().GetProperty(searchModel.SortId).GetValue(x));
                query = query.OrderBy(propertySelectorExpr);
            }
            else
            {
                //query = query.OrderByDescending(x => x.GetType().GetProperty(searchModel.SortId).GetValue(x));
                query = query.OrderByDescending(propertySelectorExpr);
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
    public static IQueryable<TListItemVM> SelectDynamicColumns<TListItemVM>(this IQueryable<TListItemVM> query, string[]? columns)
        where TListItemVM : class
    {
        if (columns == null || columns.Length == 0)
        {
            return query;
        }
        var listType = typeof(TListItemVM);

        var parameter = Expression.Parameter(listType, "x");

        var bindings = new List<MemberBinding>();
        foreach (var column in columns)
        {
            var property = listType.GetProperty(column);
            if (property != null)
            {
                var propertyAccess = Expression.Property(parameter, property);
                var binding = Expression.Bind(property, propertyAccess);
                bindings.Add(binding);
            }
        }
        var body = Expression.MemberInit(Expression.New(listType), bindings);
        var lambda = Expression.Lambda<Func<TListItemVM, TListItemVM>>(body, parameter);
        return query.Select(lambda);
    }
}
