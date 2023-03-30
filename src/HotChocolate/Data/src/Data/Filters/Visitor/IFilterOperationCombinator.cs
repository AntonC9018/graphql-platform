using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace HotChocolate.Data.Filters;

public interface IFilterOperationCombinator<in TContext, TExpression> : IFilterOperationCombinator
    where TContext : FilterVisitorContext<TExpression>
{
    bool TryCombineOperations(
        TContext context,
        Queue<TExpression> operations,
        FilterCombinator combinator,
        [NotNullWhen(true)] out TExpression? combined);
}
