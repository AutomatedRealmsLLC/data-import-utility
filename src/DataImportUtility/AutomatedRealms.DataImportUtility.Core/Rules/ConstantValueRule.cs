using System.Data;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System;
using System.Collections.Generic; // Added for IEnumerable
using System.Linq; // Added for AsEnumerable

using AutomatedRealms.DataImportUtility.Abstractions.Models;
using AutomatedRealms.DataImportUtility.Abstractions.Enums;
using AutomatedRealms.DataImportUtility.Abstractions.Interfaces;

namespace AutomatedRealms.DataImportUtility.Core.Rules;

/// <summary>
/// A rule indicating the value should be constant.
/// </summary>
public partial class ConstantValueRule : MappingRuleBase
{    /// <summary>
    /// Gets or sets the constant value for this rule.
    /// </summary>
    public new string? RuleDetail { get; set; }

    /// <summary>
    /// Gets the order value for the enum member.
    /// </summary>
    public override int EnumMemberOrder => 5;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConstantValueRule"/> class.
    /// </summary>
    public ConstantValueRule() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConstantValueRule"/> class with a specific constant value.
    /// </summary>
    /// <param name="constantValue">The constant value for this rule.</param>
    public ConstantValueRule(string? constantValue)
    {
        this.RuleDetail = constantValue;
    }

    /// <summary>
    /// Gets the enum member name for the mapping rule type.
    /// </summary>
    public override string EnumMemberName { get; } = nameof(ConstantValueRule);

    /// <summary>
    /// Gets the display name of the mapping rule.
    /// </summary>
    [JsonIgnore]
    public override string DisplayName { get; } = "Constant Value";

    /// <summary>
    /// Gets the short name of the mapping rule.
    /// </summary>
    [JsonIgnore]
    public override string ShortName => "Constant";

    /// <summary>
    /// Gets the description of the mapping rule.
    /// </summary>
    [JsonIgnore]
    public override string Description { get; } = "Outputs a constant value (from RuleDetail) for each record.";

    /// <summary>
    /// Indicates whether the mapping rule is empty or not configured.
    /// For a ConstantValueRule, it's empty if RuleDetail is not set.
    /// </summary>
    [JsonIgnore]
    public override bool IsEmpty => string.IsNullOrWhiteSpace(RuleDetail);

    /// <summary>
    /// Gets the type of the mapping rule.
    /// </summary>
    public override MappingRuleType RuleType => MappingRuleType.ConstantValueRule;

    /// <summary>
    /// Gets the <see cref="MappingRuleType"/> enum value for this rule.
    /// </summary>
    /// <returns>The <see cref="MappingRuleType.ConstantValueRule"/> enum value.</returns>
    public override MappingRuleType GetEnumValue() => this.RuleType;

    /// <summary>
    /// Applies the constant value rule to the provided data row.
    /// </summary>
    /// <param name="dataRow">The data row to apply the rule to.</param>
    /// <returns>A transformation result containing the constant value.</returns>
    public override async Task<TransformationResult?> Apply(DataRow dataRow)
    {
        // This overload should ideally use the ITransformationContext based Apply method.
        // For now, we'll create a basic context. A more complete context might be needed.
        var context = TransformationResult.Success(
            originalValue: null, // No specific original value from a single DataRow context
            originalValueType: typeof(object),
            currentValue: null, // Will be set by the rule
            currentValueType: typeof(object),
            appliedTransformations: new List<string>(),
            record: dataRow,
            tableDefinition: null, // Or resolve from ParentTableDefinition if available and relevant
            sourceRecordContext: null, // Or resolve if available
            targetFieldType: RuleDetail?.GetType() ?? typeof(string) // Assuming target type is string or type of RuleDetail
        );
        return await Apply(context);
    }

    /// <summary>
    /// Applies the constant value rule using the provided transformation context.
    /// </summary>
    /// <param name="context">The transformation context.</param>
    /// <returns>A transformation result containing the constant value.</returns>
    public override async Task<TransformationResult?> Apply(ITransformationContext context)
    {
        await Task.CompletedTask; // Simulate async work if any were needed.

        Type valueType = context.TargetFieldType ?? RuleDetail?.GetType() ?? typeof(string);

        return TransformationResult.Success(
            originalValue: this.RuleDetail, // For a constant rule, original and current are the same.
            originalValueType: RuleDetail?.GetType() ?? typeof(string),
            currentValue: this.RuleDetail,
            currentValueType: valueType,
            appliedTransformations: new[] { "ConstantValueRule applied." },
            record: context.Record,
            tableDefinition: context.TableDefinition,
            sourceRecordContext: context.SourceRecordContext,
            targetFieldType: context.TargetFieldType
        );
    }

