using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System;
using AutomatedRealms.DataImportUtility.Abstractions;
using AbstractionsModels = AutomatedRealms.DataImportUtility.Abstractions.Models;
using System.Data; // Required for DataRow

namespace AutomatedRealms.DataImportUtility.Core.ValueTransformations;

/// <summary>
/// Represents a transformation operation that applies a transformation based on a condition
/// </summary>
public partial class ConditionalTransformation : ValueTransformationBase
{
    /// <inheritdoc />
    public override int EnumMemberOrder { get; } = 3;

    /// <inheritdoc />
    public override string EnumMemberName => nameof(ConditionalTransformation);

    /// <inheritdoc />
    [JsonIgnore]
    public override string DisplayName => "Conditional";

    /// <inheritdoc />
    [JsonIgnore]
    public override string ShortName => "If/Else";

    /// <inheritdoc />
    [JsonIgnore]
    public override string Description => "Evaluate a condition and apply a transformation based on the result";

    /// <summary>
    /// The error message when any required component is missing
    /// </summary>
    public const string MissingComponentMessage = "The conditional transformation requires a comparison operation, and both true and false mapping rules.";

    /// <summary>
    /// The error message when the input DataRow/Record is missing, which is required by sub-rules.
    /// </summary>
    public const string MissingRecordMessage = "The conditional transformation cannot be applied because the input data (DataRow/Record) is missing.";

    /// <summary>
    /// The comparison operation to evaluate
    /// </summary>
    [JsonInclude]
    public ComparisonOperationBase? ComparisonOperation { get; set; }

    /// <summary>
    /// The mapping rule to apply when the condition is true
    /// </summary>
    [JsonInclude]
    public MappingRuleBase? TrueMappingRule { get; set; }

    /// <summary>
    /// The mapping rule to apply when the condition is false
    /// </summary>
    [JsonInclude]
    public MappingRuleBase? FalseMappingRule { get; set; }

    /// <inheritdoc />
    [JsonIgnore]
    public override bool IsEmpty => ComparisonOperation == null || TrueMappingRule == null || FalseMappingRule == null;
    // Consider adding || ComparisonOperation.IsEmpty if ComparisonOperationBase gets an IsEmpty property.

    /// <inheritdoc />
    [JsonIgnore]
    public override Type OutputType => typeof(object); // Output type can vary based on the chosen rule.

    /// <inheritdoc />
    public override async Task<AbstractionsModels.TransformationResult> ApplyTransformationAsync(AbstractionsModels.TransformationResult previousResult)
    {
        if (previousResult.WasFailure)
        {
            return previousResult; // Propagate earlier failures
        }

        if (ComparisonOperation is null)
        {
            return AbstractionsModels.TransformationResult.Failure(previousResult.OriginalValue, OutputType, $"{nameof(ComparisonOperation)} is null. {MissingComponentMessage}", previousResult.OriginalValueType, null, previousResult.AppliedTransformations, previousResult.Record, previousResult.TableDefinition);
        }

        if (TrueMappingRule is null)
        {
            return AbstractionsModels.TransformationResult.Failure(previousResult.OriginalValue, OutputType, $"{nameof(TrueMappingRule)} is null. {MissingComponentMessage}", previousResult.OriginalValueType, null, previousResult.AppliedTransformations, previousResult.Record, previousResult.TableDefinition);
        }

        if (FalseMappingRule is null)
        {
            return AbstractionsModels.TransformationResult.Failure(previousResult.OriginalValue, OutputType, $"{nameof(FalseMappingRule)} is null. {MissingComponentMessage}", previousResult.OriginalValueType, null, previousResult.AppliedTransformations, previousResult.Record, previousResult.TableDefinition);
        }

        // Sub-rules (TrueMappingRule, FalseMappingRule) require a DataRow to operate.
        if (previousResult.Record is null)
        {
            return AbstractionsModels.TransformationResult.Failure(previousResult.OriginalValue, OutputType, MissingRecordMessage, previousResult.OriginalValueType, null, previousResult.AppliedTransformations, previousResult.Record, previousResult.TableDefinition);
        }

        try
        {
            // Evaluate the condition using the ComparisonOperation.
            // This assumes ComparisonOperation.Evaluate can take previousResult and use its CurrentValue.
            bool conditionMet = await ComparisonOperation.Evaluate(previousResult);

            if (conditionMet)
            {
                // If condition is true, apply the TrueMappingRule
                // TrueMappingRule.GetValue requires a DataRow, which is obtained from previousResult.Record
                return await TrueMappingRule.GetValue(previousResult.Record, this.OutputType);
            }
            else
            {
                // If condition is false, apply the FalseMappingRule
                return await FalseMappingRule.GetValue(previousResult.Record, this.OutputType);
            }
        }
        catch (Exception ex)
        {
            return AbstractionsModels.TransformationResult.Failure(previousResult.OriginalValue, OutputType, $"Error in conditional transformation: {ex.Message}", previousResult.OriginalValueType, null, previousResult.AppliedTransformations, previousResult.Record, previousResult.TableDefinition);
        }
    }

    /// <inheritdoc />
    public override async Task<AbstractionsModels.TransformationResult> Transform(object? value, Type targetType)
    {
        // This transformation relies on MappingRuleBase instances which typically need a DataRow context.
        // If called without a DataRow (e.g., record is null in initialResult),
        // the underlying TrueMappingRule or FalseMappingRule might not function as expected
        // unless they are types that don't depend on the DataRow (e.g., StaticValueRule).

        var initialResult = AbstractionsModels.TransformationResult.Success(
            originalValue: value,
            originalValueType: value?.GetType() ?? typeof(object),
            currentValue: value,
            currentValueType: value?.GetType() ?? typeof(object),
            record: null, // No DataRow context available here
            tableDefinition: null // No TableDefinition context available here
        );
        
        // Directly call ApplyTransformationAsync, which includes null checks for rules/operation.
        return await ApplyTransformationAsync(initialResult);
    }

    /// <inheritdoc />
    public override ValueTransformationBase Clone()
    {
        var clone = (ConditionalTransformation)MemberwiseClone();
        // TransformationDetail is a string (from base), memberwise clone is fine.
        clone.TransformationDetail = TransformationDetail; 

        clone.ComparisonOperation = ComparisonOperation?.Clone() as ComparisonOperationBase;
        clone.TrueMappingRule = TrueMappingRule?.Clone() as MappingRuleBase; // Assuming MappingRuleBase.Clone() returns MappingRuleBase or derived
        clone.FalseMappingRule = FalseMappingRule?.Clone() as MappingRuleBase; // Assuming MappingRuleBase.Clone() returns MappingRuleBase or derived
        return clone;
    }
}
