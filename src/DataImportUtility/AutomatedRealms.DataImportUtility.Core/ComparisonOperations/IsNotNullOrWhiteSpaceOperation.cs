using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Enums;
using AutomatedRealms.DataImportUtility.Abstractions.Models;
using AutomatedRealms.DataImportUtility.Abstractions.Interfaces; // Added for ITransformationContext
using System.Threading.Tasks;
using System;

namespace AutomatedRealms.DataImportUtility.Core.ComparisonOperations
{
    /// <summary>
    /// Operation to check if a value is not null or white space.
    /// </summary>
    public class IsNotNullOrWhiteSpaceOperation : ComparisonOperationBase
    {
        /// <summary>
        /// Gets the display name of the operation.
        /// </summary>
        public override string DisplayName => "Is Not Null or WhiteSpace";

        /// <summary>
        /// Gets the description of the operation.
        /// </summary>
        public override string Description => "Checks if the input value is not null, empty, and does not consist only of white-space characters.";

        /// <summary>
        /// Gets the enum member name as a string.
        /// </summary>
        public override string EnumMemberName => ComparisonOperationType.IsNotNullOrWhiteSpace.ToString();

        /// <summary>
        /// Initializes a new instance of the <see cref="IsNotNullOrWhiteSpaceOperation"/> class.
        /// </summary>
        public IsNotNullOrWhiteSpaceOperation() : base()
        {
        }

        /// <summary>
        /// Evaluates whether the left operand's value is not null and does not consist only of white-space characters.
        /// </summary>
        /// <param name="contextResult">The transformation context.</param>
        /// <returns>True if the value is not null or white space, otherwise false.</returns>
        public override async Task<bool> Evaluate(TransformationResult contextResult) // contextResult is an ITransformationContext
        {
            if (LeftOperand == null)
            {
                throw new InvalidOperationException($"{nameof(LeftOperand)} must be set for {DisplayName} operation.");
            }

            var leftValueResult = await LeftOperand.Apply(contextResult);

            if (leftValueResult == null || leftValueResult.WasFailure)
            {
                throw new InvalidOperationException($"Failed to evaluate {nameof(LeftOperand)} for {DisplayName} operation: {leftValueResult?.ErrorMessage ?? "Result was null."}");
            }

            object? leftValue = leftValueResult.CurrentValue;

            if (leftValue == null)
            {
                return false; // Null is considered null or whitespace
            }

            if (leftValue is string stringValue)
            {
                return !string.IsNullOrWhiteSpace(stringValue);
            }

            // If it's not a string and not null, it is considered not null or whitespace.
            return true;
        }
    }
}
