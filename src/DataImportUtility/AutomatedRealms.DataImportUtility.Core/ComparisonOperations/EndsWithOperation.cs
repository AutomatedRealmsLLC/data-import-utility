using System.Text.Json.Serialization;

using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Models; // Updated for TransformationResult

namespace AutomatedRealms.DataImportUtility.Core.ComparisonOperations;

/// <summary>
/// Checks if a string value ends with another string value.
/// </summary>
public class EndsWithOperation : ComparisonOperationBase
{
    // EnumMemberOrder removed

    /// <inheritdoc />
    public override string EnumMemberName { get; } = nameof(EndsWithOperation); // Or a more specific enum if ComparisonOperatorType is introduced

    /// <inheritdoc />
    [JsonIgnore]
    public override string DisplayName { get; } = "Ends With";

    /// <inheritdoc />
    [JsonIgnore]
    public override string Description { get; } = "Checks if a string value ends with another string value.";

    /// <summary>
    /// Initializes a new instance of the <see cref="EndsWithOperation"/> class.
    /// </summary>
    public EndsWithOperation() : base()
    {
        // Operands (LeftOperand, RightOperand) are set via properties.
    }

    /// <inheritdoc />
    public override async Task<bool> Evaluate(TransformationResult contextResult)
    {
        if (LeftOperand is null || RightOperand is null)
        {
            // Consider logging contextResult.AddLogMessage("Error: Operands not set for EndsWithOperation.");
            throw new InvalidOperationException($"Both {nameof(LeftOperand)} and {nameof(RightOperand)} must be set for {nameof(EndsWithOperation)}.");
        }

        // contextResult is already an ITransformationContext
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

        return EndsWithOperationExtensions.EndsWith(leftOperandActualResult, rightOperandActualResult);
    }

    /// <inheritdoc />
    public override ComparisonOperationBase Clone()
    {
        // Base Clone() handles LeftOperand, RightOperand and MemberwiseClone.
        return (EndsWithOperation)base.Clone();
    }
}

/// <summary>
/// Extension methods for the EndsWith operation.
/// </summary>
public static class EndsWithOperationExtensions
{
    /// <summary>
    /// Checks if the string representation of the left result's current value ends with the string representation of the right result's current value.
    /// </summary>
    /// <param name="leftResult">The transformation result containing the main string value.</param>
    /// <param name="valueToCheck">The transformation result containing the suffix value.</param>
    /// <returns>True if the main string ends with the suffix string (case-insensitive); otherwise, false.</returns>
    public static bool EndsWith(this TransformationResult leftResult, TransformationResult valueToCheck)
    {
        object? mainValue = leftResult.CurrentValue;
        object? suffixValue = valueToCheck.CurrentValue;

        if (mainValue == null || suffixValue == null)
        {
            return false; // Cannot perform 'EndsWith' if either value is null.
        }

        string? mainString = mainValue.ToString();
        string? suffixString = suffixValue.ToString();

        if (mainString == null || suffixString == null) // Should be redundant if mainValue/suffixValue are not null, but good practice.
        {
            return false;
        }

        // Using OrdinalIgnoreCase for a common case-insensitive check.
        // Change to StringComparison.Ordinal for case-sensitive.
        return mainString.EndsWith(suffixString, StringComparison.OrdinalIgnoreCase);
    }
}
