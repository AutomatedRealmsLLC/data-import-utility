namespace AutomatedRealms.DataImportUtility.Components.Models.Validation;

using System;
using System.Globalization;

/// <summary>
/// Validation rule that requires a numeric value to be within a specified range.
/// </summary>
public class RangeValidationRule : ValidationRule
{
    private readonly double _min;
    private readonly double _max;

    /// <summary>
    /// Initializes a new instance of the <see cref="RangeValidationRule"/> class.
    /// </summary>
    /// <param name="min">The minimum allowed value.</param>
    /// <param name="max">The maximum allowed value.</param>
    public RangeValidationRule(double min, double max)
    {
        if (min > max)
        {
            throw new ArgumentException($"Minimum value ({min}) cannot be greater than maximum value ({max}).");
        }

        _min = min;
        _max = max;
    }

    /// <inheritdoc/>
    public override string Name => "range";

    /// <inheritdoc/>
    public override string DisplayName => $"Range ({_min} - {_max})";

    /// <inheritdoc/>
    public override ValidationSeverity DefaultSeverity => ValidationSeverity.Error;

    /// <inheritdoc/>
    public override bool Validate(object? value, out string? errorMessage)
    {
        if (value == null)
        {
            errorMessage = null;
            return true; // Null values are handled by the RequiredValidationRule
        }

        if (TryGetDoubleValue(value, out double doubleValue))
        {
            if (doubleValue < _min || doubleValue > _max)
            {
                errorMessage = $"Value must be between {_min} and {_max}.";
                return false;
            }

            errorMessage = null;
            return true;
        }

        errorMessage = $"Value '{value}' is not a valid number.";
        return false;
    }

    private static bool TryGetDoubleValue(object value, out double result)
    {
        if (value is double d)
        {
            result = d;
            return true;
        }

        if (value is int i)
        {
            result = i;
            return true;
        }

        if (value is long l)
        {
            result = l;
            return true;
        }

        if (value is float f)
        {
            result = f;
            return true;
        }

        if (value is decimal m)
        {
            result = (double)m;
            return true;
        }

        if (value is string s && double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out double parsed))
        {
            result = parsed;
            return true;
        }

        result = 0;
        return false;
    }

    /// <inheritdoc/>
    protected override bool IsApplicableToPropertyType(Type propertyType)
    {
        return propertyType == typeof(int) ||
               propertyType == typeof(int?) ||
               propertyType == typeof(long) ||
               propertyType == typeof(long?) ||
               propertyType == typeof(float) ||
               propertyType == typeof(float?) ||
               propertyType == typeof(double) ||
               propertyType == typeof(double?) ||
               propertyType == typeof(decimal) ||
               propertyType == typeof(decimal?);
    }
}