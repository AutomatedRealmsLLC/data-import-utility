using AutomatedRealms.DataImportUtility.Abstractions.Models;

using System.Text.Json.Serialization;

namespace AutomatedRealms.DataImportUtility.Abstractions;

/// <summary>
/// The base class for a value transformation operation.
/// </summary>
/// <remarks>
/// Base items can be added using another partial class. The concrete class should follow the naming convention of
/// &lt;Name&gt;Operation.
/// </remarks>
public abstract partial class ValueTransformationBase : IDisposable
{
    /// <summary>
    /// Gets the unique identifier for this type of value transformation.
    /// This is used for serialization and deserialization.
    /// </summary>
    public string TypeId { get; protected set; }

    /// <summary>
    /// The error message when the operation failed.
    /// </summary>
    public const string OperationFailedMessage = "The operation failed.";
    /// <summary>
    /// The error message when the operation is not supported for collections.
    /// </summary>
    public const string OperationInvalidForCollectionsMessage = "The operation cannot be applied to a collection of values.";
    /// <summary>
    /// The error message when an aggregation operation has an error in part of the chain.
    /// </summary>
    public const string AggregationOperationErrorMessage = "An error occurred in the aggregation operation. Check the configuration of the field transformations for more details.";

    /// <summary>
    /// The event that is raised when the definition of the operation changes.
    /// </summary>
    public event Func<Task>? OnDefinitionChanged;

    /// <summary>
    /// The name of the operation.
    /// </summary>
    [JsonIgnore]
    public abstract string DisplayName { get; }

    /// <summary>
    /// The short name to display for the operation.
    /// </summary>
    [JsonIgnore]
    public virtual string ShortName { get; } = string.Empty;

    /// <summary>
    /// The description of the operation.
    /// </summary>
    [JsonIgnore]
    public abstract string Description { get; }
    /// <summary>
    /// The error message if the operation failed.
    /// </summary>
    [JsonIgnore]
    public string? ErrorMessage { get; protected set; }
    /// <summary>
    /// Whether the transformation syntax is valid.
    /// </summary>
    [JsonIgnore]
    public bool IsError => !string.IsNullOrWhiteSpace(ErrorMessage);
    /// <summary>
    /// The additional arguments for the operation.
    /// </summary>
    public virtual string? TransformationDetail
    {
        get => _operationDetail;
        set
        {
            _operationDetail = value;
            ValidateDetail(value);
            OnDefinitionChanged?.Invoke();
        }
    }
    /// <summary>
    /// The backing field for the TransformationDetail property.
    /// It should only be used in the derived class when 
    /// overriding the TransformationDetail property.
    /// </summary>
    protected string? _operationDetail;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueTransformationBase"/> class.
    /// </summary>
    /// <param name="typeId">The unique identifier for this transformation type.</param>
    protected ValueTransformationBase(string typeId)
    {
        if (string.IsNullOrWhiteSpace(typeId))
        {
            throw new ArgumentException("TypeId cannot be null or whitespace.", nameof(typeId));
        }
        TypeId = typeId;
    }

    /// <summary>
    /// Validates the transformation detail.
    /// </summary>
    /// <param name="detail">The detail to validate.</param>
    protected virtual void ValidateDetail(string? detail)
    {
        // Base implementation does no validation.
        // Derived classes should override this method to provide specific validation logic.
        // If validation fails, set the ErrorMessage property.
        ErrorMessage = null; // Clear previous errors
    }

    /// <summary>
    /// Applies the transformation to the given input value.
    /// </summary>
    /// <param name="previousResult">The result of the previous transformation or the initial state.</param>
    /// <returns>A task representing the asynchronous operation, with a result of TransformationResult.</returns>
    public abstract Task<TransformationResult> ApplyTransformationAsync(TransformationResult previousResult);

    /// <summary>
    /// An indicator of whether the operation is empty.
    /// </summary>
    [JsonIgnore]
    public abstract bool IsEmpty { get; }

    /// <summary>
    /// The type of the value that the operation will produce.
    /// </summary>
    [JsonIgnore]
    public abstract Type OutputType { get; }

    /// <summary>
    /// Transforms the value.
    /// </summary>
    /// <param name="value">The value to transform.</param>
    /// <param name="targetType">The type to convert the value to.</param>
    /// <returns>The transformed value.</returns>
    public abstract Task<TransformationResult> Transform(object? value, Type targetType);

    /// <summary>
    /// Creates a new object that is a copy of the current instance.
    /// </summary>
    /// <returns>A new object that is a copy of this instance.</returns>
    public abstract ValueTransformationBase Clone();

    /// <summary>
    /// Helper method to clone base properties. Derived classes should call this in their Clone implementation.
    /// </summary>
    /// <param name="target">The target transformation to clone properties to.</param>
    protected virtual void CloneBaseProperties(ValueTransformationBase target)
    {
        target.TransformationDetail = this.TransformationDetail; // This will also trigger ValidateDetail
        // TypeId is set in constructor.
        // DisplayName, ShortName, Description are abstract or get-only, defined by concrete types.
        // ErrorMessage is protected set, typically not cloned directly unless state is part of the clone.
    }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString() => DisplayName;

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        return obj is ValueTransformationBase other && DisplayName == other.DisplayName &&
                   Description == other.Description &&
                   TransformationDetail == other.TransformationDetail &&
                   OutputType == other.OutputType;
    }

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode()
    {
        // Manual hash code combination for .NET Standard 2.0 compatibility
        int hash = 17;
        // DisplayName, Description and OutputType are abstract and should be non-null
        // TransformationDetail can be null
        hash = hash * 23 + (DisplayName?.GetHashCode() ?? 0);
        hash = hash * 23 + (Description?.GetHashCode() ?? 0);
        hash = hash * 23 + (TransformationDetail?.GetHashCode() ?? 0);
        hash = hash * 23 + (OutputType?.GetHashCode() ?? 0);
        return hash;
    }

    /// <summary>
    /// Disposes the object.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the object.
    /// </summary>
    /// <param name="disposing">True if called from Dispose, false if called from the finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Clear event subscriptions
            if (OnDefinitionChanged is not null)
            {
                foreach (var d in OnDefinitionChanged.GetInvocationList())
                {
                    OnDefinitionChanged -= (Func<Task>)d;
                }
            }
        }
    }
}
