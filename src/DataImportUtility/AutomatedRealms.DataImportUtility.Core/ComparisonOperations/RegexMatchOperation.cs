using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Enums;
using AutomatedRealms.DataImportUtility.Abstractions.Models;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System; // Added for ArgumentException

namespace AutomatedRealms.DataImportUtility.Core.ComparisonOperations
{
    /// <summary>
    /// Operation to check if a string matches a regular expression pattern.
    /// </summary>
    public class RegexMatchOperation : ComparisonOperationBase
    {
        /// <summary>
        /// Gets the display name of the operation.
        /// </summary>
        public override string DisplayName => "Regex Match";

        /// <summary>
        /// Gets the description of the operation.
        /// </summary>
        public override string Description => "Checks if the input string matches the specified regular expression pattern.";

        /// <summary>
        /// Gets the enum member name as a string.
        /// </summary>
        public override string EnumMemberName => ComparisonOperationType.RegexMatch.ToString();

        /// <summary>
        /// Initializes a new instance of the <see cref="RegexMatchOperation"/> class.
        /// </summary>
        public RegexMatchOperation() : base()
        {
        }

        /// <summary>
        /// Evaluates whether the left operand's string value matches the regex pattern provided by the right operand.
        /// </summary>
        /// <param name="contextResult">The transformation result context.</param>
        /// <returns>True if the input string matches the pattern, otherwise false.</returns>
        public override async Task<bool> Evaluate(TransformationResult contextResult)
        {
            if (LeftOperand == null || RightOperand == null)
            {
                return false; // Misconfigured, requires both input and pattern
            }

            var leftValueResult = await LeftOperand.Apply(contextResult);
            if (leftValueResult == null || leftValueResult.WasFailure || leftValueResult.CurrentValue == null)
            {
                return false; // Failed to get input value or it's null
            }

            var rightValueResult = await RightOperand.Apply(contextResult);
            if (rightValueResult == null || rightValueResult.WasFailure || rightValueResult.CurrentValue == null)
            {
                return false; // Failed to get pattern value or it's null
            }

            if (leftValueResult.CurrentValue is string inputString && rightValueResult.CurrentValue is string patternString)
            {
                if (string.IsNullOrEmpty(patternString))
                {
                    // An empty pattern is not valid for matching.
                    return false;
                }
                try
                {
                    return Regex.IsMatch(inputString, patternString);
                }
                catch (ArgumentException) // Catch ArgumentException for invalid regex patterns
                {
                    // Invalid regex pattern, consider logging this error.
                    return false;
                }
            }

            // If types are not string, regex match is not applicable.
            return false;
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public override ComparisonOperationBase Clone()
        {
            var clone = new RegexMatchOperation();
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
}
