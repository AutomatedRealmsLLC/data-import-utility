namespace DataImportUtility.Models.Validation;

/// <summary>
/// Configuration options that control validation behavior for an imported data file.
/// These settings determine when and how validation is performed during the data import process.
/// </summary>
public class ValidationConfiguration
{
    /// <summary>
    /// The validation mode that determines when validation is triggered.
    /// Defaults to OnDemand for optimal performance and explicit control.
    /// </summary>
    public ValidationMode Mode { get; set; } = ValidationMode.OnDemand;

    /// <summary>
    /// Configuration settings specific to reactive validation mode.
    /// These settings only apply when Mode is set to Reactive.
    /// Provides clear separation between universal and mode-specific settings.
    /// </summary>
    public ReactiveModeSettings ReactiveSettings { get; set; } = new();

    /// <summary>
    /// Creates a copy of this validation configuration.
    /// </summary>
    /// <returns>A new ValidationConfiguration instance with identical settings.</returns>
    public ValidationConfiguration Clone()
    {
        return new ValidationConfiguration
        {
            Mode = Mode,
            ReactiveSettings = ReactiveSettings.Clone()
        };
    }

    /// <summary>
    /// Convenience property for accessing the debounce delay when in reactive mode.
    /// Returns TimeSpan.Zero when not in reactive mode.
    /// </summary>
    public TimeSpan EffectiveDebounceDelay => Mode == ValidationMode.Reactive ? ReactiveSettings.DebounceDelay : TimeSpan.Zero;

    /// <summary>
    /// Checks if validation should be triggered for mapping changes based on current configuration.
    /// </summary>
    /// <returns>True if mapping changes should trigger validation.</returns>
    public bool ShouldValidateOnMappingChange => Mode == ValidationMode.Reactive && ReactiveSettings.ValidateOnMappingChange;

    /// <summary>
    /// Checks if validation should be triggered for data changes based on current configuration.
    /// </summary>
    /// <returns>True if data changes should trigger validation.</returns>
    public bool ShouldValidateOnDataChange => Mode == ValidationMode.Reactive && ReactiveSettings.ValidateOnDataChange;

    /// <summary>
    /// Checks if validation should be triggered for template changes based on current configuration.
    /// </summary>
    /// <returns>True if template changes should trigger validation.</returns>
    public bool ShouldValidateOnTemplateChange => Mode == ValidationMode.Reactive && ReactiveSettings.ValidateOnTemplateChange;
}