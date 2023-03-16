using HotChocolate.Execution.Processing;
using HotChocolate.Fusion.Metadata;

namespace HotChocolate.Fusion.Planning;

/// <summary>
/// Represents a execution step for a node field within
/// the execution plan while being in the planing phase.
/// </summary>
internal sealed class NodeExecutionStep : ExecutionStep
{
    /// <summary>
    /// Initializes a new instance of <see cref="NodeExecutionStep"/>.
    /// </summary>
    /// <param name="nodeSelection">
    /// The node selection.
    /// </param>
    /// <param name="queryType">
    /// The query type.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="nodeSelection"/> is <c>null</c>.
    /// </exception>
    public NodeExecutionStep(
        ISelection nodeSelection,
        ObjectType queryType)
        : base(queryType, null)
    {
        NodeSelection = nodeSelection ??
            throw new ArgumentNullException(nameof(nodeSelection));
    }

    /// <summary>
    /// Gets the nodes selection.
    /// </summary>
    public ISelection NodeSelection { get; }
}
