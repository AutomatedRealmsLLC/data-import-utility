using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Core.Models;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutomatedRealms.DataImportUtility.Core.ComparisonOperations;

/// <summary>
/// Checks if a value is not in a set of values.
/// </summary>
public class NotInOperation : ComparisonOperationBase
{
    /// <inheritdoc />
    [JsonIgnore]
    public override string EnumMemberName { get; } = nameof(NotInOperation);

    /// <inheritdoc />
    [JsonIgnore]
    public override string DisplayName { get; } = "Is not in";

    /// <inheritdoc />
    [JsonIgnore]
    public override string Description { get; } = "Checks if a value is not in a set of values.";

    public int EnumMemberOrder { get; set; }

    /// <summary>
    /// The set of values to check against.
    /// </summary>
    public IEnumerable<MappingRuleBase>? Values { get; set; }

    /// <inheritdoc />
    public override async Task<TransformationResult<bool>> Evaluate(ITransformationContext context)
    {
        if (LeftOperand is null)
        {
            return TransformationResult<bool>.CreateFailure($"{nameof(LeftOperand)} must be set for {DisplayName} operation.");
        }

        if (Values is null || !Values.Any())
        {
            return TransformationResult<bool>.CreateFailure($"{nameof(Values)} must be set and contain at least one value for {DisplayName} operation.");
        }

        var leftResult = await (LeftOperand?.Apply(context) ?? Task.FromResult(TransformationResult.CreateSuccess<string?>(null, context)));

        if (leftResult.WasFailure)
        {
            return TransformationResult<bool>.CreateFailure($"Failed to evaluate {nameof(LeftOperand)} for {DisplayName} operation: {leftResult.ErrorMessage}");
        }

        var valueResults = new List<TransformationResult<string?>>();
        foreach (var value in Values)
        {
            var valueResult = await value.Apply(context);
            if (valueResult.WasFailure)
            {
                return TransformationResult<bool>.CreateFailure($"Failed to evaluate a value in {nameof(Values)} for {DisplayName} operation: {valueResult.ErrorMessage}");
            }
            valueResults.Add(valueResult);
        }

        return TransformationResult<bool>.CreateSuccess(leftResult.NotIn(valueResults.ToArray()), context);
    }

    /// <inheritdoc />
    public override IComparisonOperation Clone()
    {
        return new NotInOperation
        {
            LeftOperand = LeftOperand?.Clone(),
            RightOperand = RightOperand?.Clone(), // Though NotInOperation doesn't typically use RightOperand, clone it for consistency if base class has it.
            Values = Values?.Select(v => (MappingRuleBase)v.Clone()).ToList(),
            EnumMemberOrder = EnumMemberOrder
        };
    }
}

/// <summary>
/// Extension methods for the NotIn operation.
/// </summary>
public static class NotInOperationExtensions
{
    /// <summary>
    /// Checks if the left result is not in the set of values.
    /// </summary>
    /// <param name="leftResult">The result of the left operand.</param>
    /// <param name="values">The set of values to check for.</param>
    /// <returns>True if the left result is not in the set of values; otherwise, false.</returns>
    public static bool NotIn(this TransformationResult<string?> leftResult, params TransformationResult<string?>[] values)
        => !leftResult.In(values); // Assumes In() extension method is available for TransformationResult<string?>
}
