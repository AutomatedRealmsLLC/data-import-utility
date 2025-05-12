using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Enums; // Added for ComparisonOperationType
using AutomatedRealms.DataImportUtility.Abstractions.Models; 
using AutomatedRealms.DataImportUtility.Abstractions.Interfaces; // Added for ITransformationContext
using System; 
using System.Threading.Tasks;

namespace AutomatedRealms.DataImportUtility.Core.ComparisonOperations
{
    /// <summary>
    /// Checks if a field's value is null or an empty string.
    /// </summary>
    public class IsNullOrEmptyOperation : ComparisonOperationBase
    {
        /// <summary>
        /// Gets the name of the enum member.
        /// </summary>
        public override string EnumMemberName => ComparisonOperationType.IsNullOrEmpty.ToString(); // Corrected

        /// <summary>
        /// Gets the display name of the operation.
        /// </summary>
        public override string DisplayName => "Is Null Or Empty";

        /// <summary>
        /// Gets the description of the operation.
        /// </summary>
        public override string Description => "Checks if the field value is null or an empty string.";

        /// <summary>
        /// Initializes a new instance of the <see cref="IsNullOrEmptyOperation"/> class.
        /// </summary>
        public IsNullOrEmptyOperation() : base()
        {
            // Operands will be set by the configuring rule
        }

        /// <summary>
        /// Evaluates if the LeftOperand's resolved value is null or an empty string.
        /// </summary>
        /// <param name="contextResult">The transformation context.</param>
        /// <returns>True if the LeftOperand's value is null or empty; otherwise, false.</returns>
        public override async Task<bool> Evaluate(TransformationResult contextResult) // contextResult is an ITransformationContext
        {
            if (this.LeftOperand == null)
            {
                throw new InvalidOperationException($"LeftOperand cannot be null for {DisplayName}. It must be configured by the calling rule.");
            }

            var leftOperandValueResult = await this.LeftOperand.Apply(contextResult);
            
            if (leftOperandValueResult == null)
            {
                 throw new InvalidOperationException($"Applying {nameof(LeftOperand)} for {DisplayName} operation returned null unexpectedly.");
            }

            if (leftOperandValueResult.WasFailure)
            {
                // If the operand evaluation failed, we cannot determine if it's null or empty.
                // Throwing an exception provides more details on the failure.
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
            // E.g., an integer value of 0 is not an empty string.
            return false;
        }

        // Clone method is inherited from ComparisonOperationBase
    }
}
