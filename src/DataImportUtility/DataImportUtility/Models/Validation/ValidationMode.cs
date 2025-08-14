using System.ComponentModel;

namespace DataImportUtility.Models.Validation;

/// <summary>
/// Defines how validation should be performed within the data import system.
/// </summary>
public enum ValidationMode
{
    /// <summary>
    /// No automatic validation is performed. Validation must be explicitly requested.
    /// </summary>
    [Description("Validation is disabled and must be manually triggered")]
    Disabled = 0,

    /// <summary>
    /// Validation is performed only when explicitly requested through API calls.
    /// This is the default mode for maximum performance and control.
    /// </summary>
    [Description("Validation occurs only when explicitly requested")]
    OnDemand = 1,

    /// <summary>
    /// Validation is performed automatically when dependencies change, such as
    /// field mappings, data, or templates. Uses debouncing to optimize performance.
    /// </summary>
    [Description("Validation occurs automatically when dependencies change")]
    Reactive = 2
}