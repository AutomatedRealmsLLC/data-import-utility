using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using AutomatedRealms.DataImportUtility.Abstractions.Models; // Should now correctly find ImportTableDefinition

namespace AutomatedRealms.DataImportUtility.Abstractions.Models;

/// <summary>
/// The result of a transformation operation.
/// </summary>
public partial record TransformationResult
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
    public System.Data.DataRow? Record { get; set; }

    /// <summary>
    /// The definition of the table from which the record originates, if applicable.
    /// </summary>
    [JsonIgnore]
    public ImportTableDefinition? TableDefinition { get; set; }

    /// <summary>
    /// Creates a new successful TransformationResult.
    /// </summary>
    public static TransformationResult Success(object? originalValue, Type? originalValueType, object? currentValue, Type? currentValueType, IEnumerable<string>? appliedTransformations = null, System.Data.DataRow? record = null, ImportTableDefinition? tableDefinition = null)
    {
        return new TransformationResult
        {
            OriginalValue = originalValue,
            OriginalValueType = originalValueType,
            CurrentValue = currentValue,
            CurrentValueType = currentValueType,
            AppliedTransformations = appliedTransformations ?? new List<string>(),
            Record = record,
            TableDefinition = tableDefinition
        };
    }

    /// <summary>
    /// Creates a new failed TransformationResult.
    /// </summary>
    public static TransformationResult Failure(object? originalValue, Type? targetType, string errorMessage, Type? originalValueType = null, Type? currentValueType = null, IEnumerable<string>? appliedTransformations = null, System.Data.DataRow? record = null, ImportTableDefinition? tableDefinition = null)
    {
        return new TransformationResult
        {
            OriginalValue = originalValue,
            OriginalValueType = originalValueType ?? (originalValue?.GetType()),
            CurrentValue = null,
            CurrentValueType = currentValueType,
            ErrorMessage = errorMessage,
            AppliedTransformations = appliedTransformations ?? new List<string>(),
            Record = record,
            TableDefinition = tableDefinition
        };
    }
}
