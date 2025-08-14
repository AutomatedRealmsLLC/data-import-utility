namespace DataImportUtility.Models.Validation;

/// <summary>
/// Configuration settings specific to reactive validation mode.
/// These settings only apply when ValidationConfiguration.Mode is set to Reactive.
/// </summary>
public class ReactiveModeSettings
{
    /// <summary>
    /// The delay before triggering validation after changes are detected.
    /// This debouncing prevents excessive validation during rapid changes.
    /// Default is 300 milliseconds, which provides responsive feedback while avoiding performance issues.
    /// </summary>
    public TimeSpan DebounceDelay { get; set; } = TimeSpan.FromMilliseconds(300);

    /// <summary>
    /// Whether to automatically validate when field mapping configurations change.
    /// This includes changes to mapping rules, transformations, and field assignments.
    /// Default is true for optimal user experience in reactive mode.
    /// </summary>
    public bool ValidateOnMappingChange { get; set; } = true;

    /// <summary>
    /// Whether to automatically validate when the underlying data changes.
    /// This includes loading new data files or modifying existing data.
    /// Default is true to ensure validation stays current with data changes.
    /// </summary>
    public bool ValidateOnDataChange { get; set; } = true;

    /// <summary>
    /// Whether to automatically validate when templates are applied or changed.
    /// This includes loading saved templates that modify field mappings.
    /// Default is true to validate template compatibility immediately.
    /// </summary>
    public bool ValidateOnTemplateChange { get; set; } = true;

    /// <summary>
    /// Creates a copy of these reactive mode settings.
    /// </summary>
    /// <returns>A new ReactiveModeSettings instance with identical values.</returns>
    public ReactiveModeSettings Clone()
    {
        return new ReactiveModeSettings
        {
            DebounceDelay = DebounceDelay,
            ValidateOnMappingChange = ValidateOnMappingChange,
            ValidateOnDataChange = ValidateOnDataChange,
            ValidateOnTemplateChange = ValidateOnTemplateChange
        };
    }
}