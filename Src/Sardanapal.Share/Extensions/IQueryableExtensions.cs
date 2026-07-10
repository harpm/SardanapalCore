using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Reflection;

namespace Sardanapal.Share.Extensions;

public static class IQueryableExtensions
{
    public static IQueryable<T> WhereOr<T>(this IQueryable<T> query, Expression<Func<T, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        var finder = new LastWhereFinder();
        var lastWhere = finder.Find(query.Expression);

        if (lastWhere == null)
            return query.Where(predicate);

        var predicateArg = lastWhere.Arguments[1];
        LambdaExpression lastLambda = predicateArg is UnaryExpression u && u.NodeType == ExpressionType.Quote
            ? (LambdaExpression)u.Operand
            : (LambdaExpression)predicateArg;

        var sharedParam = lastLambda.Parameters[0];
        var rebinder = new ParameterReplacer(predicate.Parameters[0], sharedParam);
        var combinedBody = Expression.OrElse(lastLambda.Body, rebinder.Visit(predicate.Body));
        var combinedLambda = Expression.Lambda<Func<T, bool>>(combinedBody, sharedParam);

        var newWhere = Expression.Call(
            typeof(Queryable), nameof(Queryable.Where), new[] { typeof(T) },
            lastWhere.Arguments[0],
            Expression.Quote(combinedLambda));

        var replacer = new NodeReplacer(lastWhere, newWhere);
        return query.Provider.CreateQuery<T>(replacer.Visit(query.Expression));
    }

    private sealed class LastWhereFinder : ExpressionVisitor
    {
        private MethodCallExpression _where;

        public MethodCallExpression Find(Expression root)
        {
            _where = null;
            Visit(root);
            return _where;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (_where == null
                && node.Method.DeclaringType == typeof(Queryable)
                && node.Method.Name == nameof(Queryable.Where))
            {
                _where = node;
                return node;
            }
            return base.VisitMethodCall(node);
        }
    }

    private sealed class ParameterReplacer : ExpressionVisitor
    {
        private readonly ParameterExpression _from;
        private readonly ParameterExpression _to;

        public ParameterReplacer(ParameterExpression from, ParameterExpression to)
        {
            _from = from;
            _to = to;
        }

        protected override Expression VisitParameter(ParameterExpression node)
            => node == _from ? _to : base.VisitParameter(node);
    }

    private sealed class NodeReplacer : ExpressionVisitor
    {
        private readonly Expression _target;
        private readonly Expression _replacement;

        public NodeReplacer(Expression target, Expression replacement)
        {
            _target = target;
            _replacement = replacement;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
            => node == _target ? _replacement : base.VisitMethodCall(node);
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
