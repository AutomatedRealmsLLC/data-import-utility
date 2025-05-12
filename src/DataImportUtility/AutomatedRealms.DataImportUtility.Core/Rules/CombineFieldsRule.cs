using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System;

using AutomatedRealms.DataImportUtility.Abstractions; 
using AutomatedRealms.DataImportUtility.Abstractions.Enums;
using AutomatedRealms.DataImportUtility.Abstractions.Interfaces;
using AutomatedRealms.DataImportUtility.Abstractions.Models;
using AutomatedRealms.DataImportUtility.Core.ValueTransformations;

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
    /// Gets or sets the list of value transformations to apply to this field before combining.
    /// </summary>
    public List<ValueTransformationBase> ValueTransformations { get; set; } = new();

    /// <summary>
    /// Clones this configured input field.
    /// </summary>
    /// <returns>A new instance of <see cref="ConfiguredInputField"/> with copied values.</returns>
    public ConfiguredInputField Clone()
    {
        var clone = new ConfiguredInputField
        {
            FieldName = this.FieldName,
            ValueTransformations = this.ValueTransformations.Select(t => t.Clone()).ToList()
        };
        return clone;
    }
}

/// <summary>
/// A rule indicating the value should be combined from multiple source fields using a format string.
/// </summary>
public class CombineFieldsRule : MappingRuleBase
{
    /// <summary>
    /// Gets or sets the list of input fields to be combined. Each field can have its own transformations.
    /// </summary>
    public List<ConfiguredInputField> InputFields { get; set; } = new();

