using System.Collections.Immutable;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using HotChocolate.Utilities;

namespace HotChocolate.Execution.Processing;

internal partial class MiddlewareContext
{
    private object? _parent;

    public Path Path { get; private set; } = default!;

    public IImmutableDictionary<string, object?> ScopedContextData { get; set; } = default!;

    public IImmutableDictionary<string, object?> LocalContextData { get; set; } = default!;

    public IType? ValueType { get; set; }

    public ObjectResult ParentResult { get; private set; } = default!;

    public bool HasErrors { get; private set; }

    public object? Result
    {
        get => TypedResult?.Value;
        set
        {
            if (ReferenceEquals(TypedResult?.Value, value))
                return;

            IsResultModified = true;
            TypedResult = TypedValue.GuessFromValue(value);
        }
    }

    public TypedValue? TypedResult { get; set; }
    public bool IsResultModified { get; private set; }

    public T Parent<T>()
    {
        if (_parent is T casted)
        {
            return casted;
        }

        if (_parent is null)
        {
            return default!;
        }

        if (_operationContext.Converter.TryConvert(_parent, out casted))
        {
            return casted;
        }

        if (_parent is object[] o &&
            Temp.ValueConverter.TryGetValue(_selection.Id, out var converter))
        {
            return (T)converter(o);
        }


        throw ThrowHelper.ResolverContext_CannotCastParent(
            Selection.Field.Coordinate,
            Path,
            typeof(T),
            _parent.GetType());
    }
}
