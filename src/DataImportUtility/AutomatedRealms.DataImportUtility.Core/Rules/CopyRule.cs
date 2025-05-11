using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Enums;
using AutomatedRealms.DataImportUtility.Abstractions.Models;
using AutomatedRealms.DataImportUtility.Core.ComparisonOperations;

namespace AutomatedRealms.DataImportUtility.Core.Rules
{
    /// <summary>
    /// Implementation of a rule that copies a value from a source field,
    /// potentially applying transformations and conditional logic.
    /// </summary>
    public class CopyRule : MappingRuleBase
    {
        private readonly string _sourceFieldName;

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyRule"/> class.
        /// </summary>
        /// <param name="sourceFieldName">The name of the source field to copy the value from.</param>
        public CopyRule(string sourceFieldName)
        {
            _sourceFieldName = sourceFieldName;
        }

        /// <inheritdoc/>
        public override string DisplayName => $"Copy from '{_sourceFieldName}'";

        /// <inheritdoc/>
        public override string Description => $"Copies the value from the source field '{_sourceFieldName}'.";

        /// <inheritdoc/>
        public override RuleType RuleType => RuleType.CopyRule;

        /// <inheritdoc/>
        public override string EnumMemberName => "CopyRule";

        /// <inheritdoc/>
        public override bool IsEmpty => string.IsNullOrEmpty(_sourceFieldName);

        /// <inheritdoc/>
        public override async Task<TransformationResult> GetValue(DataRow row, Type targetType)
        {
            if (string.IsNullOrEmpty(_sourceFieldName))
            {
                // Ensure error message refers to CopyRule
                return TransformationResult.Failure(null, targetType, "Source field name is not configured for CopyRule.", originalValueType: null, record: row, tableDefinition: this.ParentTableDefinition);
            }

            if (!row.Table.Columns.Contains(_sourceFieldName))
            {
                return TransformationResult.Failure(null, targetType, $"Source field '{_sourceFieldName}' not found in DataRow.", originalValueType: null, record: row, tableDefinition: this.ParentTableDefinition);
            }

            // Check conditional rules first
            bool conditionsMet = await AreConditionalRulesMetAsync(row).ConfigureAwait(false);
            if (!conditionsMet)
            {
                return TransformationResult.Failure(
                    originalValue: row[_sourceFieldName], 
                    targetType: targetType, 
                    errorMessage: "Conditional rules not met.", 
                    originalValueType: row[_sourceFieldName]?.GetType(), 
                    record: row, 
                    tableDefinition: this.ParentTableDefinition);
            }

            object? sourceValue = row[_sourceFieldName];
            Type? sourceValueType = sourceValue?.GetType();
            
            TransformationResult currentProcessingResult = TransformationResult.Success(
                originalValue: sourceValue,
                originalValueType: sourceValueType,
                currentValue: sourceValue,
                currentValueType: sourceValueType,
                appliedTransformations: new List<string>(),
                record: row,
                tableDefinition: this.ParentTableDefinition 
            );

            var specificFieldTransformation = this.SourceFieldTransformations
                .FirstOrDefault(ft => ft.Field?.FieldName == _sourceFieldName);

            if (specificFieldTransformation != null)
            {
                if (specificFieldTransformation.ValueTransformations != null)
                {
                    foreach (var transformation in specificFieldTransformation.ValueTransformations)
                    {
                        if (transformation == null) continue;

                        currentProcessingResult = await transformation.ApplyTransformationAsync(currentProcessingResult);

                        if (currentProcessingResult.WasFailure)
                        {
                            return currentProcessingResult; 
                        }
                    }
                }
            }

            object? valueAfterTransformations = currentProcessingResult.CurrentValue;
            Type? typeAfterTransformations = currentProcessingResult.CurrentValueType;

            try
            {
                object? finalValue = valueAfterTransformations;
                Type? finalValueType = typeAfterTransformations;

                if (targetType != null) 
                {
                    if (valueAfterTransformations == null)
                    {
                        if (targetType.IsValueType && Nullable.GetUnderlyingType(targetType) == null)
                        {
                            return TransformationResult.Failure(
                                originalValue: sourceValue, targetType: targetType,
                                errorMessage: $"Cannot assign null to non-nullable target type '{targetType.Name}'.",
                                originalValueType: sourceValueType, currentValueType: typeAfterTransformations,
                                appliedTransformations: currentProcessingResult.AppliedTransformations, record: row, tableDefinition: this.ParentTableDefinition);
                        }
                        finalValueType = targetType; 
                    }
                    else if (typeAfterTransformations == null || !targetType.IsAssignableFrom(typeAfterTransformations))
                    {
                        finalValue = Convert.ChangeType(valueAfterTransformations, targetType);
                        finalValueType = targetType;
                    }
                }
                
                return TransformationResult.Success(
                    originalValue: sourceValue, originalValueType: sourceValueType,
                    currentValue: finalValue, currentValueType: finalValueType,
                    appliedTransformations: currentProcessingResult.AppliedTransformations, record: row, tableDefinition: this.ParentTableDefinition);
            }
            catch (Exception ex)
            {
                return TransformationResult.Failure(
                    originalValue: sourceValue, targetType: targetType,
                    errorMessage: $"Failed to convert value from type '{typeAfterTransformations?.Name ?? "unknown"}' to target type '{targetType?.Name ?? "unknown"}': {ex.Message}",
                    originalValueType: sourceValueType, currentValueType: typeAfterTransformations,
                    appliedTransformations: currentProcessingResult.AppliedTransformations, record: row, tableDefinition: this.ParentTableDefinition);
            }
        }

        /// <inheritdoc/>
        protected override async Task<ComparisonOperationBase?> GetConfiguredOperationAsync(ConditionalRule conditionalRule, DataRow row)
        {
            if (conditionalRule == null)
            {
                return null;
            }
            return await ComparisonOperationFactory.CreateOperationAsync(conditionalRule, row, this.ParentTableDefinition, $"ConditionalRule_for_CopyRule_{_sourceFieldName}");
        }

        // LogOperandMissing method removed as logging is handled by ComparisonOperationFactory
    }
}
