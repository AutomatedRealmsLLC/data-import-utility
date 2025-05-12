using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Models; // For TransformationResult
using AutomatedRealms.DataImportUtility.Abstractions.Interfaces; // For ITransformationContext
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System;
using System.Collections; // For IEnumerable

namespace AutomatedRealms.DataImportUtility.Core.ComparisonOperations;

/// <summary>
/// Checks if a value does not contain another value.
/// </summary>
public class NotContainsOperation : ComparisonOperationBase
{
    /// <inheritdoc />
    [JsonIgnore]
    public override string EnumMemberName { get; } = nameof(NotContainsOperation);

    /// <inheritdoc />
    [JsonIgnore]
    public override string DisplayName { get; } = "Does not contain";

    /// <inheritdoc />
    [JsonIgnore]
    public override string Description { get; } = "Checks if a value does not contain another value (case-insensitive).";

    /// <summary>
    /// Gets or sets the order of this operation if it's part of an enumerated list of operations.
    /// </summary>
    public int EnumMemberOrder { get; set; }

    /// <inheritdoc />
    public override async Task<bool> Evaluate(TransformationResult contextResult) // contextResult is an ITransformationContext
    {
        if (LeftOperand is null)
        {
            throw new InvalidOperationException($"{nameof(LeftOperand)} must be set for {DisplayName} operation.");
        }
        if (RightOperand is null)
        {
            throw new InvalidOperationException($"{nameof(RightOperand)} must be set for {DisplayName} operation.");
        }

        var leftResult = await LeftOperand.Apply(contextResult);
        var rightResult = await RightOperand.Apply(contextResult);

        if (leftResult == null || leftResult.WasFailure)
        {
            throw new InvalidOperationException($"Failed to evaluate {nameof(LeftOperand)} for {DisplayName} operation: {leftResult?.ErrorMessage ?? "Result was null."}");
        }
        if (rightResult == null || rightResult.WasFailure)
        {
            throw new InvalidOperationException($"Failed to evaluate {nameof(RightOperand)} for {DisplayName} operation: {rightResult?.ErrorMessage ?? "Result was null."}");
        }

        object? leftValue = leftResult.CurrentValue;
        object? rightValue = rightResult.CurrentValue;

        if (leftValue == null) 
        {
            // null does not contain anything, unless rightValue is also null (which is debatable for 'contains')
            // If rightValue is null, previous Contains logic said true (everything contains null).
            // So, !(null contains null) = !true = false.
            // If rightValue is not null, previous Contains logic said false (null does not contain non-null).
            // So, !(null contains non-null) = !false = true.
            return rightValue != null; 
        }

        if (rightValue == null)
        {
            // Previous Contains logic: everything contains null (empty string) -> true.
            // So, NotContains null is false.
            return false; 
        }

        string leftString = leftValue.ToString() ?? ""; // Ensure not null for operations
        string rightString = rightValue.ToString() ?? "";

        // Handle if leftValue is a collection (e.g., string array from a multi-select or split)
        if (leftValue is IEnumerable enumerable && !(leftValue is string))
        {
            foreach (var item in enumerable)
            {
                if (item?.ToString()?.IndexOf(rightString, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return false; // Found a match in the collection, so "NotContains" is false.
                }
            }
            return true; // No match in the collection, so "NotContains" is true.
        }

        // Standard string contains check (case-insensitive)
        return leftString.IndexOf(rightString, StringComparison.OrdinalIgnoreCase) < 0;
    }

    /// <inheritdoc />
    public override ComparisonOperationBase Clone() // Changed return type
    {
        return new NotContainsOperation
        {
            LeftOperand = LeftOperand?.Clone(),
            RightOperand = RightOperand?.Clone(),
            EnumMemberOrder = EnumMemberOrder
        };
    }
}
