using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Models;

using System.Text.Json.Serialization;

namespace AutomatedRealms.DataImportUtility.Core.ComparisonOperations;

/// <summary>
/// Checks if a field's value is null or an empty string.
/// </summary>
public class IsNullOrEmptyOperation : ComparisonOperationBase
{
    /// <summary>
    /// Static TypeId for this operation.
    /// </summary>
    public static readonly string TypeIdString = "Core.IsNullOrEmpty";

    /// <inheritdoc />
    [JsonIgnore]
    public override string DisplayName { get; } = "Is Null Or Empty";

    /// <inheritdoc />
    [JsonIgnore]
    public override string Description { get; } = "Checks if the field value is null or an empty string.";

    /// <summary>
    /// Initializes a new instance of the <see cref="IsNullOrEmptyOperation"/> class.
    /// </summary>
    public IsNullOrEmptyOperation() : base(TypeIdString)
    {
    }

    /// <inheritdoc />
    public override void ConfigureOperands(MappingRuleBase? leftOperand, MappingRuleBase? rightOperand = null, MappingRuleBase? secondaryRightOperand = null)
    {
        if (leftOperand == null)
        {
            throw new ArgumentNullException(nameof(leftOperand), $"Left operand cannot be null for {DisplayName}.");
        }
        LeftOperand = leftOperand;
        // RightOperand and SecondaryRightOperand are not used by this operation.
    }

    /// <summary>
    /// Evaluates if the LeftOperand's resolved value is null or an empty string.
    /// </summary>
    /// <param name="contextResult">The transformation context.</param>
    /// <returns>True if the LeftOperand's value is null or empty; otherwise, false.</returns>
    public override async Task<bool> Evaluate(TransformationResult contextResult)
    {
        if (contextResult is not ITransformationContext context)
        {
            if (contextResult is ITransformationContext directContext)
            {
                context = directContext;
            }
            else
            {
                throw new InvalidOperationException($"The provided contextResult (type: {contextResult?.GetType().FullName}) could not be interpreted as an {nameof(ITransformationContext)} for {DisplayName} operation.");
            }
        }

        if (LeftOperand == null)
        {
            throw new InvalidOperationException($"{nameof(LeftOperand)} must be configured for {DisplayName} operation.");
        }

        var leftOperandValueResult = await LeftOperand.Apply(context);

        if (leftOperandValueResult == null)
        {
            throw new InvalidOperationException($"Applying {nameof(LeftOperand)} for {DisplayName} operation returned null unexpectedly.");
        }

        if (leftOperandValueResult.WasFailure)
        {
            throw new InvalidOperationException($"Failed to evaluate {nameof(LeftOperand)} for {DisplayName} operation: {leftOperandValueResult.ErrorMessage}");
        }

        object? valueToCheck = leftOperandValueResult.CurrentValue;

        if (valueToCheck == null)
        {
            return true; // Null is considered "null or empty"
        }

        if (valueToCheck is string stringValue)
        {
            return string.IsNullOrEmpty(stringValue);
        }

        // For non-string, non-null types, consider them not "null or empty" in the string sense.
        return false;
    }

    /// <inheritdoc />
    public override ComparisonOperationBase Clone()
    {
        var clone = new IsNullOrEmptyOperation();
        if (LeftOperand != null)
        {
            clone.LeftOperand = LeftOperand.Clone();
        }
        // RightOperand is not used by this operation.
        return clone;
    }
}
