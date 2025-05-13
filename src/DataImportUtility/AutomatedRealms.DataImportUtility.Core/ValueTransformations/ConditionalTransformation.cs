using System.Text.Json.Serialization;

using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Models;
using CoreRules = AutomatedRealms.DataImportUtility.Core.Rules;

namespace AutomatedRealms.DataImportUtility.Core.ValueTransformations;

/// <summary>
/// Represents a transformation operation that applies a transformation based on a condition
/// </summary>
public partial class ConditionalTransformation : ValueTransformationBase
{
    /// <summary>
    /// Static TypeId for this transformation.
    /// </summary>
    public static readonly string TypeIdString = "Core.ConditionalTransformation";

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
    /// Initializes a new instance of the <see cref="ConditionalTransformation"/> class.
    /// </summary>
    public ConditionalTransformation() : base(TypeIdString) { }

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

    /// <inheritdoc />
    [JsonIgnore]
    public override Type OutputType => typeof(object);

    /// <inheritdoc />
    public override async Task<TransformationResult> ApplyTransformationAsync(TransformationResult previousResult)
    {
        if (previousResult.WasFailure)
        {
            return previousResult;
        }

        if (ComparisonOperation is null)
        {
            return TransformationResult.Failure(
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
            return TransformationResult.Failure(
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
            return TransformationResult.Failure(
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
            return TransformationResult.Failure(
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
            TransformationResult? ruleResult;
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
                return TransformationResult.Failure(
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
            return TransformationResult.Failure(
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
    public override async Task<TransformationResult> Transform(object? value, Type targetType)
    {
        var initialResult = TransformationResult.Success(
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
        var clone = new ConditionalTransformation() // Ensure new instance with TypeId set by constructor
        {
            TransformationDetail = this.TransformationDetail,
            ComparisonOperation = this.ComparisonOperation?.Clone(),
            TrueMappingRule = this.TrueMappingRule?.Clone(),
            FalseMappingRule = this.FalseMappingRule?.Clone()
        };
        return clone;
    }
}
