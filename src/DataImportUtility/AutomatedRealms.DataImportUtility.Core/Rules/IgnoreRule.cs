using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Enums;
using System.Data;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AbstractionsModels = AutomatedRealms.DataImportUtility.Abstractions.Models;
using System;

namespace AutomatedRealms.DataImportUtility.Core.Rules;

/// <summary>
/// A rule indicating the field should be ignored and not output to the destination.
/// </summary>
public class IgnoreRule : MappingRuleBase
{
    /// <inheritdoc />
    public override RuleType RuleType => RuleType.IgnoreRule;

    /// <inheritdoc />
    public override string EnumMemberName => nameof(IgnoreRule);

    /// <inheritdoc />
    [JsonIgnore]
    public override string DisplayName { get; } = "Ignore";

    /// <inheritdoc />
    [JsonIgnore]
    public override string ShortName => "Ignore";

    /// <inheritdoc />
    [JsonIgnore]
    public override string Description { get; } = "Do not output this field to the destination.";

    /// <inheritdoc />
    [JsonIgnore]
    public override bool IsEmpty => false; // An ignore rule is never considered empty in terms of configuration.

    // No MaxSourceFields override, defaults to 0, which is appropriate as IgnoreRule doesn't use source fields.

    /// <inheritdoc />
    public override async Task<AbstractionsModels.TransformationResult> GetValue(DataRow row, Type targetType)
    {
        // Check conditional rules first. If they are not met, this rule might not apply,
        // and a subsequent rule in a sequence (if any) might take over.
        // However, for an IgnoreRule, if its conditions are met, it *should* be ignored.
        // If conditions are NOT met, it implies the field is NOT ignored by this rule instance.
        // This might be counter-intuitive for an IgnoreRule. Typically, an IgnoreRule is unconditional
        // or its conditions define *when* to ignore.
        if (!await AreConditionalRulesMetAsync(row))
        {
            // If conditions for *this specific ignore rule* are not met, it means this rule doesn't apply.
            // This is a special case. We return a failure indicating this rule instance did not apply,
            // allowing a potential fallback or default behavior in the mapping process.
            // It's not a failure of the *data*, but a failure of *this rule to apply*.
            return AbstractionsModels.TransformationResult.Failure(
                originalValue: null,
                targetType: targetType,
                errorMessage: "Conditional rules for IgnoreRule not met. Rule not applied.",
                originalValueType: null,
                currentValue: null,
                currentValueType: null,
                appliedTransformations: new[] { "IgnoreRule conditional check" },
                record: row,
                tableDefinition: this.ParentTableDefinition
            );
        }

        // If conditions are met (or no conditions), the field is actively ignored.
        // The original IgnoreRuleExtensions.IgnoreField set result.Value = null.
        // We create a success result but with CurrentValue as null and a message indicating it's ignored.
        return AbstractionsModels.TransformationResult.Success(
            originalValue: null, // Original value is not relevant for ignore
            originalValueType: null, // Original type not relevant
            currentValue: null, // Current value is null because it's ignored
            currentValueType: targetType, // Current type is the target type, though value is null
            appliedTransformations: new[] { "Field ignored by IgnoreRule." },
            record: row,
            tableDefinition: this.ParentTableDefinition
        );
    }

    /// <inheritdoc />
    public override MappingRuleBase Clone()
    {
        var clone = new IgnoreRule();
        base.CloneFields(clone);
        // Properties like DisplayName, ShortName, Description are get-only and don't need special cloning.
        return clone;
    }
}
