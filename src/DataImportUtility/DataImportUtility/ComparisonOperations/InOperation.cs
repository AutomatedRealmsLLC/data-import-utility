using DataImportUtility.Abstractions;
using DataImportUtility.Models;

namespace DataImportUtility.ComparisonOperations;

/// <summary>
/// Checks if a value is in a set of values.
/// </summary>
public class InOperation : ComparisonOperationBase
{
    /// <inheritdoc />
    public override string EnumMemberName { get; } = nameof(InOperation);

    /// <inheritdoc />
    public override string DisplayName { get; } = "In";

    /// <inheritdoc />
    public override string Description { get; } = "Checks if a value is in a set of values.";

    /// <summary>
    /// The set of values to check against.
    /// </summary>
    public IEnumerable<MappingRuleBase>? Values { get; set; }

    /// <inheritdoc />
    public override async Task<bool> Evaluate(TransformationResult result)
    {
        if (LeftOperand is null)
        {
            throw new InvalidOperationException($"{nameof(LeftOperand)} must be set for {nameof(InOperation)}.");
        }

        if (Values is null || !Values.Any())
        {
            throw new InvalidOperationException($"{nameof(Values)} must be set and contain at least one value for {nameof(InOperation)}.");
        }

        var leftResult = await LeftOperand.Apply(result);

        if (leftResult.WasFailure)
        {
            throw new InvalidOperationException($"Failed to evaluate {nameof(LeftOperand)} for {DisplayName} operation: {leftResult.ErrorMessage}");
        }

        var valueResults = new List<TransformationResult>();
        foreach (var value in Values)
        {
            var valueResult = await value.Apply(result);
            if (valueResult.WasFailure)
            {
                throw new InvalidOperationException($"Failed to evaluate a value in {nameof(Values)} for {DisplayName} operation: {valueResult.ErrorMessage}");
            }
            valueResults.Add(valueResult);
        }

        return leftResult.In([.. valueResults]);
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
        throw new NotImplementedException();
    }
}