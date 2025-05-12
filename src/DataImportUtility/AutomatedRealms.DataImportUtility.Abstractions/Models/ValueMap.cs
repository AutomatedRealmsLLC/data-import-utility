namespace AutomatedRealms.DataImportUtility.Abstractions.Models; // Updated

/// <summary>
/// The mapping of values from one field to another.
/// </summary>
public class ValueMap
{
    /// <summary>
    /// The imported field name.
    /// </summary>
    public required string ImportedFieldName { get; set; }

    /// <summary>
    /// The value to map from.
    /// </summary>
    public string? FromValue { get; set; }

    /// <summary>
    /// The value to map to.
    /// </summary>
    public string? ToValue { get; set; }

    /// <summary>
    /// Clones the <see cref="ValueMap" />.
    /// </summary>
    /// <returns>The cloned <see cref="ValueMap" />.</returns>
    public ValueMap Clone()
    {
        return (ValueMap)MemberwiseClone();
    }
}
