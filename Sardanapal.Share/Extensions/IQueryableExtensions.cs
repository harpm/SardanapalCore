using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Reflection;
using Sardanapal.Share.Expressions;

namespace Sardanapal.Share.Extensions;

public static class IQueryableExtensions
{
    public static IQueryable<T> WhereOr<T>(this IQueryable<T> query, Expression<Func<T, bool>> predications)
    {
        if (predications != null)
        {
            Expression topestBinaryExp = query.Expression.Find(x => x.Type == typeof(BinaryExpression));

            if (topestBinaryExp == null)
            {
                query = query.Where(predications);
            }
            else
            {
                topestBinaryExp = Expression.Or(topestBinaryExp, predications);
                var cond = Expression.Lambda<Func<T, bool>>(topestBinaryExp);
                query = query.Where(cond);
            }
            return query;
        }
        else
        {
            throw new NullReferenceException();
        }
    }

    /// <summary>
    /// Searches all the fields in the T class
    /// inside the queryable
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="query"></param>
    /// <param name="searchKeyword"></param>
    /// <returns> The input queryable with applied dynamic field search </returns>
    public static IQueryable<T> SearchDynamic<T>(this IQueryable<T> query, string searchKeyword)
    {
        // This method is written to fit every unknown domain models
        // so this is why we use expression base code to make it usable for any domain model

        var fields = typeof(T).GetProperties()
            .Where(x => !x.GetCustomAttributes()
                .Any(a => a.GetType() == typeof(NotMappedAttribute)));

        // defines entry parameter of the final lambda expression
        ParameterExpression xParam = Expression.Parameter(typeof(T), "x");

        // cach constant expression of the dynamicField (it is constant for each iteration)
        ConstantExpression searchKeywordExpression = Expression.Constant(searchKeyword);

        // also the Contains method info is constant for each iteration
        var strContainsMethod = typeof(string).GetMethods()
            .Where(x => x.Name == nameof(string.Contains) && x.GetParameters().Length == 1
                && x.GetParameters().First().ParameterType == typeof(string))
            .First();

        foreach (var field in fields)
        {
            // gets the ToString method info for this iterations field
            var tostringMethod = field.PropertyType.GetMethods()
                .Where(x => x.Name == nameof(ToString)).First();

            // extract the field from the T type model
            MemberExpression fieldExpression = Expression.PropertyOrField(xParam, field.Name);

            // calls the ToString method to get the string form of the current field
            MethodCallExpression fieldToStrExpression = Expression.Call(fieldExpression, tostringMethod);

            // calls the Contains method of the string form of the current field
            // with input of the dynamicField parameter
            MethodCallExpression containsCallExpression = Expression.Call(fieldToStrExpression, strContainsMethod, searchKeywordExpression);

            // finally convert the whole expression into lambda expression
            var predicate = Expression.Lambda<Func<T, bool>>(containsCallExpression, xParam);

            // and apply the entire condition expression in a where clause chained with Or (not And)
            // This should be WhereOr
            query = query.Where(predicate);
        }

        return query;
    }

    public static IQueryable<T> Page<T>(this IQueryable<T> query, int pageIndex, int pageSize)
    {
        return query.Skip(pageSize * pageIndex)
                .Take(pageSize);
    }
}