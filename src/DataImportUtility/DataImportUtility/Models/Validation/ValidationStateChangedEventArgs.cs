namespace DataImportUtility.Models.Validation;

/// <summary>
/// Event arguments for validation state changes within the data import system.
/// Provides detailed information about what validation states changed and their current status.
/// </summary>
public class ValidationStateChangedEventArgs
{
    /// <summary>
    /// The name of the table where validation state changed.
    /// </summary>
    public required string TableName { get; init; }

    /// <summary>
    /// The names of the fields that experienced validation changes.
    /// This allows consumers to respond selectively to specific field changes.
    /// </summary>
    public IEnumerable<string> FieldNames { get; init; } = [];

    /// <summary>
    /// Indicates whether the overall validation state for this table has errors.
    /// This is a convenience property that aggregates error status across all changed fields.
    /// </summary>
    public bool HasErrors { get; init; }

    /// <summary>
    /// The complete validation states that changed, keyed by field name.
    /// Provides full access to validation details for each affected field.
    /// </summary>
    public Dictionary<string, FieldValidationState> ChangedValidationStates { get; init; } = [];

    /// <summary>
    /// The timestamp when these validation changes occurred.
    /// Useful for debugging and understanding the sequence of validation events.
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Gets a summary of all validation errors across the changed fields.
    /// </summary>
    /// <returns>A collection of all validation error messages from the changed states.</returns>
    public IEnumerable<string> GetAllErrorMessages()
    {
        return ChangedValidationStates.Values
            .SelectMany(state => state.GetErrorSummary())
            .Distinct();
    }

    /// <summary>
    /// Gets the count of fields that have validation errors.
    /// </summary>
    /// <returns>The number of fields with validation errors in this change event.</returns>
    public int ErrorFieldCount => ChangedValidationStates.Values.Count(state => state.HasErrors);

    /// <summary>
    /// Gets the count of fields that are error-free.
    /// </summary>
    /// <returns>The number of fields without validation errors in this change event.</returns>
    public int ValidFieldCount => ChangedValidationStates.Values.Count(state => !state.HasErrors);
}