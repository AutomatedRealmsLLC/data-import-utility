namespace AutomatedRealms.DataImportUtility.Components.Models.Validation;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Represents a validation message severity level.
/// This class uses the "smart enum" pattern to allow for extensibility beyond the built-in severity levels.
/// </summary>
public abstract class ValidationSeverity : IEquatable<ValidationSeverity>, IComparable<ValidationSeverity>
{
    #region Built-in Types

    /// <summary>
    /// Information-level message - least severe.
    /// </summary>
    public static readonly ValidationSeverity Info = new InfoValidationSeverity();

    /// <summary>
    /// Warning-level message - moderately severe.
    /// </summary>
    public static readonly ValidationSeverity Warning = new WarningValidationSeverity();

    /// <summary>
    /// Error-level message - severe, prevents submission.
    /// </summary>
    public static readonly ValidationSeverity Error = new ErrorValidationSeverity();

    /// <summary>
    /// Critical-level message - most severe, requires immediate attention.
    /// </summary>
    public static readonly ValidationSeverity Critical = new CriticalValidationSeverity();

    #endregion

    #region Properties

    /// <summary>
    /// Gets the unique identifier name for this severity level.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Gets the display name for this severity level.
    /// </summary>
    public abstract string DisplayName { get; }

    /// <summary>
    /// Gets the severity level value (higher values are more severe).
    /// </summary>
    public abstract int Level { get; }

    /// <summary>
    /// Gets the CSS class to apply for this severity level.
    /// </summary>
    public abstract string CssClass { get; }

    #endregion

    #region Registry

    private static readonly List<ValidationSeverity> _knownSeverities = [Info, Warning, Error, Critical];

    /// <summary>
    /// Gets all registered severity levels.
    /// </summary>
    public static IReadOnlyList<ValidationSeverity> KnownSeverities => _knownSeverities.AsReadOnly();

    /// <summary>
    /// Registers a new severity level.
    /// </summary>
    /// <param name="severity">The severity level to register.</param>
    public static void Register(ValidationSeverity severity)
    {
        if (!_knownSeverities.Contains(severity))
        {
            _knownSeverities.Add(severity);
        }
    }

    /// <summary>
    /// Gets a severity level by its name.
    /// </summary>
    /// <param name="name">The name of the severity level.</param>
    /// <returns>The severity level with the specified name, or null if not found.</returns>
    public static ValidationSeverity? GetByName(string name)
    {
        return KnownSeverities.FirstOrDefault(s =>
            s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    #endregion

    #region Equality and Comparison

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return Equals(obj as ValidationSeverity);
    }

    /// <inheritdoc/>
    public bool Equals(ValidationSeverity? other)
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
    public static bool operator ==(ValidationSeverity? left, ValidationSeverity? right)
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
    public static bool operator !=(ValidationSeverity? left, ValidationSeverity? right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Less than operator.
    /// </summary>
    public static bool operator <(ValidationSeverity? left, ValidationSeverity? right)
    {
        if (left is null)
        {
            return right is not null;
        }

        return left.CompareTo(right) < 0;
    }

    /// <summary>
    /// Greater than operator.
    /// </summary>
    public static bool operator >(ValidationSeverity? left, ValidationSeverity? right)
    {
        if (left is null)
        {
            return false;
        }

        return left.CompareTo(right) > 0;
    }

    /// <summary>
    /// Less than or equal operator.
    /// </summary>
    public static bool operator <=(ValidationSeverity? left, ValidationSeverity? right)
    {
        if (left is null)
        {
            return true;
        }

        return left.CompareTo(right) <= 0;
    }

    /// <summary>
    /// Greater than or equal operator.
    /// </summary>
    public static bool operator >=(ValidationSeverity? left, ValidationSeverity? right)
    {
        if (left is null)
        {
            return right is null;
        }

        return left.CompareTo(right) >= 0;
    }

    /// <inheritdoc/>
    public int CompareTo(ValidationSeverity? other)
    {
        if (other is null)
        {
            return 1; // Any non-null value is greater than null
        }

        return Level.CompareTo(other.Level);
    }

    /// <inheritdoc/>
    public override string ToString() => DisplayName;

    #endregion
}

// Concrete implementations of ValidationSeverity
internal class InfoValidationSeverity : ValidationSeverity
{
    public override string Name => "info";
    public override string DisplayName => "Information";
    public override int Level => 0;
    public override string CssClass => "validation-info";
}

internal class WarningValidationSeverity : ValidationSeverity
{
    public override string Name => "warning";
    public override string DisplayName => "Warning";
    public override int Level => 1;
    public override string CssClass => "validation-warning";
}

internal class ErrorValidationSeverity : ValidationSeverity
{
    public override string Name => "error";
    public override string DisplayName => "Error";
    public override int Level => 2;
    public override string CssClass => "validation-error";
}

internal class CriticalValidationSeverity : ValidationSeverity
{
    public override string Name => "critical";
    public override string DisplayName => "Critical";
    public override int Level => 3;
    public override string CssClass => "validation-critical";
}