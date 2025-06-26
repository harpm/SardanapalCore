using System.Reflection;
using Sardanapal.Domain.Attributes;
using Sardanapal.ViewModel.Models;
using Sardanapal.Share.Extensions;
using Sardanapal.Contract.IModel;

namespace Sardanapal.Ef.Helper;

public static class QueryHelper
{
    public static IEnumerable<TEntity> Search<TKey, TEntity>(this IEnumerable<TEntity> query, GridSearchModelVM<TKey> searchModel = null)
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
            query = query.OrderBy(x => x.GetType().GetProperty(searchModel.SortId).GetValue(x));
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
