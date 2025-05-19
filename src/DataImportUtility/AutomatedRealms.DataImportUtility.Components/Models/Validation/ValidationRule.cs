namespace AutomatedRealms.DataImportUtility.Components.Models.Validation;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Base class for validation rules that can be applied to model properties.
/// This class uses the "smart enum" pattern to allow for extensibility.
/// </summary>
public abstract class ValidationRule : IEquatable<ValidationRule>
{
    #region Properties

    /// <summary>
    /// Gets the unique identifier name for this validation rule.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Gets the display name for this validation rule.
    /// </summary>
    public abstract string DisplayName { get; }

    /// <summary>
    /// Gets the default severity for validation failures using this rule.
    /// </summary>
    public abstract ValidationSeverity DefaultSeverity { get; }

    #endregion

    #region Methods

    /// <summary>
    /// Validates a value against this rule.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="errorMessage">The error message if validation fails.</param>
    /// <returns>True if validation passes, false otherwise.</returns>
    public abstract bool Validate(object? value, out string? errorMessage);

    /// <summary>
    /// Determines whether this rule can be applied to the specified property.
    /// </summary>
    /// <param name="modelType">The model type containing the property.</param>
    /// <param name="propertyName">The name of the property.</param>
    /// <returns>True if this rule can be applied to the property, false otherwise.</returns>
    public virtual bool IsApplicableToProperty(Type modelType, string propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
        {
            throw new ArgumentException("Property name cannot be null or empty.", nameof(propertyName));
        }

        var propertyInfo = modelType.GetProperty(propertyName);
        if (propertyInfo == null)
        {
            return false;
        }

        return IsApplicableToPropertyType(propertyInfo.PropertyType);
    }

    /// <summary>
    /// Determines whether this rule can be applied to the specified property type.
    /// </summary>
    /// <param name="propertyType">The type of the property.</param>
    /// <returns>True if this rule can be applied to the property type, false otherwise.</returns>
    protected abstract bool IsApplicableToPropertyType(Type propertyType);

    #endregion

    #region Registry

    private static readonly List<ValidationRule> _knownRules = [];

    /// <summary>
    /// Gets all registered validation rules.
    /// </summary>
    public static IReadOnlyList<ValidationRule> KnownRules => _knownRules.AsReadOnly();

    /// <summary>
    /// Registers a new validation rule.
    /// </summary>
    /// <param name="rule">The rule to register.</param>
    public static void Register(ValidationRule rule)
    {
        if (!_knownRules.Contains(rule))
        {
            _knownRules.Add(rule);
        }
    }

    /// <summary>
    /// Gets a validation rule by its name.
    /// </summary>
    /// <param name="name">The name of the rule.</param>
    /// <returns>The rule with the specified name, or null if not found.</returns>
    public static ValidationRule? GetByName(string name)
    {
        return KnownRules.FirstOrDefault(r =>
            r.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets all validation rules applicable to the specified property.
    /// </summary>
    /// <param name="modelType">The model type containing the property.</param>
    /// <param name="propertyName">The name of the property.</param>
    /// <returns>A collection of applicable validation rules.</returns>
    public static IEnumerable<ValidationRule> GetForProperty(Type modelType, string propertyName)
    {
        return KnownRules.Where(r => r.IsApplicableToProperty(modelType, propertyName));
    }

    #endregion

    #region Equality

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return Equals(obj as ValidationRule);
    }

    /// <inheritdoc/>
    public bool Equals(ValidationRule? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return Name.GetHashCode(StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Equality operator.
    /// </summary>
    public static bool operator ==(ValidationRule? left, ValidationRule? right)
    {
        if (left is null)
        {
            return right is null;
        }

        return left.Equals(right);
    }

    /// <summary>
    /// Inequality operator.
    /// </summary>
    public static bool operator !=(ValidationRule? left, ValidationRule? right)
    {
        return !(left == right);
    }

    /// <inheritdoc/>
    public override string ToString() => DisplayName;

    #endregion
}