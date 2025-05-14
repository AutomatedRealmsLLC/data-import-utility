using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Models;

using System.Text.Json.Serialization;

namespace AutomatedRealms.DataImportUtility.Core.ComparisonOperations;

/// <summary>
/// Operation to check if a value is null or white space.
/// </summary>
public class IsNullOrWhiteSpaceOperation : ComparisonOperationBase
{
    /// <summary>
    /// Static TypeId for this operation.
    /// </summary>
    public static readonly string TypeIdString = "Core.IsNullOrWhiteSpace";

    /// <inheritdoc />
    [JsonIgnore]
    public override string DisplayName { get; } = "Is Null or WhiteSpace";

    /// <inheritdoc />
    [JsonIgnore]
    public override string Description { get; } = "Checks if the input value is null, empty, or consists only of white-space characters.";

    /// <summary>
    /// Initializes a new instance of the <see cref="IsNullOrWhiteSpaceOperation"/> class.
    /// </summary>
    public IsNullOrWhiteSpaceOperation() : base(TypeIdString)
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
    /// Evaluates whether the left operand's value is null or consists only of white-space characters.
    /// </summary>
    /// <param name="contextResult">The transformation context.</param>
    /// <returns>True if the value is null or white space, otherwise false.</returns>
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

        var leftValueResult = await LeftOperand.Apply(context);

        if (leftValueResult == null)
        {
            throw new InvalidOperationException($"Applying {nameof(LeftOperand)} for {DisplayName} operation returned null unexpectedly.");
        }

        if (leftValueResult.WasFailure)
        {
            throw new InvalidOperationException($"Failed to evaluate {nameof(LeftOperand)} for {DisplayName} operation: {leftValueResult.ErrorMessage}");
        }

        object? leftValue = leftValueResult.CurrentValue;

        if (leftValue == null)
        {
            return true; // Null is considered whitespace for this operation
        }

        if (leftValue is string stringValue)
        {
            return string.IsNullOrWhiteSpace(stringValue);
        }

        // If it's not a string and not null, it cannot be "null or whitespace" in the string sense.
        return false;
    }

    /// <inheritdoc />
    public override ComparisonOperationBase Clone()
    {
        var clone = new IsNullOrWhiteSpaceOperation();
        if (LeftOperand != null)
        {
            clone.LeftOperand = LeftOperand.Clone();
        }
        // RightOperand is not used by this operation.
        return clone;
    }
}
