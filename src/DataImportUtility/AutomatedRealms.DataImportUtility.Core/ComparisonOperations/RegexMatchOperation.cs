using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Models;

using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace AutomatedRealms.DataImportUtility.Core.ComparisonOperations;

/// <summary>
/// Operation to check if a string matches a regular expression pattern.
/// </summary>
public class RegexMatchOperation : ComparisonOperationBase
{
    /// <summary>
    /// Static TypeId for this operation.
    /// </summary>
    public static readonly string TypeIdString = "Core.RegexMatch";

    /// <inheritdoc />
    [JsonIgnore]
    public override string DisplayName { get; } = "Regex Match";

    /// <inheritdoc />
    [JsonIgnore]
    public override string Description { get; } = "Checks if the input string matches the specified regular expression pattern.";

    /// <summary>
    /// Initializes a new instance of the <see cref="RegexMatchOperation"/> class.
    /// </summary>
    public RegexMatchOperation() : base(TypeIdString)
    {
    }

    /// <inheritdoc />
    public override void ConfigureOperands(MappingRuleBase? leftOperand, MappingRuleBase? rightOperand, MappingRuleBase? secondaryRightOperand = null)
    {
        if (leftOperand is null)
        {
            throw new ArgumentNullException(nameof(leftOperand), $"Left operand (input string) cannot be null for {DisplayName}.");
        }
        if (rightOperand is null)
        {
            throw new ArgumentNullException(nameof(rightOperand), $"Right operand (regex pattern) cannot be null for {DisplayName}.");
        }
        LeftOperand = leftOperand;
        RightOperand = rightOperand;
        // SecondaryRightOperand is not used by this operation.
    }

    /// <summary>
    /// Evaluates whether the left operand's string value matches the regex pattern provided by the right operand.
    /// </summary>
    /// <param name="contextResult">The transformation result context.</param>
    /// <returns>True if the input string matches the pattern, otherwise false.</returns>
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

        if (LeftOperand is null)
        {
            throw new InvalidOperationException($"{nameof(LeftOperand)} (input string) must be configured for {DisplayName} operation.");
        }
        if (RightOperand is null)
        {
            throw new InvalidOperationException($"{nameof(RightOperand)} (regex pattern) must be configured for {DisplayName} operation.");
        }

        var leftValueResult = await LeftOperand.Apply(context);
        if (leftValueResult is null || leftValueResult.WasFailure)
        {
            // If input evaluation fails, cannot perform regex match.
            // Consider logging: Console.WriteLine($"Warning: LeftOperand evaluation failed for RegexMatch: {leftValueResult?.ErrorMessage}");
            return false;
        }

        var rightValueResult = await RightOperand.Apply(context);
        if (rightValueResult is null || rightValueResult.WasFailure)
        {
            // If pattern evaluation fails, cannot perform regex match.
            // Consider logging: Console.WriteLine($"Warning: RightOperand (pattern) evaluation failed for RegexMatch: {rightValueResult?.ErrorMessage}");
            return false;
        }

        object? leftValue = leftValueResult.CurrentValue;
        object? rightValue = rightValueResult.CurrentValue;

        if (leftValue is null) // An input of null typically doesn't match any pattern unless pattern specifically handles it.
        {
            return false;
        }

        if (rightValue is null || rightValue is not string patternString || string.IsNullOrEmpty(patternString))
        {
            // Pattern must be a non-empty string.
            // Consider logging: Console.WriteLine("Warning: Regex pattern is null, not a string, or empty.");
            return false;
        }

        if (leftValue is string inputString)
        {
            try
            {
                // Consider adding RegexOptions (e.g. IgnoreCase) as a configurable property of this operation.
                return Regex.IsMatch(inputString, patternString);
            }
            catch (ArgumentException ex) // Catch ArgumentException for invalid regex patterns
            {
                // Invalid regex pattern.
                throw new InvalidOperationException($"Invalid regex pattern \"{patternString}\" for {DisplayName} operation. Details: {ex.Message}", ex);
            }
        }

        // If input is not a string, regex match is not applicable.
        return false;
    }

    /// <inheritdoc />
    public override ComparisonOperationBase Clone()
    {
        var clone = new RegexMatchOperation();
        if (LeftOperand is not null)
        {
            clone.LeftOperand = LeftOperand.Clone();
        }
        if (RightOperand is not null)
        {
            clone.RightOperand = RightOperand.Clone();
        }
        return clone;
    }
}
