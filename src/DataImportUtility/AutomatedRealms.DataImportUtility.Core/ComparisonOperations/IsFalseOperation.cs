using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Models; // For TransformationResult and ITransformationContext

using System.Text.Json.Serialization;

namespace AutomatedRealms.DataImportUtility.Core.ComparisonOperations;

/// <summary>
/// Checks if a value is false.
/// </summary>
public class IsFalseOperation : ComparisonOperationBase
{
    /// <summary>
    /// Static TypeId for this operation.
    /// </summary>
    public static readonly string TypeIdString = "Core.IsFalse";

    /// <inheritdoc />
    [JsonIgnore]
    public override string DisplayName { get; } = "Is false";

    /// <inheritdoc />
    [JsonIgnore]
    public override string Description { get; } = "Checks if a value is considered false (e.g., null, empty string, boolean false, 0, or 'false' string).";

    /// <summary>
    /// Initializes a new instance of the <see cref="IsFalseOperation"/> class.
    /// </summary>
    public IsFalseOperation() : base(TypeIdString)
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
        if (rightOperand != null)
        {
            // Or log a warning: Console.WriteLine($"Warning: RightOperand is provided but not used by {DisplayName}.");
        }
        if (secondaryRightOperand != null)
        {
            // Or log a warning
        }
    }

    /// <inheritdoc />
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
            throw new InvalidOperationException($"{nameof(LeftOperand)} must be configured for {DisplayName} operation.");
        }

        var leftResult = await LeftOperand.Apply(context);

        if (leftResult == null || leftResult.WasFailure)
        {
            // If evaluation of the operand fails, it cannot be determined if it's false.
            // Depending on desired behavior, this could return false or throw.
            // Throwing an exception is consistent with other operations when operands fail.
            throw new InvalidOperationException($"Failed to evaluate {nameof(LeftOperand)} for {DisplayName} operation: {leftResult?.ErrorMessage ?? "Result was null."}");
        }

        // Inlined logic from former IsFalseOperationExtensions.IsFalse
        if (leftResult.CurrentValue == null) return true;
        if (leftResult.CurrentValue is bool boolVal) return !boolVal;

        var stringVal = leftResult.CurrentValue.ToString();
        if (string.IsNullOrEmpty(stringVal)) return true;
        if (string.Equals(stringVal, "false", StringComparison.OrdinalIgnoreCase)) return true;
        if (stringVal == "0") return true;

        // Attempt to convert to double for numeric zero check, only if it's IConvertible
        if (leftResult.CurrentValue is IConvertible convertible)
        {
            try
            {
                // Using InvariantCulture for consistent number parsing
                return convertible.ToDouble(System.Globalization.CultureInfo.InvariantCulture) == 0;
            }
            catch (FormatException)
            {
                // Not a number that can be converted to double, so not numerically 0.
            }
            catch (InvalidCastException)
            {
                // Not convertible to double.
            }
            catch (OverflowException)
            {
                // Value is too large or too small for a double, so not 0.
            }
        }

        return false;
    }

    /// <inheritdoc />
    public override ComparisonOperationBase Clone()
    {
        var clone = new IsFalseOperation
        {
            LeftOperand = LeftOperand?.Clone()
        };
        // RightOperand is not used, so no need to clone it.
        return clone;
    }
}
