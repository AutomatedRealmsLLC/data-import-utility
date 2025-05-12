using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Enums;
using AutomatedRealms.DataImportUtility.Abstractions.Models;

namespace AutomatedRealms.DataImportUtility.Core.ComparisonOperations
{
    /// <summary>
    /// Operation to check if a value is not null or empty.
    /// </summary>
    public class IsNotNullOrEmptyOperation : ComparisonOperationBase
    {
        /// <summary>
        /// Gets the display name of the operation.
        /// </summary>
        public override string DisplayName => "Is Not Null or Empty";

        /// <summary>
        /// Gets the description of the operation.
        /// </summary>
        public override string Description => "Checks if the input value is not null or an empty string.";

        /// <summary>
        /// Gets the enum member name as a string.
        /// </summary>
        public override string EnumMemberName => ComparisonOperationType.IsNotNullOrEmpty.ToString();

        /// <summary>
        /// Initializes a new instance of the <see cref="IsNotNullOrEmptyOperation"/> class.
        /// </summary>
        public IsNotNullOrEmptyOperation() : base()
        {
        }

        /// <summary>
        /// Evaluates whether the left operand's value is not null or empty.
        /// </summary>
        /// <param name="contextResult">The transformation context.</param>
        /// <returns>True if the value is not null or empty, otherwise false.</returns>
        public override async Task<bool> Evaluate(TransformationResult contextResult) // contextResult is an ITransformationContext
        {
            if (LeftOperand == null)
            {
                throw new InvalidOperationException($"{nameof(LeftOperand)} must be set for {DisplayName} operation.");
            }

            var leftValueResult = await LeftOperand.Apply(contextResult);

            if (leftValueResult == null || leftValueResult.WasFailure)
            {
                // Handle failure to get value, e.g., log or throw based on how critical this is.
                // For a conditional check, returning false or throwing might be appropriate.
                // Throwing an exception provides more details on the failure.
                throw new InvalidOperationException($"Failed to evaluate {nameof(LeftOperand)} for {DisplayName} operation: {leftValueResult?.ErrorMessage ?? "Result was null."}");
            }

            object? leftValue = leftValueResult.CurrentValue;

            if (leftValue == null)
            {
                return false; // Null is considered null or empty
            }

            if (leftValue is string stringValue)
            {
                return !string.IsNullOrEmpty(stringValue);
            }

            // If it's not a string and not null, it is considered not null or empty.
            // This behavior matches common interpretations where any non-null, non-string object is not "empty" in the string sense.
            return true;
        }
    }
}
