namespace AutomatedRealms.DataImportUtility.Abstractions.Enums;

/// <summary>
/// Defines the types of mapping rules available.
/// </summary>
public enum RuleType
{
    /// <summary>
    /// Indicates no rule is specified or the rule is unknown.
    /// </summary>
    None,

    /// <summary>
    /// Rule to copy a value from a source field.
    /// </summary>
    CopyRule,

    /// <summary>
    /// Rule to use a constant value.
    /// </summary>
    ConstantValueRule,

    /// <summary>
    /// Rule to combine multiple source fields.
    /// </summary>
    CombineFieldsRule,

    /// <summary>
    /// Rule to ignore a field, not mapping it.
    /// </summary>
    IgnoreRule,

    /// <summary>
    /// Rule that is custom and does not map to a specific source field directly.
    /// </summary>
    CustomFieldlessRule,

    /// <summary>
    /// Rule to represent a static, pre-defined value, typically for internal use (e.g., operands).
    /// </summary>
    StaticValue
    // Add other rule types as they are defined
}
