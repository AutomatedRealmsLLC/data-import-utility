using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Models; // Added for TransformationContext
using AutomatedRealms.DataImportUtility.Core.ValueTransformations;

using Microsoft.Extensions.Logging;

using System.Data;
using System.Text.Json.Serialization;

namespace AutomatedRealms.DataImportUtility.Core.Rules;

/// <summary>
/// Describes a source field to be used in a combine operation, including its name and any transformations to apply to it.
/// </summary>
public class ConfiguredInputField
{
    /// <summary>
    /// Gets or sets the name of the source field.
    /// </summary>
    public string FieldName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a constant value to be used for this input.
    /// If this is set, FieldName might be ignored.
    /// </summary>
    public string? ConstantValue { get; set; }

    /// <summary>
    /// Gets or sets the list of value transformations to apply to this field before combining.
    /// </summary>
    public List<ValueTransformationBase> ValueTransformations { get; set; } = [];

    /// <summary>
    /// Clones this configured input field.
    /// </summary>
    /// <returns>A new instance of <see cref="ConfiguredInputField"/> with copied values.</returns>
    public ConfiguredInputField Clone()
    {
        var clone = new ConfiguredInputField
        {
            FieldName = this.FieldName,
            ConstantValue = this.ConstantValue,
            ValueTransformations = [.. this.ValueTransformations.Select(t => t.Clone())]
        };
        return clone;
    }
}

/// <summary>
/// A rule indicating the value should be combined from multiple source fields using a format string.
/// </summary>
public class CombineFieldsRule : MappingRuleBase
{
    private readonly ILogger _logger;

    /// <summary>
    /// Gets the unique identifier for this type of mapping rule.
    /// </summary>
    public static readonly string TypeIdString = "Core.CombineFieldsRule";

    /// <summary>
    /// Gets or sets the list of input fields to be combined. Each field can have its own transformations.
    /// </summary>
    public List<ConfiguredInputField> InputFields { get; set; } = [];

