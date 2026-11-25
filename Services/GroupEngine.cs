using GroupLib.Models;

namespace GroupLib.Services;

/// <summary>
/// Creates stable, hierarchical group trees without modifying the source sequence.
/// </summary>
public static class GroupEngine
{
    /// <summary>
    /// Groups a sequence using one or more grouping rules.
    /// Explicit LevelIndex values are ordered ascending; unindexed rules follow in declaration order.
    /// Group keys retain their first-occurrence order in the source sequence, and item order
    /// inside each leaf is preserved. Null grouping keys are supported.
    /// </summary>
    /// <typeparam name="TItem">The item type.</typeparam>
    /// <typeparam name="TParameter">Enum identifying groupable item properties.</typeparam>
    /// <param name="source">Items to group, normally already sorted by the consumer.</param>
    /// <param name="rules">Grouping rules. Explicit indices are processed ascending; unindexed rules follow in declaration order.</param>
    /// <param name="keySelector">Returns the value represented by a rule for an item.</param>
    /// <returns>Root nodes for the first grouping level, or an empty result when no rules are supplied.</returns>
    /// <exception cref="ArgumentNullException">The source or key selector is null.</exception>
    /// <exception cref="ArgumentException">A grouping parameter is repeated.</exception>
    public static IReadOnlyList<GroupNode<TItem, TParameter>> Group<TItem, TParameter>(
        this IEnumerable<TItem> source,
        IEnumerable<GroupRule<TParameter>>? rules,
        Func<TItem, TParameter, object?> keySelector) where TParameter : struct, Enum
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keySelector);

        GroupRule<TParameter>[] configuredRules = rules?.ToArray() ?? [];

        if (configuredRules.Length == 0) return Array.Empty<GroupNode<TItem, TParameter>>();

        HashSet<TParameter> usedParameters = [];

        foreach (GroupRule<TParameter> rule in configuredRules)
        {
            ArgumentNullException.ThrowIfNull(rule);

            if (!usedParameters.Add(rule.GroupParametr)) throw new ArgumentException($"Grouping parameter '{rule.GroupParametr}' cannot be repeated.", nameof(rules));
        }

        GroupRule<TParameter>[] orderedRules = configuredRules
            .Select((rule, index) => new { Rule = rule, Index = index })
            .OrderBy(entry => entry.Rule.LevelIndex.HasValue ? 0 : 1)
            .ThenBy(entry => entry.Rule.LevelIndex)
            .ThenBy(entry => entry.Index)
            .Select(entry => entry.Rule)
            .ToArray();

        return source.ToList().Build(orderedRules, keySelector, 0);
    }

    /// <summary>
    /// Groups a sequence using enum parameters without creating rule objects.
    /// </summary>
    public static IReadOnlyList<GroupNode<TItem, TParameter>> Group<TItem, TParameter>(
        this IEnumerable<TItem> source,
        IEnumerable<TParameter> parameters,
        Func<TItem, TParameter, object?> keySelector)
        where TParameter : struct, Enum
    {
        ArgumentNullException.ThrowIfNull(parameters);

        return source.Group(parameters.Select(parameter => new GroupRule<TParameter>(parameter)), keySelector);
    }

    private static IReadOnlyList<GroupNode<TItem, TParameter>> Build<TItem, TParameter>(
        this IReadOnlyList<TItem> items,
        IReadOnlyList<GroupRule<TParameter>> rules,
        Func<TItem, TParameter, object?> keySelector,
        int level) where TParameter : struct, Enum
    {
        TParameter parameter = rules[level].GroupParametr;
        List<GroupBucket<TItem>> buckets = [];
        Dictionary<object, GroupBucket<TItem>> bucketsByKey = [];
        GroupBucket<TItem>? nullBucket = null;

        foreach (TItem item in items)
        {
            object? key = keySelector(item, parameter);
            GroupBucket<TItem>? bucket;

            if (key is null)
            {
                bucket = nullBucket;

                if (bucket is null)
                {
                    bucket = new GroupBucket<TItem>(null);
                    nullBucket = bucket;

                    buckets.Add(bucket);
                }
            }
            else if (!bucketsByKey.TryGetValue(key, out bucket))
            {
                bucket = new GroupBucket<TItem>(key);

                bucketsByKey.Add(key, bucket);
                buckets.Add(bucket);
            }

            bucket.Items.Add(item);
        }

        return buckets.Select(bucket => level == rules.Count - 1
                ? new GroupNode<TItem, TParameter>(parameter, bucket.Key, items: bucket.Items)
                : new GroupNode<TItem, TParameter>(parameter, bucket.Key, children: bucket.Items.Build(rules, keySelector, level + 1)))
            .ToList();
    }
}
