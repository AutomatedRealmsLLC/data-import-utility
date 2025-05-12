using System.Text.Json.Serialization;

using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Enums;
using AutomatedRealms.DataImportUtility.Abstractions.Models;

namespace AutomatedRealms.DataImportUtility.Core.ComparisonOperations;

/// <summary>
/// Checks if a value starts with another value.
/// </summary>
public class StartsWithOperation : ComparisonOperationBase
{
    /// <inheritdoc />
    [JsonIgnore]
    public override string EnumMemberName => ComparisonOperationType.StartsWith.ToString();

    /// <inheritdoc />
    [JsonIgnore]
    public override string DisplayName { get; } = "Starts with";

    /// <inheritdoc />
    [JsonIgnore]
    public override string Description { get; } = "Checks if a value starts with another value.";

    /// <summary>
    /// Initializes a new instance of the <see cref="StartsWithOperation"/> class.
    /// </summary>
    public StartsWithOperation() : base()
    {
    }

    /// <inheritdoc />
    public override async Task<bool> Evaluate(TransformationResult contextResult)
    {
        if (LeftOperand is null || RightOperand is null)
        {
            return false; // Misconfigured
        }

        var leftResult = await LeftOperand.Apply(contextResult);
        if (leftResult == null || leftResult.WasFailure || leftResult.CurrentValue == null)
        {
            return false; // Failed to get left value or it's null
        }

        var rightResult = await RightOperand.Apply(contextResult);
        if (rightResult == null || rightResult.WasFailure || rightResult.CurrentValue == null)
        {
            return false; // Failed to get right value or it's null
        }

        if (leftResult.CurrentValue is string leftString && rightResult.CurrentValue is string rightString)
        {
            // Handle null cases for strings explicitly, though Apply should ensure non-null if not WasFailure
            if (leftString == null || rightString == null) { return false; }
            return leftString.StartsWith(rightString, StringComparison.Ordinal);
        }

        return false; // Operands are not strings
    }

    /// <inheritdoc />
    public override ComparisonOperationBase Clone()
    {
        var clone = new StartsWithOperation
        {
        };
        if (LeftOperand != null)
        {
            clone.LeftOperand = LeftOperand.Clone();
        }
        if (RightOperand != null)
        {
            clone.RightOperand = RightOperand.Clone();
        }
        return clone;
    }
}
