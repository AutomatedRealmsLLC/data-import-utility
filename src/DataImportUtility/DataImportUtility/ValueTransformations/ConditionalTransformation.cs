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
    /// The comparison operation to evaluate
    /// </summary>
    public ComparisonOperationBase? ComparisonOperation { get; set; }

    /// <summary>
    /// The condition to evaluate
    /// </summary>
    public MappingRuleBase? TrueMappingRule { get; set; }

    /// <summary>
    /// The operation to apply if the condition is true
    /// </summary>
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

        return await (await ComparisonOperation.Evaluate(result)
            ? TrueMappingRule.Apply(result)
            : FalseMappingRule.Apply(result));
    }
}
