using Sardanapal.DomainModel.Attributes;
using Sardanapal.ViewModel.Models;
using System.Reflection;

namespace Sardanapal.Ef.Helper
{
    public static class QueryHelper
    {
        public static IQueryable<TEntity> Search<TEntity>(IQueryable<TEntity> query, GridSearchModelVM searchModel = null)
            where TEntity : class
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
                query = query
                    .Skip(searchModel.PageSize * searchModel.PageIndex)
                    .Take(searchModel.PageSize);
            }

            return query;
        }
    }
}
