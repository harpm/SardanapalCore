using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Reflection;

namespace Sardanapal.Share.Extensions;

public static class IEnumerableExtensions
{
    public static IEnumerable<T> DynamicSearch<T>(this IEnumerable<T> list, string searchKeyword)
    {
        var fields = typeof(T).GetProperties()
            .Where(x => !x.GetCustomAttributes()
            .Any(a => a.GetType() == typeof(NotMappedAttribute)))
            .ToArray();

        ParameterExpression xParam = Expression.Parameter(typeof(T), "x");
        ConstantExpression searchKeywordExpression = Expression.Constant(searchKeyword);
        var strContainsMethod = typeof(string).GetMethods()
            .Where(x => x.Name == nameof(string.Contains) && x.GetParameters().Length == 1
                && x.GetParameters().First().ParameterType == typeof(string))
            .First();

        Expression finalPredicate = null;

        for (int i = 0; i < fields.Length; i++)
        {
            // gets the ToString method info for this iterations field
            var tostringMethod = fields[i].PropertyType.GetMethods()
                .Where(x => x.Name == nameof(ToString)).First();

            // defines entry parameter of the final lambda expression

            // extract the field from the T type model
            MemberExpression fieldExpression = Expression.PropertyOrField(xParam, fields[i].Name);

            // calls the ToString method to get the string form of the current field
            MethodCallExpression fieldToStrExpression = Expression.Call(fieldExpression, tostringMethod);

            // calls the Contains method of the string form of the current field
            // with input of the dynamicField parameter
            MethodCallExpression containsCallExpression = Expression.Call(fieldToStrExpression, strContainsMethod, searchKeywordExpression);

            // finally convert the whole expression into lambda expression
            var predicate = Expression.Lambda<Func<T, bool>>(containsCallExpression, xParam);

            if (finalPredicate == null)
            {
                finalPredicate = predicate;
            }
            else
            {
                finalPredicate = Expression.Or(finalPredicate, predicate);
            }
        }

        if (finalPredicate != null)
        {
            var lambda = Expression.Lambda<Func<T, bool>>(finalPredicate, xParam);

            list.Where(lambda.Compile());
        }

        return list;
    }
}