    /// <summary>
    /// Gets or sets the format string used to combine the field values (e.g., "{0} - {1}").
    /// The placeholders correspond to the order of fields in <see cref="InputFields"/>.
    /// </summary>
    public string? CombinationFormat { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CombineFieldsRule"/> class.
    /// </summary>
    public CombineFieldsRule() { }

    /// <summary>
    /// Gets the type of the mapping rule.
    /// </summary>
    public override MappingRuleType RuleType => MappingRuleType.CombineFieldsRule;

    /// <summary>
    /// Gets the enum member name for the mapping rule type.
    /// </summary>
    public override string EnumMemberName => nameof(CombineFieldsRule);

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
    /// Gets the <see cref="MappingRuleType"/> enum value for this rule.
    /// </summary>
    /// <returns>The <see cref="MappingRuleType.CombineFieldsRule"/> enum value.</returns>
    public override MappingRuleType GetEnumValue() => this.RuleType;

    /// <summary>
    /// Gets the order value for the enum member.
    /// </summary>
    public override int EnumMemberOrder => 4;

    /// <inheritdoc />
    public override async Task<TransformationResult?> Apply(ITransformationContext context)
    {
        if (IsEmpty)
        {
            return TransformationResult.Failure(null, context.TargetFieldType ?? typeof(string), "CombineFieldsRule is not configured: InputFields or CombinationFormat is missing.", record: context.Record, sourceRecordContext: context.SourceRecordContext, explicitTargetFieldType: context.TargetFieldType ?? typeof(string));
        }

        DataRow? dataRow = context.Record;
        if (dataRow == null && InputFields.Any(cf => !string.IsNullOrEmpty(cf.FieldName))) 
        {
            return TransformationResult.Failure(null, context.TargetFieldType ?? typeof(string), "CombineFieldsRule requires a DataRow in the context when FieldNames are specified.", record: null, sourceRecordContext: context.SourceRecordContext, explicitTargetFieldType: context.TargetFieldType ?? typeof(string));
        }

        var collectedValuesForCombination = new List<object?>();
        var appliedTransformationsLog = new List<string> { "CombineFieldsRule.Apply(ITransformationContext) started." };

        foreach (var configuredField in InputFields)
        {
            object? initialValue = null;
            Type? initialValueType = null;
            List<string> fieldLog = new();

            if (!string.IsNullOrEmpty(configuredField.FieldName))
            {
                if (dataRow == null || !dataRow.Table.Columns.Contains(configuredField.FieldName))
                {
                    collectedValuesForCombination.Add(null);
                    fieldLog.Add($"Source field '{configuredField.FieldName ?? "N/A"}' not found or DataRow not available, using null.");
                    appliedTransformationsLog.AddRange(fieldLog);
                    continue;
                }
                initialValue = dataRow[configuredField.FieldName];
                initialValueType = initialValue?.GetType();
                fieldLog.Add($"Initial value from '{configuredField.FieldName}': '{initialValue ?? "null"}'.");
            }
            else
            {
                fieldLog.Add("Configured field has no FieldName, initial value is null.");
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
                currentStepResult = await valueTransformation.ApplyTransformationAsync(currentStepResult); 
                if (currentStepResult.WasFailure)
                {
                    appliedTransformationsLog.Add($"Failure during value transformation for field '{configuredField.FieldName ?? "(no field name)"}': {currentStepResult.ErrorMessage}");
                    return TransformationResult.Failure(
                        originalValue: currentStepResult.OriginalValue,
                        targetType: context.TargetFieldType ?? typeof(string), 
                        errorMessage: $"Failed to transform source field '{configuredField.FieldName ?? "(no field name)"}' for combination: {currentStepResult.ErrorMessage}",
                        originalValueType: currentStepResult.OriginalValueType,
                        record: dataRow,
                        appliedTransformations: appliedTransformationsLog,
                        sourceRecordContext: context.SourceRecordContext,
                        explicitTargetFieldType: context.TargetFieldType ?? typeof(string) 
                    );
                }
            }
            collectedValuesForCombination.Add(currentStepResult.CurrentValue);
            appliedTransformationsLog.AddRange(currentStepResult.AppliedTransformations ?? Enumerable.Empty<string>());
            appliedTransformationsLog.Add($"Successfully transformed field '{configuredField.FieldName ?? "(no field name)"}'. Final value for combination: '{currentStepResult.CurrentValue ?? "null"}'.");
        }
        
        var combineTransformation = new CombineFieldsTransformation
        {
            TransformationDetail = this.CombinationFormat 
        };

        TransformationResult inputForCombine = TransformationResult.Success(
            originalValue: collectedValuesForCombination.ToArray(),
            originalValueType: typeof(object[]),
            currentValue: collectedValuesForCombination.ToArray(),
            currentValueType: typeof(object[]),
            appliedTransformations: new List<string> { "Preparing to combine collected values." },
            record: dataRow, 
            sourceRecordContext: context.SourceRecordContext, 
            targetFieldType: typeof(string) 
        );
        
        TransformationResult finalResult = await combineTransformation.ApplyTransformationAsync(inputForCombine);
        
        List<string> combinedLogs = appliedTransformationsLog;
        if (finalResult.AppliedTransformations != null)
        {
            combinedLogs.AddRange(finalResult.AppliedTransformations);
        }

        return finalResult with 
        {
            Record = dataRow, 
            SourceRecordContext = context.SourceRecordContext, 
            TargetFieldType = context.TargetFieldType ?? typeof(string),
            AppliedTransformations = combinedLogs
        };
    }

    /// <summary>
    /// Applies the combine fields rule to the provided data row.
    /// This overload creates an <see cref="ITransformationContext"/> from the DataRow and calls the context-based Apply.
    /// </summary>
    /// <param name="dataRow">The data row to apply the rule to.</param>
    /// <returns>A transformation result containing the combined value, or null/failure if issues occur.</returns>
    public override async Task<TransformationResult?> Apply(DataRow dataRow)
    {
        var context = TransformationResult.Success(null, null, null, null, record: dataRow, targetFieldType: typeof(string));
        return await Apply(context);
    }

    /// <summary>
    /// Applies the combine fields rule to all rows in the provided data table.
    /// This overload iterates through rows, creating a context for each and calling the DataRow-based Apply.
    /// </summary>
    /// <param name="data">The data table to apply the rule to.</param>
    /// <returns>An enumerable collection of transformation results for each row.</returns>
    public override async Task<IEnumerable<TransformationResult?>> Apply(DataTable data)
    {
        if (data == null) return new List<TransformationResult?>();
        var results = new List<TransformationResult?>();
        foreach (DataRow row in data.Rows)
        {
            results.Add(await Apply(row).ConfigureAwait(false)); 
        }
        return results;
    }

    /// <summary>
    /// Applies the combine fields rule in a context where no specific data table or row is provided.
    /// If any <see cref="ConfiguredInputField"/> specifies a <see cref="ConfiguredInputField.FieldName"/>,
    /// this method will return a failure, as DataRow context is required.
    /// Otherwise, it proceeds with a null DataRow context.
    /// </summary>
    /// <returns>An enumerable collection containing a single transformation result (success or failure).</returns>
    public override async Task<IEnumerable<TransformationResult?>> Apply()
    {
        ITransformationContext context;
        if (InputFields.Any(cf => !string.IsNullOrEmpty(cf.FieldName)))
        {
            var failureResult = TransformationResult.Failure(null, typeof(string), "CombineFieldsRule cannot be applied without a DataRow context when FieldNames are specified.", explicitTargetFieldType: typeof(string));
            return new List<TransformationResult?> { failureResult };
        }
        context = TransformationResult.Success(null, null, null, null, targetFieldType: typeof(string));
        var result = await Apply(context);
        return new List<TransformationResult?> { result };
    }

    /// <summary>
    /// Gets the combined value from a source record represented by a list of field descriptors.
    /// This method is intended for scenarios where a DataRow is not available, and data is provided as <see cref="ImportedRecordFieldDescriptor"/> list.
    /// </summary>
    /// <param name="sourceRecord">The source record, as a list of <see cref="ImportedRecordFieldDescriptor"/>.</param>
    /// <param name="targetField">The descriptor of the target field (used to determine target type if needed).</param>
    /// <returns>A <see cref="TransformationResult"/> containing the combined value.</returns>
    public override TransformationResult GetValue(List<ImportedRecordFieldDescriptor> sourceRecord, ImportedRecordFieldDescriptor targetField)
    {
        Type targetType = targetField?.FieldType ?? typeof(string);
        if (IsEmpty)
        {
            return TransformationResult.Failure(null, targetType, "CombineFieldsRule is not configured.", sourceRecordContext: sourceRecord, explicitTargetFieldType: targetType);
        }

        var collectedValuesForCombination = new List<object?>();
        var appliedTransformationsLog = new List<string> { "CombineFieldsRule.GetValue started." };

        foreach (var configuredField in InputFields)
        {
            var fieldDesc = sourceRecord?.FirstOrDefault(f => f.FieldName == configuredField.FieldName);
            object? initialValue = null;
            Type? initialValueType = null;
            List<string> fieldLog = new();

            if (fieldDesc == null)
            {
                if (!string.IsNullOrEmpty(configuredField.FieldName))
                {
                     fieldLog.Add($"Source field '{configuredField.FieldName}' not found in sourceRecord, using null.");
                }
                else
                {
                    fieldLog.Add("Configured field has no FieldName, initial value is null.");
                }
                collectedValuesForCombination.Add(null);
                appliedTransformationsLog.AddRange(fieldLog);
                continue;
            }
            else
            {
                initialValue = fieldDesc.ValueSet?.FirstOrDefault();
                initialValueType = initialValue?.GetType() ?? fieldDesc.FieldType;
                fieldLog.Add($"Initial value from '{configuredField.FieldName}': '{initialValue ?? "null"}'.");
            }

            TransformationResult currentStepResult = TransformationResult.Success(
                initialValue, initialValueType, initialValue, initialValueType,
                fieldLog,
                sourceRecordContext: sourceRecord,
                targetFieldType: null
            );
            
            foreach (var valueTransformation in configuredField.ValueTransformations)
            {
                try
                {
                    currentStepResult = Task.Run(async () => await valueTransformation.ApplyTransformationAsync(currentStepResult)).GetAwaiter().GetResult();
                } 
                catch (Exception ex)
                {
                    currentStepResult = currentStepResult with { ErrorMessage = $"Error applying transformation {valueTransformation.DisplayName}: {ex.Message}"};
                }

                if (currentStepResult.WasFailure)
                {
                    appliedTransformationsLog.Add($"Failure during value transformation for field '{configuredField.FieldName}' in GetValue: {currentStepResult.ErrorMessage}");
                    collectedValuesForCombination.Add(null);
                    goto nextField;
                }
            }
            collectedValuesForCombination.Add(currentStepResult.CurrentValue);
            appliedTransformationsLog.AddRange(currentStepResult.AppliedTransformations ?? Enumerable.Empty<string>());
            appliedTransformationsLog.Add($"Successfully transformed field '{configuredField.FieldName}' in GetValue. Final value for combination: '{currentStepResult.CurrentValue ?? "null"}'.");
            
            nextField:;
        }

        var combineTransformation = new CombineFieldsTransformation { TransformationDetail = this.CombinationFormat };
        TransformationResult inputForCombine = TransformationResult.Success(
            collectedValuesForCombination.ToArray(), typeof(object[]),
            collectedValuesForCombination.ToArray(), typeof(object[]),
            new List<string> { "Preparing to combine collected values for GetValue." },
            sourceRecordContext: sourceRecord,
            targetFieldType: targetType
        );
        
        try
        {
            TransformationResult finalResult = Task.Run(async () => await combineTransformation.ApplyTransformationAsync(inputForCombine)).GetAwaiter().GetResult();
            List<string> combinedLogs = appliedTransformationsLog;
            if (finalResult.AppliedTransformations != null)
            {
                combinedLogs.AddRange(finalResult.AppliedTransformations);
            }
            return finalResult with { SourceRecordContext = sourceRecord, TargetFieldType = targetType, AppliedTransformations = combinedLogs };
        }
        catch (Exception ex)
        {
            return TransformationResult.Failure(
                inputForCombine.OriginalValue, 
                targetType, 
                $"Failed to combine fields in GetValue: {ex.Message}",
                appliedTransformations: appliedTransformationsLog,
                sourceRecordContext: sourceRecord,
                explicitTargetFieldType: targetType
            );
        }
    }

    /// <summary>
    /// Creates a clone of this <see cref="CombineFieldsRule"/> instance.
    /// This performs a deep clone of the <see cref="InputFields"/> list and their <see cref="ValueTransformationBase"/> lists.
    /// </summary>
    /// <returns>A new <see cref="CombineFieldsRule"/> instance with the same configuration.</returns>
    public override MappingRuleBase Clone()
    {
        var clone = new CombineFieldsRule
        {
            CombinationFormat = this.CombinationFormat,
            InputFields = this.InputFields.Select(f => f.Clone()).ToList()
        };
        base.CloneBaseProperties(clone);
        return clone;
    }
}
