using System.Linq.Expressions;

namespace HotChocolate.Data.ExpressionNodes;

// This is needed in order to rewrap the instance.
// It doesn't make sense for the instance to suddenly point at the inner argument in Select.
// rootInstance => child[0](instance)
public sealed class ProjectionLambda : IExpressionFactory
{
    public Expression GetExpression(IExpressionCompilationContext context)
    {
        var instance = context.Expressions.InstanceRoot!;
        var projection = context.Expressions.Children[0];
        var lambda = Expression.Lambda(projection, instance);
        return lambda;
    }
}
