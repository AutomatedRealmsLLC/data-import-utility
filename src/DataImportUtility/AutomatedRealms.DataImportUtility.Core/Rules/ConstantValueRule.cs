using System.Data;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System;

using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Models;
using AutomatedRealms.DataImportUtility.Abstractions.Enums;
using AutomatedRealms.DataImportUtility.Core.ComparisonOperations;

namespace AutomatedRealms.DataImportUtility.Core.Rules;

/// <summary>
/// A rule indicating the value should be constant.
/// </summary>
public partial class ConstantValueRule : MappingRuleBase
{
    /// <inheritdoc />
    public override int EnumMemberOrder { get; } = 2;

    /// <inheritdoc />
    public override string EnumMemberName { get; } = nameof(ConstantValueRule);

    /// <inheritdoc />
    [JsonIgnore]
    public override string DisplayName { get; } = "Constant Value";

    /// <inheritdoc />
    [JsonIgnore]
    public override string ShortName => "Constant";

    /// <inheritdoc />
    [JsonIgnore]
    public override string Description { get; } = "Outputs a constant value (from RuleDetail) for each record.";

    /// <inheritdoc />
    [JsonIgnore]
    public override bool IsEmpty => string.IsNullOrWhiteSpace(RuleDetail);

    /// <inheritdoc />
    public override RuleType RuleType => RuleType.ConstantValueRule;

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
                originalValue: this.RuleDetail, // The configured constant value can be seen as the 'original' in this context
                targetType: targetType,
                errorMessage: "Conditional rules not met for ConstantValueRule. Rule not applied.",
                originalValueType: this.RuleDetail?.GetType() ?? typeof(string), // Assuming RuleDetail is string-like if not null
                currentValueType: null,
                appliedTransformations: new[] { "ConstantValueRule conditional check" },
                record: row,
                tableDefinition: this.ParentTableDefinition
            );
        }

        // If conditions are met, the constant value from RuleDetail is used.
        // No further transformations are applied by this rule itself.
        // The targetType is what the caller expects; conversion might happen later if types don't match.
        return TransformationResult.Success(
            originalValue: this.RuleDetail,
            originalValueType: this.RuleDetail?.GetType() ?? typeof(string),
            currentValue: this.RuleDetail,
            currentValueType: this.RuleDetail?.GetType() ?? targetType, // Prefer actual type of RuleDetail, fallback to targetType
            appliedTransformations: new[] { "ConstantValueRule applied." },
            record: row,
            tableDefinition: this.ParentTableDefinition
        );
    }

    /// <inheritdoc />
    public override MappingRuleBase Clone()
    {
        // Base clone handles RuleDetail, ConditionalRules, ParentTableDefinition etc.
        // ConstantValueRule does not have additional specific state that needs cloning.
        var clone = (ConstantValueRule)base.Clone();
        return clone;
    }
}
