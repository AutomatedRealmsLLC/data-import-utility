using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System;
using AutomatedRealms.DataImportUtility.Abstractions;
using AbstractionsModels = AutomatedRealms.DataImportUtility.Abstractions.Models;
using CoreRules = AutomatedRealms.DataImportUtility.Core.Rules;
using System.Data;
using System.Collections.Generic;

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
    public AbstractionsModels.MappingRuleBase? TrueMappingRule { get; set; }

    /// <summary>
    /// The mapping rule to apply when the condition is false
    /// </summary>
    [JsonInclude]
    public AbstractionsModels.MappingRuleBase? FalseMappingRule { get; set; }

    /// <inheritdoc />
    [JsonIgnore]
    public override bool IsEmpty => ComparisonOperation == null || TrueMappingRule == null || FalseMappingRule == null;

    /// <inheritdoc />
    [JsonIgnore]
    public override Type OutputType => typeof(object);

    /// <inheritdoc />
    public override async Task<AbstractionsModels.TransformationResult> ApplyTransformationAsync(AbstractionsModels.TransformationResult previousResult)
    {
        if (previousResult.WasFailure)
        {
            return previousResult;
        }

        if (ComparisonOperation is null)
        {
            return AbstractionsModels.TransformationResult.Failure(
                originalValue: previousResult.OriginalValue,
                targetType: OutputType,
                errorMessage: $"{nameof(ComparisonOperation)} is null. {MissingComponentMessage}",
                originalValueType: previousResult.OriginalValueType,
                currentValueType: null,
                appliedTransformations: previousResult.AppliedTransformations,
                record: previousResult.Record,
                tableDefinition: previousResult.TableDefinition,
                sourceRecordContext: previousResult.SourceRecordContext,
                explicitTargetFieldType: previousResult.TargetFieldType);
        }

        if (TrueMappingRule is null)
        {
            return AbstractionsModels.TransformationResult.Failure(
                originalValue: previousResult.OriginalValue,
                targetType: OutputType,
                errorMessage: $"{nameof(TrueMappingRule)} is null. {MissingComponentMessage}",
                originalValueType: previousResult.OriginalValueType,
                currentValueType: null,
                appliedTransformations: previousResult.AppliedTransformations,
                record: previousResult.Record,
                tableDefinition: previousResult.TableDefinition,
                sourceRecordContext: previousResult.SourceRecordContext,
                explicitTargetFieldType: previousResult.TargetFieldType);
        }

        if (FalseMappingRule is null)
        {
            return AbstractionsModels.TransformationResult.Failure(
                originalValue: previousResult.OriginalValue,
                targetType: OutputType,
                errorMessage: $"{nameof(FalseMappingRule)} is null. {MissingComponentMessage}",
                originalValueType: previousResult.OriginalValueType,
                currentValueType: null,
                appliedTransformations: previousResult.AppliedTransformations,
                record: previousResult.Record,
                tableDefinition: previousResult.TableDefinition,
                sourceRecordContext: previousResult.SourceRecordContext,
                explicitTargetFieldType: previousResult.TargetFieldType);
        }

        // Evaluate condition once
        bool conditionMet = await ComparisonOperation.Evaluate(previousResult);

        // Check if the chosen rule needs a record and if the record is missing
        bool ruleNeedsRecord = false;
        if (conditionMet)
        {
            ruleNeedsRecord = !(TrueMappingRule is CoreRules.StaticValueRule || TrueMappingRule is CoreRules.ConstantValueRule || TrueMappingRule is CoreRules.IgnoreRule);
        }
        else
        {
            ruleNeedsRecord = !(FalseMappingRule is CoreRules.StaticValueRule || FalseMappingRule is CoreRules.ConstantValueRule || FalseMappingRule is CoreRules.IgnoreRule);
        }

        if (ruleNeedsRecord && previousResult.Record is null)
        {
            return AbstractionsModels.TransformationResult.Failure(
                originalValue: previousResult.OriginalValue,
                targetType: OutputType,
                errorMessage: MissingRecordMessage,
                originalValueType: previousResult.OriginalValueType,
                currentValueType: null,
                appliedTransformations: previousResult.AppliedTransformations,
                record: previousResult.Record,
                tableDefinition: previousResult.TableDefinition,
                sourceRecordContext: previousResult.SourceRecordContext,
                explicitTargetFieldType: previousResult.TargetFieldType);
        }

        try
        {
            AbstractionsModels.TransformationResult? ruleResult;
            if (conditionMet)
            {
                // TrueMappingRule is guaranteed non-null by checks above
                ruleResult = await TrueMappingRule!.Apply(previousResult);
            }
            else
            {
                // FalseMappingRule is guaranteed non-null by checks above
                ruleResult = await FalseMappingRule!.Apply(previousResult);
            }

            if (ruleResult == null)
            {
                return AbstractionsModels.TransformationResult.Failure(
                    originalValue: previousResult.OriginalValue,
                    targetType: OutputType,
                    errorMessage: "The executed conditional rule returned a null result.",
                    originalValueType: previousResult.OriginalValueType,
                    currentValueType: null,
                    appliedTransformations: previousResult.AppliedTransformations,
                    record: previousResult.Record,
                    tableDefinition: previousResult.TableDefinition,
                    sourceRecordContext: previousResult.SourceRecordContext,
                    explicitTargetFieldType: previousResult.TargetFieldType);
            }
            return ruleResult;
        }
        catch (Exception ex)
        {
            return AbstractionsModels.TransformationResult.Failure(
                originalValue: previousResult.OriginalValue,
                targetType: OutputType,
                errorMessage: $"Error in conditional transformation: {ex.Message}",
                originalValueType: previousResult.OriginalValueType,
                currentValueType: null,
                appliedTransformations: previousResult.AppliedTransformations,
                record: previousResult.Record,
                tableDefinition: previousResult.TableDefinition,
                sourceRecordContext: previousResult.SourceRecordContext,
                explicitTargetFieldType: previousResult.TargetFieldType);
        }
    }

    /// <inheritdoc />
    public override async Task<AbstractionsModels.TransformationResult> Transform(object? value, Type targetType)
    {
        var initialResult = AbstractionsModels.TransformationResult.Success(
            originalValue: value,
            originalValueType: value?.GetType() ?? typeof(object),
            currentValue: value,
            currentValueType: value?.GetType() ?? typeof(object),
            appliedTransformations: new List<string>(),
            record: null,
            tableDefinition: null,
            sourceRecordContext: null,
            targetFieldType: targetType
        );

        return await ApplyTransformationAsync(initialResult);
    }

    /// <inheritdoc />
    public override ValueTransformationBase Clone()
    {
        var clone = (ConditionalTransformation)MemberwiseClone();
        clone.TransformationDetail = TransformationDetail;

        clone.ComparisonOperation = ComparisonOperation?.Clone() as ComparisonOperationBase;
        clone.TrueMappingRule = TrueMappingRule?.Clone() as AbstractionsModels.MappingRuleBase;
        clone.FalseMappingRule = FalseMappingRule?.Clone() as AbstractionsModels.MappingRuleBase;
        return clone;
    }
}
