# GroupLib

A UI- and model-independent .NET library for stable, multi-level grouping.

## Concepts

- GroupRule<TParameter> selects one grouping key and optionally sets its LevelIndex.
- Lower explicit LevelIndex values have higher priority and become outer levels.
- Rules without LevelIndex are placed after every explicitly indexed rule, retaining their declaration order.
- If no rule has LevelIndex, the original declaration order is used.
- GroupEngine.Group(...) builds a tree of GroupNode<TItem, TParameter>.
- Group nodes expose Parameter, Value, Children and leaf Items.

Group and item order are stable: groups appear in the order their keys first occur in the source sequence, and items in each leaf retain their source order. The engine does not sort or mutate the source. Apply sorting before grouping when the desired group and item order should follow a configured sort.

For example, rules declared as `Department (LevelIndex=2)`, `IsActive (no LevelIndex)`, `City (LevelIndex=0)` are resolved as `City`, `Department`, `IsActive`. The unindexed rule receives an effective lower priority than every explicitly indexed rule. When explicit indices are equal, declaration order breaks the tie. Explicit index values do not need to be consecutive. If all rules omit the index, the declaration order is preserved without reordering.

Null keys are supported. A grouping parameter cannot be used more than once in the same rule set.

## Example

~~~csharp
public enum PersonGroupParametr
{
    Department,
    IsActive,
    City
}

GroupRule<PersonGroupParametr>[] rules =
[
    new(PersonGroupParametr.Department),
    new(PersonGroupParametr.IsActive),
    new(PersonGroupParametr.City)
];

IReadOnlyList<GroupNode<Person, PersonGroupParametr>> groups =
    GroupEngine.Group(
        people,
        rules,
        (person, parameter) => parameter switch
        {
            PersonGroupParametr.Department => person.Department,
            PersonGroupParametr.IsActive => person.IsActive,
            PersonGroupParametr.City => person.City,
            _ => null
        });
~~~

For consumers that already have enum parameters, an overload accepts the parameter sequence directly without allocating rule objects.
