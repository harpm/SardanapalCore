using System.Reflection;
using System.Linq.Expressions;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sardanapal.Share.Extensions;

public static class IEnumerableExtensions
{
    public static IEnumerable<TListItemVM> SelectDynamicColumns<TListItemVM>(this IEnumerable<TListItemVM> list, string[]? columns)
    {
        if (columns == null || columns.Length == 0)
        {
            return list;
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

        return list.Select(lambda.Compile());
    }

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

    /// <summary>
    /// Searches all the fields in the T class
    /// inside the queryable
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="query"></param>
    /// <param name="searchKeyword"></param>
    /// <returns> The input queryable with applied dynamic field search </returns>
    public static IEnumerable<T> SearchDynamic<T>(this IEnumerable<T> query, string searchKeyword)
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

        Expression predication = null;
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

            if (predication == null)
            {
                predication = containsCallExpression;
            }
            else
            {
                predication = Expression.Or(predication, containsCallExpression);
            }
        }

        if (predication != null)
        {
            // finally convert the whole expression into lambda expression
            var predicate = Expression.Lambda<Func<T, bool>>(predication, xParam);

            // and apply the entire condition expression in a where clause chained with Or (not And)
            // This should be WhereOr
            query = query.Where(predicate.Compile());
        }

        return query;
    }

    public static IEnumerable<T> Page<T>(this IEnumerable<T> query, int pageIndex, int pageSize)
    {
        return query.Skip(pageSize * pageIndex)
                .Take(pageSize);
    }
}
