using Sardanapal.DomainModel.Attributes;
using Sardanapal.ViewModel.Models;
using System.Reflection;

namespace Sardanapal.Ef.Helper
{
    public static class QueryHelper
    {
        public static IQueryable<TEntity> Search<TEntity>(this IQueryable<TEntity> query, GridSearchModelVM searchModel = null)
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
                query = query.Page(searchModel.PageIndex, searchModel.PageSize);
            }

            return query;
        }

        public static IQueryable<T> Page<T>(this IQueryable<T> query, int pageIndex, int pageSize)
        {
            return query.Skip(pageSize * pageIndex)
                    .Take(pageSize);
        }
    }
}
