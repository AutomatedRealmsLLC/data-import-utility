using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AutomatedRealms.DataImportUtility.Abstractions.Enums;
using AutomatedRealms.DataImportUtility.Abstractions.Models;
using AutomatedRealms.DataImportUtility.Abstractions.Interfaces;

namespace AutomatedRealms.DataImportUtility.Core.Rules;

/// <summary>
/// A custom rule that does not necessarily require a direct source field for its primary operation,
/// but can be configured with transformations and conditional logic. Its behavior is primarily
/// driven by its configured transformations.
/// </summary>
public class CustomFieldlessRule : MappingRuleBase
{    /// <summary>
    /// Gets or sets an optional detail or initial value for the rule, which might be used by transformations.
    /// </summary>
    public new string? RuleDetail { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomFieldlessRule"/> class.
    /// </summary>
    public CustomFieldlessRule()
    {
    }

    /// <summary>
    /// Gets the type of the mapping rule.
    /// </summary>
    public override MappingRuleType RuleType => MappingRuleType.CustomFieldlessRule;

    /// <summary>
    /// Gets the enum member name for the mapping rule type.
    /// </summary>
    public override string EnumMemberName => nameof(CustomFieldlessRule);

    /// <summary>
    /// Gets the display name of the mapping rule.
    /// </summary>
    public override string DisplayName => "Custom Fieldless Rule";

    /// <summary>
    /// Gets the short name of the mapping rule.
    /// </summary>
    public override string ShortName => "Custom...";

    /// <summary>
    /// Gets the description of the mapping rule.
    /// </summary>
    public override string Description => "A custom rule that executes logic defined by its transformations and conditions, not necessarily tied to a single source field.";

    /// <summary>
    /// Indicates whether the mapping rule is empty or not configured.
    /// For a CustomFieldlessRule, it's empty if it has no value transformations configured.
    /// </summary>
    public override bool IsEmpty => !SourceFieldTransformations.Any();

    /// <summary>
    /// Gets the <see cref="MappingRuleType"/> enum value for this rule.
    /// </summary>
    /// <returns>The <see cref="MappingRuleType.CustomFieldlessRule"/> enum value.</returns>
    public override MappingRuleType GetEnumValue() => this.RuleType;

    /// <summary>
    /// Gets the order value for the enum member.
    /// </summary>
    public override int EnumMemberOrder => 6;

    /// <summary>
    /// Applies the custom fieldless rule to the provided data row.
    /// The rule's behavior is primarily driven by its configured <see cref="MappingRuleBase.SourceFieldTransformations"/>.
    /// </summary>
    /// <param name="dataRow">The data row to apply the rule to.</param>
    /// <returns>A transformation result, or null if the rule could not be applied.</returns>
    public override async Task<TransformationResult?> Apply(DataRow dataRow)
    {
        var context = TransformationResult.Success(
            originalValue: this.RuleDetail,
            originalValueType: this.RuleDetail?.GetType(),
            currentValue: this.RuleDetail,
            currentValueType: this.RuleDetail?.GetType(),
            appliedTransformations: new List<string>(),
            record: dataRow,
            tableDefinition: null,
            sourceRecordContext: null,
            targetFieldType: null
        );
        return await Apply(context);
    }

    /// <summary>
    /// Applies the custom fieldless rule using the provided transformation context.
    /// Its behavior is primarily driven by its configured <see cref="MappingRuleBase.SourceFieldTransformations"/>.
    /// </summary>
    /// <param name="context">The transformation context.</param>
    /// <returns>A transformation result, or null if the rule could not be applied or if transformations fail.</returns>
    public override async Task<TransformationResult?> Apply(ITransformationContext context)
    {
        TransformationResult currentProcessingResult = TransformationResult.Success(
            originalValue: this.RuleDetail,
            originalValueType: this.RuleDetail?.GetType(),
            currentValue: this.RuleDetail,
            currentValueType: context.TargetFieldType ?? this.RuleDetail?.GetType() ?? typeof(object),
            appliedTransformations: new List<string>(),
            record: context.Record,
            tableDefinition: context.TableDefinition,
            sourceRecordContext: context.SourceRecordContext,
            targetFieldType: context.TargetFieldType
        );

        foreach (var transformation in this.SourceFieldTransformations)
        {
            if (transformation == null) continue;

            currentProcessingResult = await transformation.ApplyTransformationAsync(currentProcessingResult);

            if (currentProcessingResult.WasFailure)
            {
                return currentProcessingResult;
            }
        }
        
        return currentProcessingResult;
    }

    /// <summary>
    /// Applies the custom fieldless rule to all rows in the provided data table.
    /// </summary>
    /// <param name="data">The data table to apply the rule to.</param>
    /// <returns>An enumerable collection of transformation results for each row.</returns>
    public override async Task<IEnumerable<TransformationResult?>> Apply(DataTable data)
    {
        if (data == null) return new List<TransformationResult?>();
        var results = new List<TransformationResult?>();
        foreach (DataRow row in data.Rows)
        {
            var rowContext = TransformationResult.Success(
                originalValue: this.RuleDetail,
                originalValueType: this.RuleDetail?.GetType(),
                currentValue: this.RuleDetail,
                currentValueType: this.RuleDetail?.GetType(),
                appliedTransformations: new List<string>(),
                record: row,
                tableDefinition: null,
                sourceRecordContext: null,
                targetFieldType: null
            );
            results.Add(await Apply(rowContext).ConfigureAwait(false));
        }
        return results;
    }

    /// <summary>
    /// Applies the custom fieldless rule in a context where no specific data table or row is provided.
    /// This implies operating on an initial value (e.g., <see cref="RuleDetail"/>) and transformations.
    /// </summary>
    /// <returns>An enumerable collection of transformation results.</returns>
    public override async Task<IEnumerable<TransformationResult?>> Apply()
    {
        var emptyContext = TransformationResult.Success(
            originalValue: this.RuleDetail,
            originalValueType: this.RuleDetail?.GetType(),
            currentValue: this.RuleDetail,
            currentValueType: this.RuleDetail?.GetType(),
            appliedTransformations: new List<string>(),
            record: null,
            tableDefinition: null,
            sourceRecordContext: null,
            targetFieldType: null
        );
        var result = await Apply(emptyContext);
        return new List<TransformationResult?> { result };
    }

    /// <summary>
    /// Gets the value for the custom fieldless rule based on transformations, potentially using an initial value from <see cref="RuleDetail"/>.
    /// The sourceRecord is not directly used to fetch a field value but might provide context for transformations if they are designed to use it.
    /// </summary>
    /// <param name="sourceRecordContextList">The source record context (largely ignored for direct field access but available for complex transformations).</param>
    /// <param name="targetField">The descriptor of the target field (used to determine target type if needed).</param>
    /// <returns>A <see cref="TransformationResult"/> containing the processed value.</returns>
    public override TransformationResult GetValue(List<ImportedRecordFieldDescriptor> sourceRecordContextList, ImportedRecordFieldDescriptor targetField)
    {
        Type effectiveTargetType = targetField?.FieldType ?? this.RuleDetail?.GetType() ?? typeof(object);

        var contextForApply = TransformationResult.Success(
            originalValue: this.RuleDetail,
            originalValueType: this.RuleDetail?.GetType(),
            currentValue: this.RuleDetail,
            currentValueType: this.RuleDetail?.GetType(),
            appliedTransformations: new List<string>(),
            record: null,
            tableDefinition: null,
            sourceRecordContext: sourceRecordContextList,
            targetFieldType: effectiveTargetType
        );

        TransformationResult? resultFromApply = Apply(contextForApply).ConfigureAwait(false).GetAwaiter().GetResult();

        if (resultFromApply == null || resultFromApply.WasFailure)
        {
            return resultFromApply ?? TransformationResult.Failure(
                originalValue: this.RuleDetail,
                targetType: effectiveTargetType,
                errorMessage: $"Failed to apply CustomFieldlessRule in GetValue. {(resultFromApply?.ErrorMessage ?? "Result was null.")}",
                originalValueType: this.RuleDetail?.GetType(),
                sourceRecordContext: sourceRecordContextList,
                explicitTargetFieldType: effectiveTargetType
            );
        }
        
        if (targetField?.FieldType != null && 
            resultFromApply.CurrentValue != null && 
            resultFromApply.CurrentValueType != targetField.FieldType)
        {
            try
            {
                var convertedValue = Convert.ChangeType(resultFromApply.CurrentValue, targetField.FieldType);
                return resultFromApply with { CurrentValue = convertedValue, CurrentValueType = targetField.FieldType };
            }
            catch (Exception ex)
            {
                return resultFromApply with 
                { 
                    ErrorMessage = (!string.IsNullOrEmpty(resultFromApply.ErrorMessage) ? resultFromApply.ErrorMessage + " " : "") + $"Failed to convert final value to target type '{targetField.FieldType.Name}': {ex.Message}" 
                };
            }
        }

        return resultFromApply;
    }

    /// <summary>
    /// Creates a clone of this <see cref="CustomFieldlessRule"/> instance.
    /// </summary>
    /// <returns>A new <see cref="CustomFieldlessRule"/> instance with the same configuration.</returns>
    public override MappingRuleBase Clone()
    {
        var clone = new CustomFieldlessRule
        {
            RuleDetail = this.RuleDetail
        };
        base.CloneBaseProperties(clone);
        return clone;
    }
}
