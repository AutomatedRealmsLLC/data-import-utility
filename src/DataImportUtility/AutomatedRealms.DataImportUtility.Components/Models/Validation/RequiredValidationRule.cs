namespace AutomatedRealms.DataImportUtility.Components.Models.Validation;

using System;

/// <summary>
/// Validation rule that requires a value to be non-null and non-empty.
/// </summary>
public class RequiredValidationRule : ValidationRule
{
    /// <inheritdoc/>
    public override string Name => "required";

    /// <inheritdoc/>
    public override string DisplayName => "Required";

    /// <inheritdoc/>
    public override ValidationSeverity DefaultSeverity => ValidationSeverity.Error;

    /// <inheritdoc/>
    public override bool Validate(object? value, out string? errorMessage)
    {
        if (value == null)
        {
            errorMessage = "This field is required.";
            return false;
        }

        if (value is string stringValue && string.IsNullOrWhiteSpace(stringValue))
        {
            errorMessage = "This field is required.";
            return false;
        }

        errorMessage = null;
        return true;
    }

    /// <inheritdoc/>
    protected override bool IsApplicableToPropertyType(Type propertyType)
    {
        // Can be applied to any property type
        return true;
    }
}