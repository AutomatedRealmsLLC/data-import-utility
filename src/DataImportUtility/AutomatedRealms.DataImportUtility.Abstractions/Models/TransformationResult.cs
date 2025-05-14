using System.Data;
using System.Text.Json.Serialization;

namespace AutomatedRealms.DataImportUtility.Abstractions.Models;

/// <summary>
/// The result of a transformation operation.
/// </summary>
public partial record TransformationResult : ITransformationContext // Implements ITransformationContext
{
    /// <summary>
    /// The type of the original value.
    /// </summary>
    public Type? OriginalValueType { get; set; }

    /// <summary>
    /// The original value before transformations are applied.
    /// Can be any type, typically the raw data from the source.
    /// </summary>
    public object? OriginalValue { get; set; }

    /// <summary>
    /// The type of the current value after transformation (if successful).
    /// </summary>
    public Type? CurrentValueType { get; set; }

    /// <summary>
    /// The value after transformation. 
    /// Can be any type, representing the state of the data after one or more transformations.
    /// </summary>
    public object? CurrentValue { get; set; }

    /// <summary>
    /// The error message if the transformation failed.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// A list of transformation descriptions that were applied to reach the current state.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public IEnumerable<string>? AppliedTransformations { get; set; }

    /// <summary>
    /// Whether the transformation was successful.
    /// </summary>
    [JsonIgnore]
    public bool WasFailure => !string.IsNullOrWhiteSpace(ErrorMessage);

    /// <summary>
    /// The DataRow associated with this transformation, if applicable.
    /// This provides context for rules that need to access other fields from the same row.
    /// </summary>
    [JsonIgnore]
    public DataRow? Record { get; set; }

    /// <summary>
    /// The definition of the table from which the record originates, if applicable.
    /// </summary>
    [JsonIgnore]
    public ImportTableDefinition? TableDefinition { get; set; }

    /// <summary>
    /// Gets or sets the context of the source record, typically a list of field descriptors.
    /// </summary>
    [JsonIgnore]
    public List<ImportedRecordFieldDescriptor>? SourceRecordContext { get; set; }

    /// <summary>
    /// Gets or sets the expected target type of the field being transformed.
    /// </summary>
    [JsonIgnore]
    public Type? TargetFieldType { get; set; }

    /// <summary>
    /// Creates a new successful TransformationResult.
    /// </summary>
    public static TransformationResult Success(
        object? originalValue,
        Type? originalValueType,
        object? currentValue,
        Type? currentValueType,
        IEnumerable<string>? appliedTransformations = null,
        DataRow? record = null,
        ImportTableDefinition? tableDefinition = null,
        List<ImportedRecordFieldDescriptor>? sourceRecordContext = null, // Added
        Type? targetFieldType = null) // Added
    {
        return new TransformationResult
        {
            OriginalValue = originalValue,
            OriginalValueType = originalValueType,
            CurrentValue = currentValue,
            CurrentValueType = currentValueType,
            AppliedTransformations = appliedTransformations ?? [],
            Record = record,
            TableDefinition = tableDefinition,
            SourceRecordContext = sourceRecordContext, // Added
            TargetFieldType = targetFieldType // Added
        };
    }

    /// <summary>
    /// Creates a new failed TransformationResult.
    /// </summary>
    public static TransformationResult Failure(
        object? originalValue,
        Type? targetType, // This is likely the TargetFieldType for context of failure
        string errorMessage,
        Type? originalValueType = null,
        Type? currentValueType = null, // Usually null on failure
        IEnumerable<string>? appliedTransformations = null,
        DataRow? record = null,
        ImportTableDefinition? tableDefinition = null,
        List<ImportedRecordFieldDescriptor>? sourceRecordContext = null, // Added
        Type? explicitTargetFieldType = null) // Added, renamed from targetType to avoid conflict if targetType param is used for something else
    {
        return new TransformationResult
        {
            OriginalValue = originalValue,
            OriginalValueType = originalValueType ?? (originalValue?.GetType()),
            CurrentValue = null,
            CurrentValueType = currentValueType, // Or perhaps targetType if it represents the intended conversion type
            ErrorMessage = errorMessage,
            AppliedTransformations = appliedTransformations ?? [],
            Record = record,
            TableDefinition = tableDefinition,
            SourceRecordContext = sourceRecordContext, // Added
            TargetFieldType = explicitTargetFieldType ?? targetType // Added, use explicit if provided, else the existing targetType param
        };
    }
}
