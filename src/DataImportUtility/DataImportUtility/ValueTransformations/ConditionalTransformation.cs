using System.Text.Json.Serialization;

using DataImportUtility.Abstractions;
using DataImportUtility.Models;

namespace DataImportUtility.ValueTransformations;

/// <summary>
/// Represents a transformation operation that applies a transformation based on a condition
/// </summary>
public partial class ConditionalTransformation : ValueTransformationBase
{
    /// <inheritdoc />
    public override string EnumMemberName => nameof(ConditionalTransformation);

    /// <inheritdoc />
    public override string DisplayName => "Conditional";

    /// <inheritdoc />
    public override string ShortName => "If/Else";

    /// <inheritdoc />
    public override string Description => "Evaluate a condition and apply a transformation based on the result";

    /// <summary>
    /// The error message when any required component is missing
    /// </summary>
    public const string MissingComponentMessage = "The conditional transformation requires a comparison operation, true mapping rule, and false mapping rule.";

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
    public override async Task<TransformationResult> Apply(TransformationResult result)
    {
        if (ComparisonOperation is null)
        {
            throw new InvalidOperationException($"{nameof(ComparisonOperation)} is null");
        }

        if (TrueMappingRule is null)
        {
            throw new InvalidOperationException($"{nameof(TrueMappingRule)} is null");
        }

        if (FalseMappingRule is null)
        {
            throw new InvalidOperationException($"{nameof(FalseMappingRule)} is null");
        }

        try
        {
            // Evaluate the condition with the current transformation result
            if (await ComparisonOperation.Evaluate(result))
            {
                // If condition is true, apply the TrueMappingRule
                // We're passing the original result so the TrueMappingRule can access
                // its own field configuration
                return await TrueMappingRule.Apply(result);
            }
            else
            {
                // This is passing the value of the original result to the FalseMappingRule
                // If that MappingRule has a transformation of Conditional Transformation
                // where the left mapping rule is a CopyRule, it will use this erroneously
                // use this value instead of the appropriate value from the original result.


                // If condition is false, apply the FalseMappingRule
                // We're passing the original result so the FalseMappingRule can access
                // its own field configuration
                return await FalseMappingRule.Apply(result);
            }
        }
        catch (Exception ex)
        {
            return result with { ErrorMessage = $"Error in conditional transformation: {ex.Message}" };
        }
    }

    /// <inheritdoc />
    public override ValueTransformationBase Clone()
    {
        var clone = (ConditionalTransformation)base.Clone();
        clone.ComparisonOperation = ComparisonOperation?.Clone() as ComparisonOperationBase;
        clone.TrueMappingRule = TrueMappingRule?.Clone();
        clone.FalseMappingRule = FalseMappingRule?.Clone();
        return clone;
    }
}