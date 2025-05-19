namespace AutomatedRealms.DataImportUtility.Components.Models.Validation;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Represents when validation should be triggered.
/// This class uses the "smart enum" pattern to allow for extensibility beyond the built-in triggers.
/// </summary>
public abstract class ValidationTrigger : IEquatable<ValidationTrigger>
{
    #region Built-in Types

    /// <summary>
    /// Validation occurs when the field is changed.
    /// </summary>
    public static readonly ValidationTrigger OnChange = new OnChangeValidationTrigger();

    /// <summary>
    /// Validation occurs when the field loses focus.
    /// </summary>
    public static readonly ValidationTrigger OnBlur = new OnBlurValidationTrigger();

    /// <summary>
    /// Validation occurs when the form is submitted.
    /// </summary>
    public static readonly ValidationTrigger OnSubmit = new OnSubmitValidationTrigger();

    /// <summary>
    /// Validation occurs manually when explicitly requested.
    /// </summary>
    public static readonly ValidationTrigger Manual = new ManualValidationTrigger();

    #endregion

    #region Properties

    /// <summary>
    /// Gets the unique identifier name for this trigger.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Gets the display name for this trigger.
    /// </summary>
    public abstract string DisplayName { get; }

    #endregion

    #region Registry

    private static readonly List<ValidationTrigger> _knownTriggers = [OnChange, OnBlur, OnSubmit, Manual];

    /// <summary>
    /// Gets all registered validation triggers.
    /// </summary>
    public static IReadOnlyList<ValidationTrigger> KnownTriggers => _knownTriggers.AsReadOnly();

    /// <summary>
    /// Registers a new validation trigger.
    /// </summary>
    /// <param name="trigger">The trigger to register.</param>
    public static void Register(ValidationTrigger trigger)
    {
        if (!_knownTriggers.Contains(trigger))
        {
            _knownTriggers.Add(trigger);
        }
    }

    /// <summary>
    /// Gets a validation trigger by its name.
    /// </summary>
    /// <param name="name">The name of the trigger.</param>
    /// <returns>The trigger with the specified name, or null if not found.</returns>
    public static ValidationTrigger? GetByName(string name)
    {
        return KnownTriggers.FirstOrDefault(t =>
            t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    #endregion

    #region Equality

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return Equals(obj as ValidationTrigger);
    }

    /// <inheritdoc/>
    public bool Equals(ValidationTrigger? other)
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
    public static bool operator ==(ValidationTrigger? left, ValidationTrigger? right)
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
    public static bool operator !=(ValidationTrigger? left, ValidationTrigger? right)
    {
        return !(left == right);
    }

    /// <inheritdoc/>
    public override string ToString() => DisplayName;

    #endregion
}

// Concrete implementations of ValidationTrigger
internal class OnChangeValidationTrigger : ValidationTrigger
{
    public override string Name => "onchange";
    public override string DisplayName => "On Change";
}

internal class OnBlurValidationTrigger : ValidationTrigger
{
    public override string Name => "onblur";
    public override string DisplayName => "On Blur";
}

internal class OnSubmitValidationTrigger : ValidationTrigger
{
    public override string Name => "onsubmit";
    public override string DisplayName => "On Submit";
}

internal class ManualValidationTrigger : ValidationTrigger
{
    public override string Name => "manual";
    public override string DisplayName => "Manual";
}