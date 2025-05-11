using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Enums;
using AutomatedRealms.DataImportUtility.Abstractions.Models;
using AutomatedRealms.DataImportUtility.Core.ComparisonOperations;

namespace AutomatedRealms.DataImportUtility.Core.Rules;

/// <summary>
/// A custom rule that does not necessarily require a direct source field for its primary operation,
/// but can be configured with transformations and conditional logic. Its behavior is primarily
/// driven by its configured transformations.
/// </summary>
public class CustomFieldlessRule : MappingRuleBase
{
    /// <inheritdoc />
    public override RuleType RuleType => RuleType.CustomFieldlessRule;

    /// <inheritdoc />
    public override string EnumMemberName => nameof(CustomFieldlessRule);

    /// <inheritdoc />
    public override string DisplayName => "Custom Fieldless Rule";

    /// <inheritdoc />
    public override string ShortName => "Custom...";

    /// <inheritdoc />
    public override string Description => "A custom rule that executes logic defined by its transformations and conditions, not necessarily tied to a single source field.";

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomFieldlessRule"/> class.
    /// </summary>
    public CustomFieldlessRule()
    {
        // Constructor can be used for specific initialization if needed.
    }

    /// <inheritdoc />
    /// <remarks>
    /// For a CustomFieldlessRule, "empty" means it has no value transformations configured
    /// across any of its (potentially conceptual) source field transformation entries.
    /// </remarks>
    public override bool IsEmpty => !SourceFieldTransformations.Any(sft => sft.ValueTransformations.Any());

    /// <inheritdoc />
    public override async Task<TransformationResult> GetValue(DataRow row, Type targetType)
    {
        // Check conditional rules first
        bool conditionsMet = await AreConditionalRulesMetAsync(row).ConfigureAwait(false);
        if (!conditionsMet)
        {
            return TransformationResult.Failure(
                originalValue: null, 
                targetType: targetType, 
                errorMessage: "Conditional rules not met for CustomFieldlessRule.", 
                originalValueType: null, 
                record: row, 
                tableDefinition: this.ParentTableDefinition);
        }

        // Initialize a base result. The actual "value" will be determined by transformations.
        // The initial currentValue can be null or based on RuleDetail if that's a desired starting point.
        // For this implementation, we'll start with null and let transformations build the value.
        TransformationResult currentProcessingResult = TransformationResult.Success(
            originalValue: this.RuleDetail, // RuleDetail can serve as an initial piece of data if relevant
            originalValueType: this.RuleDetail?.GetType(),
            currentValue: this.RuleDetail, // Start with RuleDetail, transformations can override
            currentValueType: this.RuleDetail?.GetType(),
            appliedTransformations: new List<string>(),
            record: row,
            tableDefinition: this.ParentTableDefinition
        );

        // Apply transformations. These define the "action" of the fieldless rule.
        // They might operate on the row context, the initial RuleDetail, or produce a new value.
        foreach (var fieldTransformation in this.SourceFieldTransformations)
        {
            // Even if "fieldless", FieldTransformation can hold a collection of ValueTransformations.
            // The "Field" property within FieldTransformation might be null or a placeholder.
            if (fieldTransformation.ValueTransformations != null)
            {
                foreach (var transformation in fieldTransformation.ValueTransformations)
                {
                    if (transformation == null) continue;

                    currentProcessingResult = await transformation.ApplyTransformationAsync(currentProcessingResult);

                    if (currentProcessingResult.WasFailure)
                    {
                        // Ensure tableDefinition is propagated from the failed transformation result
                        return currentProcessingResult; 
                    }
                }
            }
        }
        
        object? valueAfterTransformations = currentProcessingResult.CurrentValue;
        Type? typeAfterTransformations = currentProcessingResult.CurrentValueType;

        try
        {
            object? finalValue = valueAfterTransformations;
            Type? finalValueType = typeAfterTransformations;

            if (targetType != null) // Only attempt conversion if a targetType is specified
            {
                if (valueAfterTransformations == null)
                {
                    if (targetType.IsValueType && Nullable.GetUnderlyingType(targetType) == null)
                    {
                        return TransformationResult.Failure(
                            originalValue: currentProcessingResult.OriginalValue, targetType: targetType,
                            errorMessage: $"CustomFieldlessRule: Cannot assign null to non-nullable target type '{targetType.Name}'.",
                            originalValueType: currentProcessingResult.OriginalValueType, currentValueType: typeAfterTransformations,
                            appliedTransformations: currentProcessingResult.AppliedTransformations, record: row, tableDefinition: this.ParentTableDefinition);
                    }
                    finalValueType = targetType; 
                }
                else if (typeAfterTransformations == null || !targetType.IsAssignableFrom(typeAfterTransformations))
                {
                    finalValue = Convert.ChangeType(valueAfterTransformations, targetType);
                    finalValueType = targetType;
                }
            }
            
            return TransformationResult.Success(
                originalValue: currentProcessingResult.OriginalValue,
                originalValueType: currentProcessingResult.OriginalValueType,
                currentValue: finalValue,
                currentValueType: finalValueType,
                appliedTransformations: currentProcessingResult.AppliedTransformations,
                record: row,
                tableDefinition: this.ParentTableDefinition
            );
        }
        catch (Exception ex)
        {
            return TransformationResult.Failure(
                originalValue: currentProcessingResult.OriginalValue, targetType: targetType,
                errorMessage: $"CustomFieldlessRule: Failed to convert/process value for target type '{targetType?.Name ?? "unknown"}': {ex.Message}",
                originalValueType: currentProcessingResult.OriginalValueType, currentValueType: typeAfterTransformations,
                appliedTransformations: currentProcessingResult.AppliedTransformations, record: row, tableDefinition: this.ParentTableDefinition);
        }
    }

    /// <inheritdoc />
    protected override async Task<ComparisonOperationBase?> GetConfiguredOperationAsync(ConditionalRule conditionalRule, DataRow row)
    {
        if (conditionalRule == null)
        {
            return null;
        }
        // Use the factory to create the operation, providing context.
        // The context identifier helps in logging/debugging from the factory.
        return await ComparisonOperationFactory.CreateOperationAsync(conditionalRule, row, this.ParentTableDefinition, $"ConditionalRule_for_CustomFieldlessRule_{this.Id}");
    }

    // No custom Clone() needed if base.Clone() and base.CloneFields() are sufficient.
    // RuleDetail (string) is handled by MemberwiseClone.
    // SourceFieldTransformations and ConditionalRules are handled by base.Clone().
}
