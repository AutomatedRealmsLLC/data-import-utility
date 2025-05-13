using System.Globalization; // Added for CultureInfo.InvariantCulture
using System.Text.Json.Serialization;

using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Models; // For TransformationResult and ITransformationContext

namespace AutomatedRealms.DataImportUtility.Core.ComparisonOperations;

/// <summary>
/// Checks if a value is true.
/// </summary>
public class IsTrueOperation : ComparisonOperationBase
{
    /// <summary>
    /// Static TypeId for this operation.
    /// </summary>
    public static readonly string TypeIdString = "Core.IsTrue";

    /// <inheritdoc />
    [JsonIgnore]
    public override string DisplayName { get; } = "Is true";

    /// <inheritdoc />
    [JsonIgnore]
    public override string Description { get; } = "Checks if a value is considered true (e.g., boolean true, non-zero number, or 'true' string).";

    /// <summary>
    /// Initializes a new instance of the <see cref="IsTrueOperation"/> class.
    /// </summary>
    public IsTrueOperation() : base(TypeIdString)
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
    /// Evaluates whether the left operand's value is considered true.
    /// A value is considered true if it's a boolean true, a non-zero number, or the string "true" (case-insensitive).
    /// Null, empty/whitespace strings, boolean false, zero, or other string values are considered false.
    /// </summary>
    /// <param name="contextResult">The transformation context.</param>
    /// <returns>True if the value is considered true, otherwise false.</returns>
    public override async Task<bool> Evaluate(TransformationResult contextResult)
    {
        ITransformationContext? context = contextResult as ITransformationContext;
        if (context == null)
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
            throw new InvalidOperationException($"{nameof(LeftOperand)} must be configured for {DisplayName} operation.");
        }

        var leftResult = await LeftOperand.Apply(context);

        if (leftResult == null)
        {
            throw new InvalidOperationException($"Applying {nameof(LeftOperand)} for {DisplayName} operation returned null unexpectedly.");
        }

        if (leftResult.WasFailure)
        {
            throw new InvalidOperationException($"Failed to evaluate {nameof(LeftOperand)} for {DisplayName} operation: {leftResult.ErrorMessage}");
        }

        object? value = leftResult.CurrentValue;

        if (value is null)
        {
            return false; // Null is not true
        }

        if (value is bool boolValue)
        {
            return boolValue;
        }

        if (value is string stringValue)
        {
            if (string.IsNullOrWhiteSpace(stringValue))
            {
                return false; // Empty or whitespace string is not true
            }
            // bool.TryParse handles "true"/"false" case-insensitively
            if (bool.TryParse(stringValue, out var parsedBool))
            {
                return parsedBool;
            }
            // Check for numeric interpretations if not parsed as bool
            if (double.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var numberValue))
            {
                return numberValue != 0;
            }
            return false; // String is not recognizably true (and not a valid bool string like "True")
        }

        // Handle numeric types directly (e.g., int, double, decimal)
        if (value is sbyte || value is byte || value is short || value is ushort || value is int || value is uint || value is long || value is ulong || value is float || value is double || value is decimal)
        {
            try
            {
                return Convert.ToDouble(value, CultureInfo.InvariantCulture) != 0;
            }
            catch (OverflowException)
            {
                // If the number is too large/small for a double but is non-zero, it's true.
                return true; 
            }
            catch (InvalidCastException)
            {
                return false;
            }
        }

        // For any other type, it's not considered true in this context.
        return false;
    }

    /// <inheritdoc />
    public override ComparisonOperationBase Clone()
    {
        var clone = new IsTrueOperation();
        if (LeftOperand != null)
        {
            clone.LeftOperand = LeftOperand.Clone();
        }
        // RightOperand is not used by this operation.
        return clone;
    }
}
