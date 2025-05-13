using System.Text.Json.Serialization;

namespace AutomatedRealms.DataImportUtility.Abstractions.Models;

/// <summary>
/// Represents a condition that must be met for a parent rule or transformation to be applied.
/// </summary>
public class ConditionalRule : IDisposable, IEquatable<ConditionalRule>
{
    /// <summary>
    /// The field to check the condition against.
    /// </summary>
    public ImportedRecordFieldDescriptor? SourceField { get; set; }

    /// <summary>
    /// Gets or sets the TypeId of the comparison operation to perform.
    /// Used for serialization.
    /// </summary>
    public string? OperationTypeId { get; set; }

    /// <summary>
    /// Gets or sets the actual comparison operation instance.
    /// This is resolved from OperationTypeId by the TypeRegistryService and is ignored during serialization.
    /// </summary>
    [JsonIgnore]
    public ComparisonOperationBase? Operation { get; set; }

    /// <summary>
    /// The value to compare against. The interpretation of this value depends on the <see cref="Operation"/>.
    /// For operations like IsNullOrEmpty, this might not be used.
    /// For 'IsBetween', this could be the lower bound, and <see cref="SecondaryComparisonValue"/> the upper bound.
    /// </summary>
    public string? ComparisonValue { get; set; }

    /// <summary>
    /// An optional secondary value for comparison, used by operations like 'IsBetween'.
    /// </summary>
    public string? SecondaryComparisonValue { get; set; }

    /// <summary>
    /// Event that is raised when the definition of this conditional rule changes.
    /// </summary>
    public event Action? OnDefinitionChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConditionalRule"/> class.
    /// </summary>
    public ConditionalRule() { }

    /// <summary>
    /// Triggers the <see cref="OnDefinitionChanged"/> event.
    /// </summary>
    protected virtual void InvokeDefinitionChanged()
    {
        OnDefinitionChanged?.Invoke();
    }

    /// <summary>
    /// Creates a new object that is a copy of the current instance.
    /// </summary>
    /// <returns>A new object that is a copy of this instance.</returns>
    public virtual ConditionalRule Clone()
    {
        return new ConditionalRule
        {
            SourceField = this.SourceField?.Clone(),
            OperationTypeId = this.OperationTypeId,
            Operation = this.Operation?.Clone(),
            ComparisonValue = this.ComparisonValue,
            SecondaryComparisonValue = this.SecondaryComparisonValue
        };
        // Note: Event subscriptions are not cloned.
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Clear event subscriptions
            if (OnDefinitionChanged != null)
            {
                foreach (var d in OnDefinitionChanged.GetInvocationList())
                {
                    OnDefinitionChanged -= (Action)d;
                }
            }
        }
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return Equals(obj as ConditionalRule);
    }

    /// <inheritdoc />
    public bool Equals(ConditionalRule? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return EqualityComparer<ImportedRecordFieldDescriptor?>.Default.Equals(SourceField, other.SourceField) &&
               OperationTypeId == other.OperationTypeId &&
               ComparisonValue == other.ComparisonValue &&
               SecondaryComparisonValue == other.SecondaryComparisonValue;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        // Manual hash code calculation for netstandard2.0
        int hashCode = 17; // Start with a prime number

        // Suitable nullity checks
        hashCode = hashCode * 23 + (SourceField?.GetHashCode() ?? 0);
        hashCode = hashCode * 23 + (OperationTypeId?.GetHashCode() ?? 0);
        hashCode = hashCode * 23 + (ComparisonValue?.GetHashCode() ?? 0);
        hashCode = hashCode * 23 + (SecondaryComparisonValue?.GetHashCode() ?? 0);

        return hashCode;
    }
}
