using System;
using HotChocolate.Language;
using HotChocolate.Language.Visitors;

namespace HotChocolate.Types.Sorting;

[Obsolete("Use HotChocolate.Data.")]
public class QueryableSortVisitor : SortVisitorBase<QueryableSortVisitorContext>
{
    protected QueryableSortVisitor()
    {
    }

    #region Object Value

    protected override ISyntaxVisitorAction Enter(
        ObjectValueNode node, QueryableSortVisitorContext context) => Continue;

    protected override ISyntaxVisitorAction Leave(
        ObjectValueNode node, QueryableSortVisitorContext context) => Continue;

    #endregion

    #region Object Field

    protected override ISyntaxVisitorAction Enter(
        ObjectFieldNode node,
        QueryableSortVisitorContext context)
    {
        base.Enter(node, context);

        if (context.Operations.Peek() is SortOperationField sortField &&
            sortField.Operation?.Property != null)
        {
            context.Closure.EnqueueProperty(sortField.Operation.Property);
            if (!sortField.Operation.IsObject)
            {
                var parser = context.InputParser;
                var kind = (SortOperationKind)parser.ParseLiteral(node.Value, sortField)!;
                context.SortOperations.Enqueue(context.CreateSortOperation(kind));
            }
        }

        return Continue;
    }

    protected override ISyntaxVisitorAction Leave(
        ObjectFieldNode node,
        QueryableSortVisitorContext context)
    {
        if (context.Operations.Peek() is SortOperationField)
        {
            context.Closure.Pop();
        }
        return base.Leave(node, context);
    }

    #endregion

    #region List

    protected override ISyntaxVisitorAction Enter(
        ListValueNode node,
        QueryableSortVisitorContext context) => Continue;

    #endregion

    public static readonly QueryableSortVisitor Default = new();
}