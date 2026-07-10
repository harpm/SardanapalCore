using System.Linq.Expressions;

namespace Sardanapal.Share.Expressions;

public static class ExpressionHelper
{
    public static Expression Find(this Expression body, Func<Expression, bool> predicate)
    {
        if (predicate(body))
        {
            return body;
        }
        else
        {
            if (body.CanReduce)
            {
                return body.Reduce().Find(predicate);
            }
            else
            {
                return null; 
            }
        }
    }
}
