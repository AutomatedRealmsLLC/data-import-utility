using System.Collections; // For IEnumerable
using System.Text.Json.Serialization;

using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Models; // Updated for TransformationResult

namespace AutomatedRealms.DataImportUtility.Core.ComparisonOperations;

/// <summary>
/// Checks if a value contains another value.
/// If the primary value is a collection, it checks for element existence.
/// If the primary value is a string, it checks for substring existence.
/// </summary>
public class ContainsOperation : ComparisonOperationBase
{
    // EnumMemberOrder removed

    /// <inheritdoc />
    public override string EnumMemberName { get; } = nameof(ContainsOperation); // Or a more specific enum if ComparisonOperatorType is introduced

    /// <inheritdoc />
    [JsonIgnore]
    public override string DisplayName { get; } = "Contains";

    /// <inheritdoc />
    [JsonIgnore]
    public override string Description { get; } = "Checks if a value (string or collection) contains another value.";

    /// <summary>
    /// Initializes a new instance of the <see cref="ContainsOperation"/> class.
    /// </summary>
    public ContainsOperation() : base()
    {
        // Operands (LeftOperand, RightOperand) are set via properties.
    }

    /// <inheritdoc />
    public override async Task<bool> Evaluate(TransformationResult contextResult)
    {
        if (LeftOperand is null || RightOperand is null)
        {
            // Consider logging contextResult.AddLogMessage("Error: Operands not set for ContainsOperation.");
            throw new InvalidOperationException($"Both {nameof(LeftOperand)} and {nameof(RightOperand)} must be set for {nameof(ContainsOperation)}.");
        }

        // contextResult is already an ITransformationContext
        // No need to check contextResult.Record == null specifically here, as Apply methods should handle context appropriately.

        var leftOperandActualResult = await LeftOperand.Apply(contextResult);
        if (leftOperandActualResult == null || leftOperandActualResult.WasFailure)
        {
            // Consider logging
            throw new InvalidOperationException($"Failed to evaluate {nameof(LeftOperand)} for {DisplayName} operation: {leftOperandActualResult?.ErrorMessage ?? "Result was null."}");
        }

        var rightOperandActualResult = await RightOperand.Apply(contextResult);
        if (rightOperandActualResult == null || rightOperandActualResult.WasFailure)
        {
            // Consider logging
            throw new InvalidOperationException($"Failed to evaluate {nameof(RightOperand)} for {DisplayName} operation: {rightOperandActualResult?.ErrorMessage ?? "Result was null."}");
        }

        return ContainsOperationExtensions.Contains(leftOperandActualResult, rightOperandActualResult);
    }

    /// <inheritdoc />
    public override ComparisonOperationBase Clone()
    {
        // Base Clone() handles LeftOperand, RightOperand and MemberwiseClone.
        return (ContainsOperation)base.Clone();
    }
}

/// <summary>
/// Extension methods for the Contains operation.
/// </summary>
public static class ContainsOperationExtensions
{
    /// <summary>
    /// Checks if the main value (from leftResult) contains the sub-value (from valueToFind).
    /// If mainValue is an IEnumerable (and not a string), it checks for element equality.
    /// Otherwise, it performs a string.Contains check on their string representations.
    /// </summary>
    /// <param name="leftResult">The transformation result containing the main value.</param>
    /// <param name="valueToFind">The transformation result containing the value to find.</param>
    /// <returns>True if the main value contains the sub-value; otherwise, false.</returns>
    public static bool Contains(this TransformationResult leftResult, TransformationResult valueToFind)
    {
        object? mainValue = leftResult.CurrentValue;
        object? subValue = valueToFind.CurrentValue;

        if (mainValue == null || subValue == null)
        {
            return false; // Cannot perform 'contains' if either value is null.
        }

        // Case 1: mainValue is a collection (but not a string, as string is also IEnumerable<char>)
        if (mainValue is IEnumerable enumerableMainValue && mainValue is not string)
        {
            foreach (var item in enumerableMainValue)
            {
                if (object.Equals(item, subValue)) // Use object.Equals for type-aware comparison
                {
                    return true;
                }
            }
            return false; // subValue not found in the collection
        }

        // Case 2: Treat as strings (covers string mainValue and other types converted to string)
        string? mainString = mainValue.ToString();
        string? subString = subValue.ToString();

        // After ToString(), check for null again (though unlikely if original objects were not null)
        if (mainString == null || subString == null)
        {
            return false;
        }

        // Using OrdinalIgnoreCase for a common case-insensitive string search.
        // Change to StringComparison.Ordinal for case-sensitive.
        return mainString.IndexOf(subString, StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
