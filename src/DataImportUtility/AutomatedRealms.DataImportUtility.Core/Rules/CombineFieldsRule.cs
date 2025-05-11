using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System;

using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Enums;
using AutomatedRealms.DataImportUtility.Abstractions.Models;
using AutomatedRealms.DataImportUtility.Core.ValueTransformations;
using AutomatedRealms.DataImportUtility.Core.ComparisonOperations; // Added for ComparisonOperationFactory

namespace AutomatedRealms.DataImportUtility.Core.Rules;

/// <summary>
/// A rule indicating the value should be combined from multiple source fields.
/// </summary>
public class CombineFieldsRule : MappingRuleBase
{
    /// <inheritdoc />
    public override int EnumMemberOrder { get; } = 3;

    /// <inheritdoc />
    public override string EnumMemberName { get; } = nameof(CombineFieldsRule);

    /// <inheritdoc />
    public override string DisplayName { get; } = "Combine Fields";

    /// <inheritdoc />
    public override string ShortName => "Combine";

    /// <inheritdoc />
    [JsonIgnore]
    public override string Description { get; } = "Combine the values of the source fields into the output field using a format string in RuleDetail.";

    /// <inheritdoc />
    [JsonIgnore]
    public override bool IsEmpty => !SourceFieldTransformations.Any(x => x.Field?.FieldName != null && !string.IsNullOrWhiteSpace(x.Field.FieldName));

    /// <inheritdoc />
    public override RuleType RuleType => RuleType.CombineFieldsRule;

    /// <inheritdoc />
    protected override Task<ComparisonOperationBase?> GetConfiguredOperationAsync(ConditionalRule conditionalRule, DataRow row)
    {
        return ComparisonOperationFactory.CreateOperationAsync(conditionalRule, row);
    }

    /// <inheritdoc />
    public override async Task<TransformationResult> GetValue(DataRow row, Type targetType)
    {
        if (!await AreConditionalRulesMetAsync(row))
        {
            return TransformationResult.Failure(
                originalValue: null,
                targetType: targetType,
                errorMessage: "Conditional rules for CombineFieldsRule not met. Rule not applied.",
                originalValueType: null,
                currentValueType: null,
                appliedTransformations: new[] { "CombineFieldsRule conditional check" },
                record: row,
                tableDefinition: this.ParentTableDefinition
            );
        }

        var collectedValuesForCombination = new List<object?>();
        var appliedTransformationsLog = new List<string> { "CombineFieldsRule started." };

        foreach (var fieldTransformation in SourceFieldTransformations)
        {
            if (fieldTransformation.Field?.FieldName == null || !row.Table.Columns.Contains(fieldTransformation.Field.FieldName))
            {
                collectedValuesForCombination.Add(null);
                appliedTransformationsLog.Add($"Source field '{fieldTransformation.Field?.FieldName ?? "N/A"}' not found or not specified, using null.");
                continue;
            }

            object? initialValue = row[fieldTransformation.Field.FieldName];
            Type? initialValueType = initialValue?.GetType();
            
            TransformationResult currentStepResult = TransformationResult.Success(
                originalValue: initialValue,
                originalValueType: initialValueType,
                currentValue: initialValue,
                currentValueType: initialValueType,
                appliedTransformations: new List<string> { $"Initial value from '{fieldTransformation.Field.FieldName}'." },
                record: row,
                tableDefinition: this.ParentTableDefinition
            );

            foreach (var valueTransformation in fieldTransformation.ValueTransformations)
            {
                currentStepResult = await valueTransformation.ApplyTransformationAsync(currentStepResult);
                if (currentStepResult.WasFailure)
                {
                    appliedTransformationsLog.Add($"Failure during value transformation for field '{fieldTransformation.Field.FieldName}': {currentStepResult.ErrorMessage}");
                    return TransformationResult.Failure(
                        originalValue: currentStepResult.OriginalValue,
                        targetType: targetType,
                        errorMessage: $"Failed to transform source field '{fieldTransformation.Field.FieldName}' for combination: {currentStepResult.ErrorMessage}",
                        originalValueType: currentStepResult.OriginalValueType,
                        currentValueType: null,
                        appliedTransformations: appliedTransformationsLog,
                        record: row,
                        tableDefinition: this.ParentTableDefinition
                    );
                }
            }
            collectedValuesForCombination.Add(currentStepResult.CurrentValue);
            appliedTransformationsLog.Add($"Successfully transformed field '{fieldTransformation.Field.FieldName}'. Value: '{currentStepResult.CurrentValue ?? "null"}'.");
        }
        
        var combineTransformation = new CombineFieldsTransformation
        {
            TransformationDetail = this.RuleDetail
        };

        TransformationResult inputForCombine = TransformationResult.Success(
            originalValue: collectedValuesForCombination.ToArray(),
            originalValueType: typeof(object[]),
            currentValue: collectedValuesForCombination.ToArray(),
            currentValueType: typeof(object[]),
            appliedTransformations: appliedTransformationsLog,
            record: row,
            tableDefinition: this.ParentTableDefinition
        );
        
        return await combineTransformation.ApplyTransformationAsync(inputForCombine);
    }

    /// <inheritdoc />
    public override MappingRuleBase Clone()
    {
        var clone = (CombineFieldsRule)base.Clone();
        return clone;
    }
}
