using System.ComponentModel.DataAnnotations;

namespace DataImportUtility.Models.Validation;

/// <summary>
/// Represents the validation state for a specific field mapping within a data import table.
/// This class tracks validation results, timing, and state management for individual fields.
/// </summary>
public class FieldValidationState
{
    /// <summary>
    /// The name of the field this validation state represents.
    /// </summary>
    public required string FieldName { get; init; }

    /// <summary>
    /// A unique identifier for this validation state version.
    /// Changes each time the validation state is updated to enable change detection.
    /// </summary>
    public Guid ValidationVersion { get; private set; } = Guid.NewGuid();

    /// <summary>
    /// Indicates whether this validation state is stale and needs to be refreshed.
    /// Set to true when field mappings or data change, false after successful validation.
    /// </summary>
    public bool IsStale { get; set; }

    /// <summary>
    /// The timestamp when this field was last successfully validated.
    /// Used for debugging and performance monitoring.
    /// </summary>
    public DateTime LastValidated { get; private set; } = DateTime.MinValue;

    /// <summary>
    /// Indicates whether this field currently has any validation errors.
    /// This is a summary flag derived from the CachedResults.
    /// </summary>
    public bool HasErrors { get; set; }

    /// <summary>
    /// Cached validation results for distinct values encountered during transformation.
    /// The key is the transformed value, the value is the list of validation errors (or null if valid).
    /// This caching improves performance by avoiding repeated validation of the same values.
    /// </summary>
    public Dictionary<string, List<ValidationResult>?> CachedResults { get; } = [];

    /// <summary>
    /// Marks this validation state as successfully updated with fresh validation results.
    /// Updates the validation version, timestamp, and clears the stale flag.
    /// </summary>
    public void MarkAsValidated()
    {
        LastValidated = DateTime.UtcNow;
        ValidationVersion = Guid.NewGuid();
        IsStale = false;
    }

    /// <summary>
    /// Marks this validation state as stale, indicating it needs to be refreshed.
    /// This is typically called when field mappings or source data changes.
    /// </summary>
    public void MarkAsStale()
    {
        IsStale = true;
    }

    /// <summary>
    /// Clears all cached validation results.
    /// Useful when field mappings change significantly and cached results are no longer valid.
    /// </summary>
    public void ClearCache()
    {
        CachedResults.Clear();
        MarkAsStale();
    }

    /// <summary>
    /// Gets a summary of validation errors for display purposes.
    /// </summary>
    /// <returns>A collection of all validation error messages from the cached results.</returns>
    public IEnumerable<string> GetErrorSummary()
    {
        return CachedResults.Values
            .Where(results => results is not null)
            .SelectMany(results => results!)
            .Where(result => !string.IsNullOrWhiteSpace(result.ErrorMessage))
            .Select(result => result.ErrorMessage!)
            .Distinct();
    }
}