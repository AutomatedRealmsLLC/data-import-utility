using System.Text.Json.Serialization;

using DataImportUtility.CustomConverters;
using DataImportUtility.Models;

namespace DataImportUtility.Abstractions;

/// <summary>
/// The base class for a value transformation operation.
/// </summary>
/// <remarks>
/// Base items can be added using another partial class. The concrete class should follow the naming convention of
/// &lt;Name&gt;Operation.
/// </remarks>
[JsonConverter(typeof(ValueTransformationBaseConverter))]
public abstract partial class ValueTransformationBase : IDisposable
{
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
    /// The value the generated enum member display in the <see cref="ValueTransformationType" />.
    /// </summary>
    /// <remarks>
    /// The values for the enum members will be determined by sorting first by the 
    /// <see cref="EnumMemberOrder" />, then by the <see cref="EnumMemberName" />. This means that 
    /// any resulting duplicate orders will result in values that do not match the order value.
    /// </remarks>
    [JsonIgnore]
    public virtual int EnumMemberOrder { get; } = 0;

    /// <summary>
    /// The member name to use for the <see cref="ValueTransformationType"/>.
    /// </summary>
    /// <remarks>
    /// This should follow the rules used for Enum Member Names.
    /// </remarks>
    public abstract string EnumMemberName { get; }

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
    /// Generates the syntax for the operation based on the TransformationDetail.
    /// </summary>
    /// <returns>
    /// The result of the transformation.
    /// </returns>
    /// <remarks>
    /// If you add additional properties to the operation, you can include them in the syntax.
    /// </remarks>
    public virtual string GenerateSyntax() => TransformationDetail ?? string.Empty;

    /// <summary>
    /// Applies the transformation to the provided <see cref="TransformationResult" />.
    /// </summary>
    /// <param name="result">
    /// The result to apply the transformation to.
    /// </param>
    /// <returns>
    /// The result of the transformation as a <see cref="TransformationResult" />.
    /// </returns>
    public abstract Task<TransformationResult> Apply(TransformationResult result);

    /// <summary>
    /// Applies the transformation to the provided value.
    /// </summary>
    /// <param name="value">
    /// The value to apply the transformation to.
    /// </param>
    /// <returns>
    /// The result of the transformation as a <see cref="TransformationResult" />.
    /// </returns>
    public virtual Task<TransformationResult> Apply(string value)
        => Apply(new TransformationResult() { OriginalValue = value, Value = value });

    /// <summary>
    /// Clones the ValueTransformationBase.
    /// </summary>
    /// <returns>
    /// A new ValueTransformationBase that is a deep clone of the original.
    /// </returns>
    public virtual ValueTransformationBase Clone()
    {
        return (ValueTransformationBase)MemberwiseClone();
    }

    /// <summary>
    /// Clones the ValueTransformationBase.
    /// </summary>
    /// <returns>
    /// A new ValueTransformationBase that is a deep clone of the original.
    /// </returns>
    public virtual TRuleType Clone<TRuleType>() where TRuleType : ValueTransformationBase
        => (TRuleType)Clone();

    /// <inheritdoc />
    public virtual void Dispose()
    {
        OnDefinitionChanged = null;
    }
}