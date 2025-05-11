using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Enums; // Added for ComparisonOperationType
using AutomatedRealms.DataImportUtility.Abstractions.Models; 
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
            // Operands will be set by the configuring rule (e.g., CopyRuleCore)
        }

        /// <summary>
        /// Evaluates if the LeftOperand's resolved value is null or an empty string.
        /// The 'result' parameter provides context (like the DataRow) for operand resolution.
        /// </summary>
        /// <param name="transformationResult">The transformation result context, which includes the DataRow for operand resolution.</param>
        /// <returns>True if the LeftOperand's value is null or empty; otherwise, false.</returns>
        public override async Task<bool> Evaluate(TransformationResult transformationResult) // Renamed parameter for clarity
        {
            if (this.LeftOperand == null)
            {
                // This indicates a configuration error if an operation requiring a left operand doesn't have one.
                // Consider logging this or throwing a more specific configuration exception.
                throw new InvalidOperationException("LeftOperand cannot be null for IsNullOrEmptyOperation. It must be configured by the calling rule.");
            }

            if (transformationResult.Record == null)
            {
                // Cannot evaluate if Record is null, as FieldAccessRule (common LeftOperand) would need it.
                // If LeftOperand could be a StaticValueRule, this might be permissible,
                // but GetValue still expects a DataRow.
                // For now, assume if Record is null, we can't proceed.
                // Consider logging: "DataRow (Record) is null in TransformationResult for IsNullOrEmptyOperation."
                return false; // Or throw an exception if this state is truly unexpected.
            }

            // Retrieve the value from the LeftOperand.
            // The type requested (string) is a hint; GetValue should handle conversions if possible/necessary.
            var leftOperandValueResult = await this.LeftOperand.GetValue(transformationResult.Record, typeof(string));
            
            if (leftOperandValueResult.WasFailure)
            {
                // If the operand itself failed to evaluate (e.g., field not found by FieldAccessRule),
                // the condition effectively cannot be met in a 'true' sense.
                // Consider logging: leftOperandValueResult.ErrorMessage
                return false; 
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
    }
}