    /// <summary>
    /// Gets or sets the format string used to combine the field values (e.g., "{0} - {1}").
    /// The placeholders correspond to the order of fields in <see cref="InputFields"/>.
    /// </summary>
    public string? CombinationFormat { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CombineFieldsRule"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public CombineFieldsRule(ILogger<CombineFieldsRule> logger) : base(TypeIdString)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CombineFieldsRule"/> class. Used for cloning.
    /// </summary>
    private CombineFieldsRule() : base(TypeIdString)
    {
        _logger = null!;
    }

    /// <summary>
    /// Gets the display name of the mapping rule.
    /// </summary>
    public override string DisplayName => "Combine Fields";

    /// <summary>
    /// Gets the short name of the mapping rule.
    /// </summary>
    public override string ShortName => "Combine";

    /// <summary>
    /// Gets the description of the mapping rule.
    /// </summary>
    [JsonIgnore]
    public override string Description => "Combine the values of multiple source fields into the output field using a format string.";

    /// <summary>
    /// Indicates whether the mapping rule is empty or not configured.
    /// For a CombineFieldsRule, it's empty if there are no input fields or no combination format string.
    /// </summary>
    [JsonIgnore]
    public override bool IsEmpty => !InputFields.Any() || string.IsNullOrWhiteSpace(CombinationFormat);

    /// <summary>
    /// Applies the combine fields rule to the provided transformation context.
    /// </summary>
    /// <param name="context">The transformation context containing the data row and other relevant information.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the transformation result, or null if the rule could not be applied.</returns>
    public override async Task<TransformationResult?> Apply(ITransformationContext context)
    {
        _logger?.LogDebug("CombineFieldsRule.Apply(ITransformationContext) for target '{TargetField}' started.", TargetField ?? context.TargetFieldType?.Name);
        if (IsEmpty)
        {
            _logger?.LogWarning("CombineFieldsRule for target '{TargetField}' is not configured: InputFields or CombinationFormat is missing.", TargetField);
            return TransformationResult.Failure(
                originalValue: null,
                targetType: context.TargetFieldType ?? typeof(string),
                errorMessage: "CombineFieldsRule is not configured: InputFields or CombinationFormat is missing.",
                record: context.Record,
                sourceRecordContext: context.SourceRecordContext,
                explicitTargetFieldType: context.TargetFieldType ?? typeof(string));
        }

        DataRow? dataRow = context.Record;
        if (dataRow == null && InputFields.Any(cf => string.IsNullOrEmpty(cf.ConstantValue) && !string.IsNullOrEmpty(cf.FieldName)))
        {
            _logger?.LogWarning("CombineFieldsRule for target '{TargetField}' requires a DataRow when FieldNames are specified and no ConstantValue is provided.", TargetField);
            return TransformationResult.Failure(
                originalValue: null,
                targetType: context.TargetFieldType ?? typeof(string),
                errorMessage: "CombineFieldsRule requires a DataRow in the context when FieldNames are specified and no ConstantValue is provided for those fields.",
                record: null,
                sourceRecordContext: context.SourceRecordContext,
                explicitTargetFieldType: context.TargetFieldType ?? typeof(string));
        }

        var collectedValuesForCombination = new List<object?>();
        var appliedTransformationsLog = new List<string> { $"CombineFieldsRule.Apply(ITransformationContext) for target '{TargetField ?? context.TargetFieldType?.Name}' processing." };

        foreach (var configuredField in InputFields)
        {
            object? initialValue = null;
            Type? initialValueType = null;
            List<string> fieldLog = [];
            string fieldIdentifier;

            if (!string.IsNullOrEmpty(configuredField.ConstantValue))
            {
                initialValue = configuredField.ConstantValue;
                initialValueType = typeof(string);
                fieldIdentifier = $"constant value '{initialValue}'";
                fieldLog.Add($"Using {fieldIdentifier}.");
                _logger?.LogTrace("Using {FieldIdentifier} for target '{TargetField}'.", fieldIdentifier, TargetField);
            }
            else if (!string.IsNullOrEmpty(configuredField.FieldName))
            {
                fieldIdentifier = $"field '{configuredField.FieldName}'";
                if (dataRow != null && dataRow.Table.Columns.Contains(configuredField.FieldName))
                {
                    initialValue = dataRow[configuredField.FieldName];
                    if (initialValue == DBNull.Value) initialValue = null;
                    initialValueType = initialValue?.GetType();
                    fieldLog.Add($"Initial value from {fieldIdentifier}: '{initialValue ?? "null"}'.");
                    _logger?.LogTrace("Initial value from {FieldIdentifier}: '{InitialValue}' for target '{TargetField}'.", fieldIdentifier, initialValue ?? "null", TargetField);
                }
                else
                {
                    fieldLog.Add($"Source {fieldIdentifier} not found in DataRow (or DataRow is null) and no ConstantValue. Using null for this input.");
                    _logger?.LogDebug("Source {FieldIdentifier} not found or DataRow null for target '{TargetField}'. Using null.", fieldIdentifier, TargetField);
                    initialValue = null;
                    initialValueType = null;
                }
            }
            else
            {
                fieldIdentifier = "an unnamed input without constant value";
                fieldLog.Add($"Configured field has no FieldName and no ConstantValue. Initial value is null.");
                _logger?.LogDebug("Configured input for target '{TargetField}' has no FieldName or ConstantValue. Using null.", TargetField);
                initialValue = null;
                initialValueType = null;
            }

            TransformationResult currentStepResult = TransformationResult.Success(
                originalValue: initialValue,
                originalValueType: initialValueType,
                currentValue: initialValue,
                currentValueType: initialValueType,
                appliedTransformations: fieldLog,
                record: dataRow,
                sourceRecordContext: context.SourceRecordContext,
                targetFieldType: null
            );

            foreach (var valueTransformation in configuredField.ValueTransformations)
            {
                _logger?.LogTrace("Applying transformation '{TransformationDisplayName}' to {FieldIdentifier} for target '{TargetField}'.", valueTransformation.DisplayName, fieldIdentifier, TargetField);
                currentStepResult = await valueTransformation.ApplyTransformationAsync(currentStepResult);
                if (currentStepResult.WasFailure)
                {
                    appliedTransformationsLog.Add($"Failure during value transformation for {fieldIdentifier}: {currentStepResult.ErrorMessage}");
                    _logger?.LogWarning("Failure during value transformation for {FieldIdentifier} for target '{TargetField}': {ErrorMessage}", fieldIdentifier, TargetField, currentStepResult.ErrorMessage);
                    return TransformationResult.Failure(
                        originalValue: currentStepResult.OriginalValue,
                        targetType: context.TargetFieldType ?? typeof(string),
                        errorMessage: $"Failed to transform {fieldIdentifier} for combination: {currentStepResult.ErrorMessage}",
                        originalValueType: currentStepResult.OriginalValueType,
                        currentValueType: currentStepResult.CurrentValueType,
                        record: dataRow,
                        appliedTransformations: [.. appliedTransformationsLog.Concat(currentStepResult.AppliedTransformations ?? []).Distinct()],
                        sourceRecordContext: context.SourceRecordContext,
                        explicitTargetFieldType: context.TargetFieldType ?? typeof(string)
                    );
                }
            }
            collectedValuesForCombination.Add(currentStepResult.CurrentValue);
            appliedTransformationsLog.AddRange(currentStepResult.AppliedTransformations?.Except(fieldLog).ToList() ?? Enumerable.Empty<string>());
            appliedTransformationsLog.Add($"Successfully processed {fieldIdentifier}. Final value for combination: '{currentStepResult.CurrentValue ?? "null"}'.");
            _logger?.LogTrace("Successfully processed {FieldIdentifier} for target '{TargetField}'. Value: '{CurrentValue}'.", fieldIdentifier, TargetField, currentStepResult.CurrentValue ?? "null");
        }

        var combineTransformation = new CombineFieldsTransformation
        {
            TransformationDetail = this.CombinationFormat
        };
        _logger?.LogTrace("Preparing to combine {NumValues} values for target '{TargetField}' using format '{CombinationFormat}'.", collectedValuesForCombination.Count, TargetField, CombinationFormat);

        TransformationResult inputForCombine = TransformationResult.Success(
            originalValue: collectedValuesForCombination.ToArray(),
            originalValueType: typeof(object[]),
            currentValue: collectedValuesForCombination.ToArray(),
            currentValueType: typeof(object[]),
            appliedTransformations: ["Preparing to combine collected values."],
            record: dataRow,
            sourceRecordContext: context.SourceRecordContext,
            targetFieldType: typeof(string)
        );

        TransformationResult finalResult = await combineTransformation.ApplyTransformationAsync(inputForCombine);

        List<string> combinedLogs = appliedTransformationsLog;
        if (finalResult.AppliedTransformations != null)
        {
            combinedLogs.AddRange(finalResult.AppliedTransformations.Except(inputForCombine.AppliedTransformations ?? []));
        }

        var result = finalResult with
        {
            Record = dataRow,
            SourceRecordContext = context.SourceRecordContext,
            TargetFieldType = context.TargetFieldType ?? typeof(string),
            AppliedTransformations = [.. combinedLogs.Distinct()]
        };
        _logger?.LogDebug("CombineFieldsRule.Apply(ITransformationContext) for target '{TargetField}' completed. Success: {WasSuccess}, Value: '{CurrentValue}'.", TargetField, !result.WasFailure, result.CurrentValue);
        return result;
    }

    /// <summary>
    /// Applies the combine fields rule when no specific data context is provided.
    /// This overload is typically used when all input fields are configured with constant values.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable of transformation results.</returns>
    public override async Task<IEnumerable<TransformationResult?>> Apply()
    {
        _logger?.LogDebug("CombineFieldsRule.Apply() for target '{TargetField}' started (no context).", TargetField);
        var initialLogs = new List<string> { $"CombineFieldsRule.Apply() for target '{TargetField}' processing." };

        if (IsEmpty)
        {
            var emptyMsg = "CombineFieldsRule is not configured: InputFields or CombinationFormat is missing.";
            _logger?.LogWarning(emptyMsg + " Target: '{TargetField}'.", TargetField);
            initialLogs.Add(emptyMsg);
            return [TransformationResult.Failure(null, typeof(string), emptyMsg, appliedTransformations: initialLogs, explicitTargetFieldType: typeof(string))];
        }

        if (InputFields.Any(cf => string.IsNullOrEmpty(cf.ConstantValue) && !string.IsNullOrEmpty(cf.FieldName)))
        {
            var failureMsg = "CombineFieldsRule.Apply() cannot be used if any input field relies on FieldName without a ConstantValue.";
            _logger?.LogWarning(failureMsg + " Target: '{TargetField}'.", TargetField);
            initialLogs.Add(failureMsg);
            var failureResult = TransformationResult.Failure(null, typeof(string), failureMsg, appliedTransformations: initialLogs, explicitTargetFieldType: typeof(string));
            return [failureResult];
        }

        if (InputFields.Any(cf => string.IsNullOrEmpty(cf.ConstantValue) && string.IsNullOrEmpty(cf.FieldName)))
        {
            var failureMsg = "CombineFieldsRule.Apply() requires all input fields to have a ConstantValue if FieldName is not specified.";
            _logger?.LogWarning(failureMsg + " Target: '{TargetField}'.", TargetField);
            initialLogs.Add(failureMsg);
            var failureResult = TransformationResult.Failure(null, typeof(string), failureMsg, appliedTransformations: initialLogs, explicitTargetFieldType: typeof(string));
            return [failureResult];
        }

        var collectedValuesForCombination = new List<object?>();
        var appliedTransformationsLog = new List<string>(initialLogs);

        foreach (var configuredField in InputFields)
        {
            object? initialValue = configuredField.ConstantValue;
            Type? initialValueType = typeof(string);
            List<string> fieldLog = [];
            string fieldIdentifier = $"constant value '{initialValue}'";
            fieldLog.Add($"Using {fieldIdentifier}.");
            _logger?.LogTrace("Using {FieldIdentifier} for target '{TargetField}' (no context).", fieldIdentifier, TargetField);

            TransformationResult currentStepResult = TransformationResult.Success(
                originalValue: initialValue,
                originalValueType: initialValueType,
                currentValue: initialValue,
                currentValueType: initialValueType,
                appliedTransformations: fieldLog,
                record: null,
                sourceRecordContext: null,
                targetFieldType: null
            );

            foreach (var valueTransformation in configuredField.ValueTransformations)
            {
                _logger?.LogTrace("Applying transformation '{TransformationDisplayName}' to {FieldIdentifier} for target '{TargetField}' (no context).", valueTransformation.DisplayName, fieldIdentifier, TargetField);
                currentStepResult = await valueTransformation.ApplyTransformationAsync(currentStepResult);
                if (currentStepResult.WasFailure)
                {
                    appliedTransformationsLog.Add($"Failure during value transformation for {fieldIdentifier}: {currentStepResult.ErrorMessage}");
                    _logger?.LogWarning("Failure during value transformation for {FieldIdentifier} for target '{TargetField}' (no context): {ErrorMessage}", fieldIdentifier, TargetField, currentStepResult.ErrorMessage);
                    var overallFailure = TransformationResult.Failure(
                        originalValue: string.Join(", ", InputFields.Select(cf => cf.ConstantValue ?? "null")),
                        targetType: typeof(string),
                        errorMessage: $"Failed to transform {fieldIdentifier} for combination: {currentStepResult.ErrorMessage}",
                        appliedTransformations: [.. appliedTransformationsLog.Concat(currentStepResult.AppliedTransformations ?? []).Distinct()],
                        explicitTargetFieldType: typeof(string)
                    );
                    return [overallFailure];
                }
            }
            collectedValuesForCombination.Add(currentStepResult.CurrentValue);
            appliedTransformationsLog.AddRange(currentStepResult.AppliedTransformations?.Except(fieldLog).ToList() ?? Enumerable.Empty<string>());
            appliedTransformationsLog.Add($"Successfully processed {fieldIdentifier}. Final value for combination: '{currentStepResult.CurrentValue ?? "null"}'.");
            _logger?.LogTrace("Successfully processed {FieldIdentifier} for target '{TargetField}' (no context). Value: '{CurrentValue}'.", fieldIdentifier, TargetField, currentStepResult.CurrentValue ?? "null");
        }

        var combineTransformation = new CombineFieldsTransformation { TransformationDetail = this.CombinationFormat };
        _logger?.LogTrace("Preparing to combine {NumValues} constant values for target '{TargetField}' using format '{CombinationFormat}' (no context).", collectedValuesForCombination.Count, TargetField, CombinationFormat);

        TransformationResult inputForCombine = TransformationResult.Success(
            originalValue: collectedValuesForCombination.ToArray(),
            originalValueType: typeof(object[]),
            currentValue: collectedValuesForCombination.ToArray(),
            currentValueType: typeof(object[]),
            appliedTransformations: ["Preparing to combine collected constant values."],
            targetFieldType: typeof(string)
        );

        TransformationResult finalResult = await combineTransformation.ApplyTransformationAsync(inputForCombine);

        List<string> combinedLogs = appliedTransformationsLog;
        if (finalResult.AppliedTransformations != null)
        {
            combinedLogs.AddRange(finalResult.AppliedTransformations.Except(inputForCombine.AppliedTransformations ?? []));
        }

        var finalResultWithLogs = finalResult with { AppliedTransformations = [.. combinedLogs.Distinct()], TargetFieldType = typeof(string) };
        _logger?.LogDebug("CombineFieldsRule.Apply() for target '{TargetField}' (no context) completed. Success: {WasSuccess}, Value: '{CurrentValue}'.", TargetField, !finalResultWithLogs.WasFailure, finalResultWithLogs.CurrentValue);
        return [finalResultWithLogs];
    }

    /// <summary>
    /// Applies the combine fields rule to a single DataRow.
    /// </summary>
    /// <param name="dataRow">The DataRow to apply the rule to.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the transformation result, or null if the rule could not be applied.</returns>
    public override async Task<TransformationResult?> Apply(DataRow dataRow)
    {
        _logger?.LogDebug("CombineFieldsRule.Apply(DataRow) for target '{TargetField}' started.", TargetField);

        Type resolvedTargetFieldType = typeof(string); // Default
        if (this.ParentTableDefinition != null && !string.IsNullOrEmpty(this.TargetField))
        {
            var descriptor = this.ParentTableDefinition.FieldDescriptors?.FirstOrDefault(fd => fd.FieldName == this.TargetField);
            if (descriptor?.FieldType != null)
            {
                resolvedTargetFieldType = descriptor.FieldType;
            }
        }

        // Construct SourceRecordContext from DataRow columns
        List<ImportedRecordFieldDescriptor>? sourceRecordContext = dataRow?.Table.Columns
            .Cast<DataColumn>()
            .Select(col => new ImportedRecordFieldDescriptor
            {
                FieldName = col.ColumnName,
                FieldType = col.DataType,
            })
            .ToList();

        var initialContext = TransformationResult.Success(
            originalValue: null,
            originalValueType: null,
            currentValue: null,
            currentValueType: resolvedTargetFieldType, // Use resolved type
            record: dataRow,
            tableDefinition: this.ParentTableDefinition,
            sourceRecordContext: sourceRecordContext,
            targetFieldType: resolvedTargetFieldType, // Use resolved type
            appliedTransformations: [$"Initial context for DataRow processing of target '{this.TargetField}'."]
        );

        var result = await Apply(initialContext); // Call the ITransformationContext overload

        if (result == null)
        {
            _logger?.LogWarning("CombineFieldsRule.Apply(DataRow) for target '{TargetField}' resulted in a null TransformationResult. Returning a failure.", TargetField);
            return TransformationResult.Failure(
                originalValue: null,
                targetType: resolvedTargetFieldType, // Use resolved type
                errorMessage: "CombineFieldsRule.Apply(DataRow) failed to produce a result.",
                record: dataRow,
                sourceRecordContext: sourceRecordContext,
                explicitTargetFieldType: resolvedTargetFieldType // Use resolved type
            );
        }
        _logger?.LogDebug("CombineFieldsRule.Apply(DataRow) for target '{TargetField}' completed. Success: {WasSuccess}, Value: '{CurrentValue}'.", TargetField, !result.WasFailure, result.CurrentValue);
        return result;
    }

    /// <summary>
    /// Applies the combine fields rule to each DataRow in a DataTable.
    /// </summary>
    /// <param name="data">The DataTable to apply the rule to.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable of transformation results for each row.</returns>
    public override async Task<IEnumerable<TransformationResult?>> Apply(DataTable data)
    {
        _logger?.LogDebug("CombineFieldsRule.Apply(DataTable) for target '{TargetField}' started for {RowCount} rows.", TargetField, data?.Rows?.Count ?? 0);

        Type resolvedTargetFieldType = typeof(string); // Default
        if (this.ParentTableDefinition != null && !string.IsNullOrEmpty(this.TargetField))
        {
            var descriptor = this.ParentTableDefinition.FieldDescriptors?.FirstOrDefault(fd => fd.FieldName == this.TargetField);
            if (descriptor?.FieldType != null)
            {
                resolvedTargetFieldType = descriptor.FieldType;
            }
        }

        if (data == null)
        {
            _logger?.LogWarning("CombineFieldsRule.Apply(DataTable) for target '{TargetField}' received a null DataTable.", TargetField);
            return [
                TransformationResult.Failure(null, resolvedTargetFieldType, "Input DataTable is null.", explicitTargetFieldType: resolvedTargetFieldType)
            ];
        }

        var results = new List<TransformationResult?>();
        foreach (DataRow row in data.Rows)
        {
            results.Add(await Apply(row)); // This will call the updated Apply(DataRow)
        }
        _logger?.LogDebug("CombineFieldsRule.Apply(DataTable) for target '{TargetField}' completed for {RowCount} rows.", TargetField, data.Rows.Count);
        return results;
    }

    /// <summary>
    /// Gets the combined value from the source record based on the rule's configuration.
    /// This method is typically used in scenarios where an ITransformationContext is not available or suitable.
    /// </summary>
    /// <param name="sourceRecord">The list of source record field descriptors. Each descriptor provides the name and value of a field from the source.</param>
    /// <param name="targetField">The target field descriptor, indicating the field to populate and its expected type.</param>
    /// <returns>The transformation result containing the combined value and details of the operation.</returns>
    public override TransformationResult GetValue(List<ImportedRecordFieldDescriptor> sourceRecord, ImportedRecordFieldDescriptor targetField)
    {
        string targetFieldNameForLog = targetField?.FieldName ?? this.TargetField ?? "UnknownTarget";
        _logger?.LogDebug("CombineFieldsRule.GetValue for target '{TargetFieldNameForLog}' started.", targetFieldNameForLog);
        Type targetType = targetField?.FieldType ?? typeof(string);
        if (IsEmpty)
        {
            _logger?.LogWarning("CombineFieldsRule for target '{TargetFieldNameForLog}' is not configured: InputFields or CombinationFormat is missing.", targetFieldNameForLog);
            return TransformationResult.Failure(null, targetType, "CombineFieldsRule is not configured.", sourceRecordContext: sourceRecord, explicitTargetFieldType: targetType);
        }

        var collectedValuesForCombination = new List<object?>();
        var appliedTransformationsLog = new List<string> { $"CombineFieldsRule.GetValue for target '{targetFieldNameForLog}' processing." };

        foreach (var configuredField in InputFields)
        {
            object? initialValue = null;
            Type? initialValueType = null;
            List<string> fieldLog = [];
            string fieldIdentifier;

            if (!string.IsNullOrEmpty(configuredField.ConstantValue))
            {
                initialValue = configuredField.ConstantValue;
                initialValueType = typeof(string);
                fieldIdentifier = $"constant value '{initialValue}'";
                fieldLog.Add($"Using {fieldIdentifier}.");
                _logger?.LogTrace("Using {FieldIdentifier} for target '{TargetFieldNameForLog}'.", fieldIdentifier, targetFieldNameForLog);
            }
            else if (!string.IsNullOrEmpty(configuredField.FieldName))
            {
                fieldIdentifier = $"field '{configuredField.FieldName}'";
                var fieldDesc = sourceRecord?.FirstOrDefault(f => f.FieldName == configuredField.FieldName);
                if (fieldDesc == null)
                {
                    fieldLog.Add($"Source {fieldIdentifier} not found in sourceRecord and no ConstantValue. Using null for this input.");
                    _logger?.LogDebug("Source {FieldIdentifier} not found in sourceRecord for target '{TargetFieldNameForLog}'. Using null.", fieldIdentifier, targetFieldNameForLog);
                    initialValue = null;
                    initialValueType = null;
                }
                else
                {
                    initialValue = fieldDesc.ValueSet?.FirstOrDefault();
                    if (initialValue == DBNull.Value) initialValue = null;
                    initialValueType = initialValue?.GetType() ?? fieldDesc.FieldType;
                    fieldLog.Add($"Initial value from {fieldIdentifier}: '{initialValue ?? "null"}'.");
                    _logger?.LogTrace("Initial value from {FieldIdentifier}: '{InitialValue}' for target '{TargetFieldNameForLog}'.", fieldIdentifier, initialValue ?? "null", targetFieldNameForLog);
                }
            }
            else
            {
                fieldIdentifier = "an unnamed input without constant value";
                fieldLog.Add($"Configured field has no FieldName and no ConstantValue. Initial value is null.");
                _logger?.LogDebug("Configured input for target '{TargetFieldNameForLog}' has no FieldName or ConstantValue. Using null.", targetFieldNameForLog);
                initialValue = null;
                initialValueType = null;
            }

            TransformationResult currentStepResult = TransformationResult.Success(
                originalValue: initialValue,
                originalValueType: initialValueType,
                currentValue: initialValue,
                currentValueType: initialValueType,
                appliedTransformations: fieldLog,
                sourceRecordContext: sourceRecord,
                targetFieldType: null
            );

            foreach (var valueTransformation in configuredField.ValueTransformations)
            {
                _logger?.LogTrace("Applying transformation '{TransformationDisplayName}' to {FieldIdentifier} for target '{TargetFieldNameForLog}'.", valueTransformation.DisplayName, fieldIdentifier, targetFieldNameForLog);
                try
                {
                    currentStepResult = Task.Run(async () => await valueTransformation.ApplyTransformationAsync(currentStepResult)).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    var stepErrorMessage = $"Error applying transformation {valueTransformation.DisplayName} to {fieldIdentifier}: {ex.Message}";
                    appliedTransformationsLog.Add(stepErrorMessage);
                    _logger?.LogWarning("Error applying transformation {TransformationDisplayName} to {FieldIdentifier} for target '{TargetFieldNameForLog}': {ExceptionMessage}", valueTransformation.DisplayName, fieldIdentifier, targetFieldNameForLog, ex.Message);

                    currentStepResult = TransformationResult.Failure(
                        originalValue: currentStepResult.OriginalValue,
                        targetType: targetType,
                        errorMessage: stepErrorMessage,
                        originalValueType: currentStepResult.OriginalValueType,
                        currentValueType: currentStepResult.CurrentValueType,
                        appliedTransformations: currentStepResult.AppliedTransformations?.Append(stepErrorMessage).Distinct().ToList() ?? [stepErrorMessage],
                        sourceRecordContext: sourceRecord,
                        explicitTargetFieldType: targetType
                    );
                }

                if (currentStepResult.WasFailure)
                {
                    appliedTransformationsLog.Add($"Transformation failed for {fieldIdentifier}. Error: {currentStepResult.ErrorMessage}. Adding null to combination list.");
                    _logger?.LogWarning("Transformation failed for {FieldIdentifier} for target '{TargetFieldNameForLog}'. Error: {ErrorMessage}. Adding null.", fieldIdentifier, targetFieldNameForLog, currentStepResult.ErrorMessage);
                    collectedValuesForCombination.Add(null);
                    goto nextField;
                }
            }
            collectedValuesForCombination.Add(currentStepResult.CurrentValue);
            appliedTransformationsLog.AddRange(currentStepResult.AppliedTransformations?.Except(fieldLog).ToList() ?? Enumerable.Empty<string>());
            appliedTransformationsLog.Add($"Successfully processed {fieldIdentifier}. Final value for combination: '{currentStepResult.CurrentValue ?? "null"}'.");
            _logger?.LogTrace("Successfully processed {FieldIdentifier} for target '{TargetFieldNameForLog}'. Value: '{CurrentValue}'.", fieldIdentifier, targetFieldNameForLog, currentStepResult.CurrentValue ?? "null");

        nextField:;
        }

        var combineTransformation = new CombineFieldsTransformation { TransformationDetail = this.CombinationFormat };
        _logger?.LogTrace("Preparing to combine {NumValues} values for target '{TargetFieldNameForLog}' using format '{CombinationFormat}' (GetValue).", collectedValuesForCombination.Count, targetFieldNameForLog, CombinationFormat);

        TransformationResult inputForCombine = TransformationResult.Success(
            originalValue: collectedValuesForCombination.ToArray(),
            originalValueType: typeof(object[]),
            currentValue: collectedValuesForCombination.ToArray(),
            currentValueType: typeof(object[]),
            appliedTransformations: ["Preparing to combine collected values for GetValue."],
            sourceRecordContext: sourceRecord,
            targetFieldType: targetType
        );

        try
        {
            TransformationResult finalResult = Task.Run(async () => await combineTransformation.ApplyTransformationAsync(inputForCombine)).GetAwaiter().GetResult();
            List<string> combinedLogs = appliedTransformationsLog;
            if (finalResult.AppliedTransformations != null)
            {
                combinedLogs.AddRange(finalResult.AppliedTransformations.Except(inputForCombine.AppliedTransformations ?? []));
            }
            var result = finalResult with { SourceRecordContext = sourceRecord, TargetFieldType = targetType, AppliedTransformations = [.. combinedLogs.Distinct()] };
            _logger?.LogDebug("CombineFieldsRule.GetValue for target '{TargetFieldNameForLog}' completed. Success: {WasSuccess}, Value: '{CurrentValue}'.", targetFieldNameForLog, !result.WasFailure, result.CurrentValue);
            return result;
        }
        catch (Exception ex)
        {
            var errorMessage = $"Failed to combine fields in GetValue for target '{targetFieldNameForLog}': {ex.Message}";
            appliedTransformationsLog.Add(errorMessage);
            _logger?.LogError(ex, "Failed to combine fields in GetValue for target '{TargetFieldNameForLog}'.", targetFieldNameForLog);
            return TransformationResult.Failure(
                originalValue: inputForCombine.OriginalValue,
                targetType: targetType,
                errorMessage: errorMessage,
                appliedTransformations: [.. appliedTransformationsLog.Distinct()],
                sourceRecordContext: sourceRecord,
                explicitTargetFieldType: targetType
            );
        }
    }

    /// <summary>
    /// Creates a clone of the current mapping rule.
    /// The clone includes copies of all configurable properties, such as InputFields and CombinationFormat.
    /// The logger instance is not cloned; the new instance will have a null logger or one provided by its constructor if applicable.
    /// </summary>
    /// <returns>A new instance of <see cref="CombineFieldsRule"/> with the same configuration.</returns>
    public override MappingRuleBase Clone()
    {
        _logger?.LogTrace("Cloning CombineFieldsRule for target '{TargetField}'.", TargetField);
        var clone = new CombineFieldsRule
        {
            InputFields = [.. this.InputFields.Select(f => f.Clone())],
            CombinationFormat = this.CombinationFormat
        };
        this.CloneBaseProperties(clone);
        _logger?.LogDebug("Successfully cloned CombineFieldsRule for target '{TargetField}'.", clone.TargetField);
        return clone;
    }
}
