using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AutomatedRealms.DataImportUtility.Abstractions.Models;

/// <summary>
/// Defines a field and the sequence of value transformations to be applied to it.
/// This class is intended for definition and serialization; execution logic resides elsewhere.
/// </summary>
public class FieldTransformation : IDisposable, IEquatable<FieldTransformation>
{
    private ImmutableList<ValueTransformationBase> _valueTransformations = ImmutableList<ValueTransformationBase>.Empty;

    /// <summary>
    /// The descriptor of the field whose value is to be transformed.
    /// </summary>
    public virtual ImportedRecordFieldDescriptor? Field { get; set; }

    /// <summary>
    /// Gets the list of value transformations to apply to the field's value, in order.
    /// </summary>
    [JsonInclude]
    public ImmutableList<ValueTransformationBase> ValueTransformations
    {
        get => _valueTransformations;
        protected set // Changed to protected set
        {
            if (EqualityComparer<ImmutableList<ValueTransformationBase>>.Default.Equals(_valueTransformations, value)) return;

            // Unsubscribe from old transformations
            foreach (var transformation in _valueTransformations)
            {
                transformation.OnDefinitionChanged -= HandleChildDefinitionChangedAsync;
            }

            _valueTransformations = value ?? ImmutableList<ValueTransformationBase>.Empty;

            // Subscribe to new transformations
            foreach (var transformation in _valueTransformations)
            {
                transformation.OnDefinitionChanged += HandleChildDefinitionChangedAsync;
            }
            _ = InvokeDefinitionChangedAsync(); // Fire and get task
        }
    }

    /// <summary>
    /// Event that is raised when the definition of this field transformation changes
    /// (either its own properties or a child transformation's definition).
    /// </summary>
    public event Func<Task>? OnDefinitionChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="FieldTransformation"/> class.
    /// </summary>
    public FieldTransformation()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FieldTransformation"/> class with a specific field.
    /// </summary>
    /// <param name="field">The field descriptor.</param>
    public FieldTransformation(ImportedRecordFieldDescriptor? field)
    {
        Field = field;
    }

    private Task InvokeDefinitionChangedAsync()
    {
        return OnDefinitionChanged?.Invoke() ?? Task.CompletedTask;
    }

    private Task HandleChildDefinitionChangedAsync()
    {
        return InvokeDefinitionChangedAsync();
    }

    #region ValueTransformation Management Methods

    /// <summary>
    /// Adds a value transformation to the end of the sequence.
    /// </summary>
    /// <param name="transformation">The transformation to add.</param>
    public virtual void AddTransformation(ValueTransformationBase transformation) // Made virtual
    {
        if (transformation == null) throw new ArgumentNullException(nameof(transformation));
        ValueTransformations = ValueTransformations.Add(transformation);
    }

    /// <summary>
    /// Removes a specific value transformation from the sequence.
    /// </summary>
    /// <param name="transformation">The transformation to remove.</param>
    public virtual void RemoveTransformation(ValueTransformationBase transformation) // Made virtual
    {
        if (transformation == null) throw new ArgumentNullException(nameof(transformation));
        ValueTransformations = ValueTransformations.Remove(transformation);
    }

    /// <summary>
    /// Removes a value transformation at the specified index.
    /// </summary>
    /// <param name="index">The index of the transformation to remove.</param>
    public virtual void RemoveTransformationAt(int index) // Made virtual
    {
        ValueTransformations = ValueTransformations.RemoveAt(index);
    }

    /// <summary>
    /// Replaces an existing value transformation with a new one.
    /// </summary>
    /// <param name="oldTransformation">The transformation to be replaced.</param>
    /// <param name="newTransformation">The new transformation.</param>
    public virtual void ReplaceTransformation(ValueTransformationBase oldTransformation, ValueTransformationBase newTransformation) // Made virtual
    {
        if (oldTransformation == null) throw new ArgumentNullException(nameof(oldTransformation));
        if (newTransformation == null) throw new ArgumentNullException(nameof(newTransformation));
        ValueTransformations = ValueTransformations.Replace(oldTransformation, newTransformation);
    }

    /// <summary>
    /// Replaces the value transformation at the specified index with a new one.
    /// </summary>
    /// <param name="index">The index of the transformation to replace.</param>
    /// <param name="newTransformation">The new transformation.</param>
    public virtual void ReplaceTransformationAt(int index, ValueTransformationBase newTransformation) // Made virtual
    {
        if (newTransformation == null) throw new ArgumentNullException(nameof(newTransformation));
        ValueTransformations = ValueTransformations.SetItem(index, newTransformation);
    }

    /// <summary>
    /// Clears all value transformations.
    /// </summary>
    public virtual void ClearTransformations() // Made virtual
    {
        ValueTransformations = ImmutableList<ValueTransformationBase>.Empty;
    }

    /// <summary>
    /// Sets all value transformations, replacing any existing ones.
    /// </summary>
    /// <param name="transformations">The collection of transformations to set.</param>
    public virtual void SetTransformations(IEnumerable<ValueTransformationBase> transformations) // Made virtual
    {
        ValueTransformations = transformations?.ToImmutableList() ?? ImmutableList<ValueTransformationBase>.Empty;
    }

    #endregion

    /// <summary>
    /// Creates a new object that is a copy of the current instance.
    /// </summary>
    /// <returns>A new object that is a copy of this instance.</returns>
    public virtual FieldTransformation Clone()
    {
        var clone = (FieldTransformation)this.MemberwiseClone();

        clone.OnDefinitionChanged = null; // Clear event subscriptions on the new clone

        clone.Field = this.Field?.Clone(); // Clone the field descriptor

        // Deep clone ValueTransformations and re-subscribe internal child event handlers for the clone
        var clonedValueTransformations = ImmutableList.CreateRange(this.ValueTransformations.Select(vt => vt.Clone()));
        clone._valueTransformations = ImmutableList<ValueTransformationBase>.Empty; // Start with empty to clear old (if any from memberwise)
        clone.ValueTransformations = clonedValueTransformations; // This will call the setter and wire up clone.HandleChildDefinitionChangedAsync

        return clone;
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
            // Unsubscribe from child transformations
            foreach (var transformation in _valueTransformations)
            {
                transformation.OnDefinitionChanged -= HandleChildDefinitionChangedAsync;
            }
            
            // Clear the main event's invocation list
            if (OnDefinitionChanged != null)
            {
                foreach(var d in OnDefinitionChanged.GetInvocationList())
                {
                    OnDefinitionChanged -= (Func<Task>)d;
                }
            }
        }
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return Equals(obj as FieldTransformation);
    }

    /// <inheritdoc />
    public bool Equals(FieldTransformation? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return EqualityComparer<ImportedRecordFieldDescriptor?>.Default.Equals(Field, other.Field) &&
               ValueTransformations.SequenceEqual(other.ValueTransformations);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        int hash = 17;
        hash = hash * 23 + (Field?.GetHashCode() ?? 0);
        foreach (var transformation in ValueTransformations)
        {
            hash = hash * 23 + (transformation?.GetHashCode() ?? 0);
        }
        return hash;
    }
}
