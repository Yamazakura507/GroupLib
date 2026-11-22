using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GroupLib.Models;

/// <summary>
/// Describes one level in a hierarchical grouping operation.
/// Explicit LevelIndex values determine priority; unindexed rules are placed after indexed rules.
/// </summary>
/// <typeparam name="TParameter">Enum identifying the property used as the grouping key.</typeparam>
public class GroupRule<TParameter> : INotifyPropertyChanged where TParameter : struct, Enum
{
    private TParameter groupParametr;
    private int? levelIndex;

    /// <summary>
    /// Gets or sets the enum value that identifies the property used as this grouping level.
    /// </summary>
    public TParameter GroupParametr
    {
        get => groupParametr;
        set => SetField(ref groupParametr, value);
    }

    /// <summary>
    /// Gets or sets the explicit priority of this grouping level. Lower values are processed
    /// first (outermost). A null value places this rule after all explicitly indexed rules,
    /// preserving its relative order among other rules without an index.
    /// </summary>
    public int? LevelIndex
    {
        get => levelIndex;
        set => SetField(ref levelIndex, value);
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Initializes a rule with its default enum value.
    /// </summary>
    public GroupRule() { }

    /// <summary>
    /// Initializes a rule for the supplied grouping key and optional priority.
    /// </summary>
    /// <param name="groupParametr">The enum value representing this grouping key.</param>
    /// <param name="levelIndex">Optional priority. Lower values are processed first; null is automatic lower priority.</param>
    public GroupRule(TParameter groupParametr, int? levelIndex = null)
    {
        this.groupParametr = groupParametr;
        this.levelIndex = levelIndex;
    }

    private bool SetField<TValue>(ref TValue field, TValue value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<TValue>.Default.Equals(field, value)) return false;

        field = value;

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        return true;
    }
}
