using System.Linq.Expressions;

namespace HotChocolate.Data.ExpressionNodes;
// TODO:
// Since things like this effectively have no dependencies at all,
// we need to cache them once for the lifetime.
public sealed class InstanceExpressionFactory : IExpressionFactory
{
    public static readonly InstanceExpressionFactory Instance = new();

    public Expression GetExpression(IExpressionCompilationContext context)
    {
        var name = "x" + context.NodeId.Value;
        return Expression.Variable(context.ExpectedExpressionType, name);
    }
}
