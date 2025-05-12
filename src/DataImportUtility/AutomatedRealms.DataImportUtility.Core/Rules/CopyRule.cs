using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AutomatedRealms.DataImportUtility.Abstractions.Enums;
using AutomatedRealms.DataImportUtility.Abstractions.Models;
using AutomatedRealms.DataImportUtility.Abstractions.Interfaces;

namespace AutomatedRealms.DataImportUtility.Core.Rules
{
    /// <summary>
    /// Implementation of a rule that copies a value from a source field,
    /// potentially applying transformations and conditional logic.
    /// </summary>
    public class CopyRule : MappingRuleBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CopyRule"/> class.
        /// </summary>
        /// <param name="sourceFieldName">The name of the source field to copy the value from.</param>
        public CopyRule(string sourceFieldName)
        {
            this.SourceField = sourceFieldName;
        }

        /// <summary>
        /// Gets the order value for the enum member.
        /// </summary>
        public override int EnumMemberOrder => 1;

        /// <summary>
        /// Gets the display name of the mapping rule.
        /// </summary>
        public override string DisplayName => $"Copy from '{this.SourceField ?? "[undefined]"}'";

        /// <summary>
        /// Gets the description of the mapping rule.
        /// </summary>
        public override string Description => $"Copies the value from the source field '{this.SourceField ?? "[undefined]"}'.";
        
        /// <summary>
        /// Gets the type of the mapping rule.
        /// </summary>
        public override MappingRuleType RuleType => MappingRuleType.CopyRule;

        /// <summary>
        /// Gets the enum member name for the mapping rule type.
        /// </summary>
        public override string EnumMemberName => nameof(CopyRule);

        /// <summary>
        /// Gets the short name of the mapping rule.
        /// </summary>
        public override string ShortName => "Copy";

        /// <summary>
        /// Indicates whether the mapping rule is empty or not configured.
        /// For a CopyRule, it's empty if the SourceField is not set.
        /// </summary>
        public override bool IsEmpty => string.IsNullOrEmpty(this.SourceField);

        /// <summary>
        /// Gets the <see cref="MappingRuleType"/> enum value for this rule.
        /// </summary>
        /// <returns>The <see cref="MappingRuleType.CopyRule"/> enum value.</returns>
        public override MappingRuleType GetEnumValue() => this.RuleType;

        /// <summary>
        /// Applies the mapping rule to the provided data row.
        /// </summary>
        /// <param name="dataRow">The data row to apply the rule to.</param>
        /// <returns>A transformation result, or null if the rule could not be applied.</returns>
        public override async Task<TransformationResult?> Apply(DataRow dataRow)
        {
            if (string.IsNullOrEmpty(SourceField))
            {
                return TransformationResult.Failure(null, typeof(object), "Source field name is not configured for CopyRule.", originalValueType: null, record: dataRow, tableDefinition: null);
            }

            if (dataRow == null || !dataRow.Table.Columns.Contains(SourceField))
            {
                return TransformationResult.Failure(null, typeof(object), $"Source field '{SourceField}' not found in DataRow or DataRow is null.", originalValueType: null, record: dataRow, tableDefinition: null);
            }

            await Task.CompletedTask;

            object? sourceValue = dataRow[SourceField];
            Type? sourceValueType = sourceValue?.GetType();
            
            TransformationResult currentProcessingResult = TransformationResult.Success(
                originalValue: sourceValue,
                originalValueType: sourceValueType,
                currentValue: sourceValue,
                currentValueType: sourceValueType,
                appliedTransformations: new List<string>(),
                record: dataRow,
                tableDefinition: null 
            );

            return currentProcessingResult;
        }

