using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Enums;
using AutomatedRealms.DataImportUtility.Abstractions.Models;
using System.Threading.Tasks;

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
        /// <param name="transformationResult">The transformation result context, which includes the DataRow for operand resolution.</param>
        /// <returns>True if the value is not null or empty, otherwise false.</returns>
        public override async Task<bool> Evaluate(TransformationResult transformationResult)
        {
            if (LeftOperand == null)
            {
                // Consider logging an error or specific handling for misconfiguration
                return false; 
            }

            if (transformationResult.Record == null)
            {
                // Cannot evaluate if Record is null, as FieldAccessRule would need it.
                // This might depend on whether LeftOperand can ever be a StaticValueRule here.
                // For now, assume if Record is null, we can't proceed if LeftOperand needs it.
                // If LeftOperand is a StaticValueRule, it wouldn't use Record, but GetValue still takes it.
                // A more robust solution might involve checking the type of LeftOperand or if it requires Record.
                return false; // Or throw new InvalidOperationException("DataRow (Record) is null in TransformationResult for conditional evaluation.");
            }

            var leftValueResult = await LeftOperand.GetValue(transformationResult.Record, typeof(string));
            if (leftValueResult.WasFailure)
            {
                // Handle failure to get value, e.g., log or return false based on requirements
                return false;
            }

            object? leftValue = leftValueResult.CurrentValue; // Corrected: Use CurrentValue

            if (leftValue == null)
            {
                return false; // Null is considered empty
            }

            if (leftValue is string stringValue)
            {
                return !string.IsNullOrEmpty(stringValue);
            }

            // If it's not a string and not null, it is considered not null or empty.
            // This matches the previous logic.
            return true;
        }
    }
}
