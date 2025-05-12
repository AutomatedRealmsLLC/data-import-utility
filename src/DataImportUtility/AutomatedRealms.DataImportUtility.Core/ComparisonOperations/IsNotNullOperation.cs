using System.Text.Json.Serialization;

using AutomatedRealms.DataImportUtility.Abstractions;
// using AutomatedRealms.DataImportUtility.Core.Models; // Removed
using AutomatedRealms.DataImportUtility.Abstractions.Models; // Ensured

namespace AutomatedRealms.DataImportUtility.Core.ComparisonOperations;

/// <summary>
/// Checks if a value is not null.
/// </summary>
public class IsNotNullOperation : ComparisonOperationBase
{
    /// <inheritdoc />
    [JsonIgnore]
    public override string EnumMemberName { get; } = nameof(IsNotNullOperation);

    /// <inheritdoc />
    [JsonIgnore]
    public override string DisplayName { get; } = "Is not null";

    /// <inheritdoc />
    [JsonIgnore]
    public override string Description { get; } = "Checks if a value is not null.";

    // public int EnumMemberOrder { get; set; } // Property seems to be unused, consider removing. Assuming it's not for now.

    /// <inheritdoc />
    public override async Task<bool> Evaluate(TransformationResult contextResult) // Signature updated
    {
        if (LeftOperand is null)
        {
            throw new InvalidOperationException($"{nameof(LeftOperand)} must be set for {DisplayName} operation.");
        }

        var leftResult = await LeftOperand.Apply(contextResult); // Pass contextResult

        if (leftResult == null || leftResult.WasFailure) // Check for null result as well, though Apply should ideally not return null if successful.
        {
            // If WasFailure is true, or result is null (unexpected for a successful Apply), treat as evaluation failure.
            throw new InvalidOperationException($"Failed to evaluate {nameof(LeftOperand)} for {DisplayName} operation: {leftResult?.ErrorMessage ?? "Result was unexpectedly null."}");
        }

        return leftResult.IsNotNull(); // Call updated extension
    }

    /// <inheritdoc />
    public override ComparisonOperationBase Clone() // Signature updated
    {
        return new IsNotNullOperation
        {
            LeftOperand = LeftOperand?.Clone(),
            RightOperand = RightOperand?.Clone(), // Cloning RightOperand for consistency with base class
            // EnumMemberOrder = EnumMemberOrder // If EnumMemberOrder is kept
        };
    }
}

/// <summary>
/// Extension method for the IsNotNull operation.
/// </summary>
public static class IsNotNullOperationExtensions
{
    /// <summary>
    /// Checks if the result's current value is not null.
    /// </summary>
    /// <param name="result">The transformation result to check.</param>
    /// <returns>True if the result's current value is not null; otherwise, false.</returns>
    public static bool IsNotNull(this TransformationResult result)
    {
        return result.CurrentValue != null;
    }

    // Assuming IsNull() would be defined in IsNullOperationExtensions
    // public static bool IsNull(this TransformationResult result) => result.CurrentValue == null;
}