    /// <summary>
    /// Applies the constant value rule to all rows in the provided data table.
    /// </summary>
    /// <param name="data">The data table to apply the rule to.</param>
    /// <returns>An enumerable collection of transformation results for each row.</returns>
    public override async Task<IEnumerable<TransformationResult?>> Apply(DataTable data)
    {
        if (data == null) return new List<TransformationResult?>();
        var results = new List<TransformationResult?>();
        foreach (DataRow row in data.Rows)
        {
            // Create a context for each row
            var context = TransformationResult.Success(
                originalValue: null,
                originalValueType: typeof(object),
                currentValue: null,
                currentValueType: typeof(object),
                appliedTransformations: new List<string>(),
                record: row,
                tableDefinition: null, // Or resolve from ParentTableDefinition
                sourceRecordContext: null, // Or resolve if available
                targetFieldType: RuleDetail?.GetType() ?? typeof(string)
            );
            results.Add(await Apply(context).ConfigureAwait(false));
        }
        return results;
    }

    /// <summary>
    /// Applies the constant value rule in a context where no specific data table or row is provided.
    /// </summary>
    /// <returns>An enumerable collection containing a single transformation result with the constant value.</returns>
    public override async Task<IEnumerable<TransformationResult?>> Apply()
    {
        // Create a basic context.
        var context = TransformationResult.Success(
            originalValue: null,
            originalValueType: typeof(object),
            currentValue: null,
            currentValueType: typeof(object),
            appliedTransformations: new List<string>(),
            record: null,
            tableDefinition: null,
            sourceRecordContext: null,
            targetFieldType: RuleDetail?.GetType() ?? typeof(string)
        );
        var result = await Apply(context);
        return new List<TransformationResult?> { result }.AsEnumerable();
    }

    /// <summary>
    /// Gets the constant value, irrespective of the source record.
    /// </summary>
    /// <param name="sourceRecordContext">The source record (ignored for this rule).</param>
    /// <param name="targetField">The descriptor of the target field (used to determine target type if needed).</param>
    /// <returns>A <see cref="TransformationResult"/> containing the constant value.</returns>
    public override TransformationResult GetValue(List<ImportedRecordFieldDescriptor> sourceRecordContext, ImportedRecordFieldDescriptor targetField)
    {
        Type targetType = targetField?.FieldType ?? RuleDetail?.GetType() ?? typeof(string);
        // Create a context for GetValue.
        var context = TransformationResult.Success(
            originalValue: this.RuleDetail,
            originalValueType: RuleDetail?.GetType() ?? typeof(string),
            currentValue: this.RuleDetail,
            currentValueType: targetType,
            appliedTransformations: new List<string>(),
            record: null, 
            tableDefinition: null, 
            sourceRecordContext: sourceRecordContext,
            targetFieldType: targetType
        );

        var task = Apply(context);
        return task.ConfigureAwait(false).GetAwaiter().GetResult()
            ?? TransformationResult.Failure(
                    originalValue: this.RuleDetail,
                    targetType: targetType, // Positional argument for target type context
                    errorMessage: "Failed to apply ConstantValueRule in GetValue.",
                    originalValueType: RuleDetail?.GetType() ?? typeof(string),
                    sourceRecordContext: sourceRecordContext,
                    explicitTargetFieldType: targetType // Named argument for clarity and to match definition
                );
    }

    /// <summary>
    /// Creates a clone of this <see cref="ConstantValueRule"/> instance.
    /// </summary>
    /// <returns>A new <see cref="ConstantValueRule"/> instance with the same configuration.</returns>
    public override MappingRuleBase Clone()
    {
        var clone = new ConstantValueRule
        {
            RuleDetail = this.RuleDetail // Clone specific properties
        };
        base.CloneBaseProperties(clone); // Clone base properties
        return clone;
    }
}
