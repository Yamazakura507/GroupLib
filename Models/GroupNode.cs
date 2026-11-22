namespace GroupLib.Models;

/// <summary>
/// One immutable node in a hierarchical grouping result.
/// A node contains either child groups or the items belonging to a leaf group.
/// </summary>
/// <typeparam name="TItem">The grouped item type.</typeparam>
/// <typeparam name="TParameter">Enum identifying the property used for this grouping level.</typeparam>
public sealed class GroupNode<TItem, TParameter> where TParameter : struct, Enum
{
    /// <summary>
    /// Gets the grouping property represented by this node.
    /// </summary>
    public TParameter Parameter { get; }

    /// <summary>
    /// Gets the key shared by items in this group. Null keys are supported.
    /// </summary>
    public object? Value { get; }

    /// <summary>
    /// Gets the child groups for the next grouping level.
    /// </summary>
    public IReadOnlyList<GroupNode<TItem, TParameter>> Children { get; }

    /// <summary>
    /// Gets the items in this leaf group. Intermediate groups return an empty collection.
    /// </summary>
    public IReadOnlyList<TItem> Items { get; }

    internal GroupNode(
        TParameter parameter,
        object? value,
        IReadOnlyList<GroupNode<TItem, TParameter>>? children = null,
        IReadOnlyList<TItem>? items = null)
    {
        Parameter = parameter;
        Value = value;
        Children = children ?? Array.Empty<GroupNode<TItem, TParameter>>();
        Items = items ?? Array.Empty<TItem>();
    }
}
