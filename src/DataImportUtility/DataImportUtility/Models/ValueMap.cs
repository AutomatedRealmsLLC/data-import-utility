namespace DataImportUtility.Models;

/// <summary>
/// The mapping of values from one field to another.
/// </summary>
public class ValueMap
{
    /// <summary>
    /// The imported field name.
    /// </summary>
    public /*required*/ string ImportedFieldName { get; set; } = string.Empty;

    /// <summary>
    /// The value to map from.
    /// </summary>
    public string? FromValue { get; set; }

    /// <summary>
    /// The value to map to.
    /// </summary>
    public string? ToValue { get; set; }
}