        /// <summary>
        /// Applies the mapping rule using the provided transformation context.
        /// </summary>
        /// <param name="context">The transformation context, which includes the DataRow and TableDefinition.</param>
        /// <returns>A transformation result, or null if the rule could not be applied.</returns>
        public override async Task<TransformationResult?> Apply(ITransformationContext context)
        {
            if (string.IsNullOrEmpty(SourceField))
            {
                return TransformationResult.Failure(null, typeof(object), "Source field name is not configured for CopyRule.", 
                    originalValueType: null, 
                    record: (context as TransformationResult)?.Record, 
                    tableDefinition: (context as TransformationResult)?.TableDefinition);
            }

            var transformationContext = context as TransformationResult;
            DataRow? dataRow = transformationContext?.Record;
            ImportTableDefinition? tableDefinition = transformationContext?.TableDefinition;

            if (dataRow == null || !dataRow.Table.Columns.Contains(SourceField))
            {
                return TransformationResult.Failure(null, typeof(object), $"Source field '{SourceField}' not found in DataRow or DataRow is null.", 
                    originalValueType: null, 
                    record: dataRow, 
                    tableDefinition: tableDefinition);
            }

            await Task.CompletedTask;

            object? sourceValue = dataRow[SourceField];
            Type? sourceValueType = sourceValue?.GetType();
            
            TransformationResult currentProcessingResult = TransformationResult.Success(
                originalValue: sourceValue,
                originalValueType: sourceValueType,
                currentValue: sourceValue,
                currentValueType: sourceValueType,
                appliedTransformations: new List<string>(),
                record: dataRow,
                tableDefinition: tableDefinition 
            );

            return currentProcessingResult;
        }

        /// <summary>
        /// Applies the mapping rule to all rows in the provided data table.
        /// </summary>
        /// <param name="data">The data table to apply the rule to.</param>
        /// <returns>An enumerable collection of transformation results for each row.</returns>
        public override async Task<IEnumerable<TransformationResult?>> Apply(DataTable data)
        {
            if (data == null) return new List<TransformationResult?>();
            var results = new List<TransformationResult?>();
            foreach (DataRow row in data.Rows)
            {
                var rowContext = TransformationResult.Success(null, null, null, null, record: row, tableDefinition: null);
                results.Add(await Apply(rowContext).ConfigureAwait(false)); 
            }
            return results;
        }

        /// <summary>
        /// Applies the mapping rule in a context where no specific data table or row is provided.
        /// This might imply a default or pre-configured source. For CopyRule, this is typically not used without data.
        /// </summary>
        /// <returns>An enumerable collection of transformation results.</returns>
        public override Task<IEnumerable<TransformationResult?>> Apply()
        {
            return Task.FromResult(new List<TransformationResult?>().AsEnumerable());
        }

        /// <summary>
        /// Gets the value from a source record represented by a list of field descriptors.
        /// </summary>
        /// <param name="sourceRecord">The source record, represented as a list of <see cref="ImportedRecordFieldDescriptor"/>.</param>
        /// <param name="targetField">The descriptor of the target field (used to determine target type if needed).</param>
        /// <returns>A <see cref="TransformationResult"/> containing the copied value.</returns>
        public override TransformationResult GetValue(List<ImportedRecordFieldDescriptor> sourceRecord, ImportedRecordFieldDescriptor targetField)
        {
            Type targetValueType = targetField?.FieldType ?? typeof(object);

            if (string.IsNullOrEmpty(SourceField))
            {
                return TransformationResult.Failure(null, targetValueType, "Source field name is not configured for CopyRule.");
            }

            var sourceFieldDescriptor = sourceRecord?.FirstOrDefault(f => f.FieldName == SourceField);
            if (sourceFieldDescriptor == null)
            {
                return TransformationResult.Failure(null, targetValueType, $"Source field '{SourceField}' not found in source record.");
            }

            object? val = sourceFieldDescriptor.ValueSet?.FirstOrDefault();
            Type valType = sourceFieldDescriptor.FieldType ?? (val?.GetType() ?? typeof(object));

            return TransformationResult.Success(val, valType, val, valType, new List<string>());
        }

        /// <summary>
        /// Creates a clone of this <see cref="CopyRule"/> instance.
        /// </summary>
        /// <returns>A new <see cref="CopyRule"/> instance with the same configuration.</returns>
        public override MappingRuleBase Clone()
        {
            var clone = new CopyRule(this.SourceField!);
            base.CloneBaseProperties(clone);
            return clone;
        }
    }
}
