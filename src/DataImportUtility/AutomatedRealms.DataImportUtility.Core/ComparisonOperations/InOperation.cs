using System.Text.Json.Serialization;

using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Models;

namespace AutomatedRealms.DataImportUtility.Core.ComparisonOperations;

/// <summary>
/// Checks if a value is in a set of values.
/// </summary>
public class InOperation : ComparisonOperationBase
{
    /// <inheritdoc />
    [JsonIgnore]
    public override string EnumMemberName { get; } = nameof(InOperation);

    /// <inheritdoc />
    [JsonIgnore]
    public override string DisplayName { get; } = "In";

    /// <inheritdoc />
    [JsonIgnore]
    public override string Description { get; } = "Checks if a value is in a set of values.";

    /// <summary>
    /// The set of values to check against.
    /// </summary>
    public IEnumerable<MappingRuleBase>? Values { get; set; }

    /// <inheritdoc />
    public override async Task<bool> Evaluate(TransformationResult contextResult)
    {
        if (LeftOperand is null)
        {
            throw new InvalidOperationException($"Both {nameof(LeftOperand)} must be set for {DisplayName} operation.");
        }

        if (Values is null || !Values.Any())
        {
            throw new InvalidOperationException($"{nameof(Values)} must be set and contain at least one value for {DisplayName} operation.");
        }

        var leftResult = await LeftOperand.Apply(contextResult);

        if (leftResult == null || leftResult.WasFailure)
        {
            throw new InvalidOperationException($"Failed to evaluate {nameof(LeftOperand)} for {DisplayName} operation: {leftResult?.ErrorMessage ?? "Result was null."}");
        }

        var valueResults = new List<TransformationResult>();
        foreach (var valueRule in Values)
        {
            if (valueRule == null) continue;
            var valueEvaluationResult = await valueRule.Apply(contextResult);
            if (valueEvaluationResult == null || valueEvaluationResult.WasFailure)
            {
                throw new InvalidOperationException($"Failed to evaluate a value in {nameof(Values)} for {DisplayName} operation: {valueEvaluationResult?.ErrorMessage ?? "Result was null."}");
            }
            valueResults.Add(valueEvaluationResult);
        }

        return leftResult.In([.. valueResults]);
    }

    /// <inheritdoc />
    public override ComparisonOperationBase Clone()
    {
        return new InOperation
        {
            LeftOperand = LeftOperand?.Clone(),
            RightOperand = RightOperand?.Clone(),
            Values = Values?.Select(v => v.Clone()).ToList(),
        };
    }
}

/// <summary>
/// Extension methods for the In operation.
/// </summary>
public static class InOperationExtensions
{
    /// <summary>
    /// Checks if the left result is in the set of values.
    /// </summary>
    /// <param name="leftResult">The result of the left operand.</param>
    /// <param name="values">The set of values to check for.</param>
    /// <returns>True if the left result is in the set of values; otherwise, false.</returns>
    public static bool In(this TransformationResult leftResult, params TransformationResult[] values)
    {
        if (leftResult.CurrentValue is null)
        {
            return values.Any(val => val.CurrentValue is null);
        }

        return values.Any(val => object.Equals(leftResult.CurrentValue, val.CurrentValue));
    }
}
