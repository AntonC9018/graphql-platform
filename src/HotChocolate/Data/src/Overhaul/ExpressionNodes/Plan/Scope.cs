namespace HotChocolate.Data.ExpressionNodes;

public sealed class Scope
{
    public Scope? ParentScope { get; set; }

    // This indicates the root node that gets you the instance expression.
    public ExpressionNode RootInstance => Instance!.InnermostInitialNode;

    // This one can be wrapped
    public ExpressionNode? Instance { get; set; }

    public ExpressionNode? DeclaringNode { get; set; }
}
