// Filepath: d:\git\AutomatedRealms\data-import-utility\src\DataImportUtility\AutomatedRealms.DataImportUtility.Core\Rules\FieldAccessRule.cs
using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Models;
using AutomatedRealms.DataImportUtility.Abstractions.Enums;
using AutomatedRealms.DataImportUtility.Abstractions.Interfaces; // Added for ITransformationContext
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System;

namespace AutomatedRealms.DataImportUtility.Core.Rules
{
    /// <summary>
    /// A rule that accesses a field's value from a DataRow or a list of field descriptors.
    /// The primary purpose is to retrieve a value from a specified source field.
    /// </summary>
    public class FieldAccessRule : MappingRuleBase
    {        /// <summary>
        /// Initializes a new instance of the <see cref="FieldAccessRule"/> class.
        /// </summary>
        /// <param name="sourceFieldName">The name of the field to access.</param>
        public FieldAccessRule(string sourceFieldName)
        {
            this.SourceField = sourceFieldName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldAccessRule"/> class.
        /// </summary>
        /// <param name="sourceFieldName">The name of the field to access.</param>
        /// <param name="ruleDetail">Additional information about the rule.</param>
        public FieldAccessRule(string sourceFieldName, string ruleDetail)
        {
            this.SourceField = sourceFieldName;
            this.RuleDetail = ruleDetail;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldAccessRule"/> class.
        /// Parameterless constructor for serialization.
        /// </summary>
        public FieldAccessRule() { }

        /// <summary>
        /// Gets the type of the mapping rule.
        /// </summary>
        public override MappingRuleType RuleType => MappingRuleType.FieldAccessRule;

        /// <summary>
        /// Gets the enum member name for the mapping rule type.
        /// </summary>
        public override string EnumMemberName => nameof(MappingRuleType.FieldAccessRule);

        /// <summary>
        /// Gets the display name of the mapping rule.
        /// </summary>
        public override string DisplayName => "Field Access";

        /// <summary>
        /// Gets the short name of the mapping rule.
        /// </summary>
        public override string ShortName => "Access";

        /// <summary>
        /// Gets the description of the mapping rule.
        /// </summary>
        [JsonIgnore]
        public override string Description => "Accesses the value of a specified source field.";

        /// <summary>
        /// Indicates whether the mapping rule is empty or not configured.
        /// For a FieldAccessRule, it's empty if the SourceField is not specified.
        /// </summary>
        [JsonIgnore]
        public override bool IsEmpty => string.IsNullOrEmpty(this.SourceField);

        /// <summary>
        /// Gets the <see cref="MappingRuleType"/> enum value for this rule.
        /// </summary>
        /// <returns>The <see cref="MappingRuleType.FieldAccessRule"/> enum value.</returns>
        public override MappingRuleType GetEnumValue() => this.RuleType;

        /// <summary>
        /// Gets the order value for the enum member.
        /// </summary>
        public override int EnumMemberOrder => 3;

        /// <summary>
        /// Applies the rule to a DataRow. This overload extracts the field value directly from the row.
        /// </summary>
        /// <param name="dataRow">The DataRow to apply the rule to.</param>
        /// <returns>A TransformationResult containing the field's value or a failure if the field is not found.</returns>
        public override async Task<TransformationResult?> Apply(DataRow dataRow)
        {
            var context = TransformationResult.Success(
                originalValue: null, // Original value is what we are trying to fetch
                originalValueType: null,
                currentValue: null, // Current value will be the fetched value
                currentValueType: null,
                appliedTransformations: new List<string>(),
                record: dataRow,
                tableDefinition: null, // Potentially ParentTableDefinition if available and relevant
                sourceRecordContext: null, // DataRow is the primary source here
                targetFieldType: null // Target type will be inferred from the source field
            );
            return await Apply(context);
        }

        /// <summary>
        /// Applies the field access rule using the provided transformation context.
        /// </summary>
        /// <param name="context">The transformation context.</param>
        /// <returns>A transformation result containing the accessed value, or a failure result.</returns>
        public override Task<TransformationResult?> Apply(ITransformationContext context)
        {
            if (string.IsNullOrEmpty(this.SourceField))
            {
                return Task.FromResult<TransformationResult?>(TransformationResult.Failure(
                    originalValue: null,
                    targetType: context.TargetFieldType ?? typeof(object),
                    errorMessage: "FieldAccessRule is not configured: SourceField is missing.",
                    record: context.Record,
                    tableDefinition: context.TableDefinition,
                    sourceRecordContext: context.SourceRecordContext,
                    explicitTargetFieldType: context.TargetFieldType
                ));
            }

            object? value = null;
            Type? valueType = null;
            bool found = false;

            if (context.Record != null)
            {
                if (context.Record.Table.Columns.Contains(this.SourceField))
                {
                    value = context.Record[this.SourceField];
                    valueType = value?.GetType();
                    found = true;
                }
                else
                {
                    return Task.FromResult<TransformationResult?>(TransformationResult.Failure(
                        originalValue: null,
                        targetType: context.TargetFieldType ?? typeof(object),
                        errorMessage: $"Field '{this.SourceField}' not found in DataRow.",
                        record: context.Record,
                        tableDefinition: context.TableDefinition,
                        sourceRecordContext: context.SourceRecordContext,
                        explicitTargetFieldType: context.TargetFieldType
                    ));
                }
            }
            else if (context.SourceRecordContext != null)
            {
                var fieldDesc = context.SourceRecordContext.FirstOrDefault(f => f.FieldName == this.SourceField);
                if (fieldDesc != null)
                {
                    value = fieldDesc.ValueSet?.FirstOrDefault();
                    valueType = fieldDesc.FieldType ?? value?.GetType();
                    found = true;
                }
                else
                {
                    return Task.FromResult<TransformationResult?>(TransformationResult.Failure(
                        originalValue: null,
                        targetType: context.TargetFieldType ?? typeof(object),
                        errorMessage: $"Field '{this.SourceField}' not found in SourceRecordContext.",
                        record: context.Record,
                        tableDefinition: context.TableDefinition,
                        sourceRecordContext: context.SourceRecordContext,
                        explicitTargetFieldType: context.TargetFieldType
                    ));
                }
            }

            if (found)
            {
                return Task.FromResult<TransformationResult?>(TransformationResult.Success(
                    originalValue: value, // The accessed value is the original in this context
                    originalValueType: valueType,
                    currentValue: value,
                    currentValueType: valueType,
                    appliedTransformations: new List<string> { $"Accessed field '{this.SourceField}'." },
                    record: context.Record,
                    tableDefinition: context.TableDefinition,
                    sourceRecordContext: context.SourceRecordContext,
                    targetFieldType: context.TargetFieldType ?? valueType // Use actual value type if target not specified
                ));
            }
            
            // If neither DataRow nor SourceRecordContext is available, or field not found in available context
            return Task.FromResult<TransformationResult?>(TransformationResult.Failure(
                originalValue: null,
                targetType: context.TargetFieldType ?? typeof(object),
                errorMessage: "FieldAccessRule requires a DataRow or SourceRecordContext containing the SourceField.",
                record: context.Record,
                tableDefinition: context.TableDefinition,
                sourceRecordContext: context.SourceRecordContext,
                explicitTargetFieldType: context.TargetFieldType
            ));
        }

        /// <summary>
        /// Applies the field access rule to all rows in the provided data table.
        /// </summary>
        /// <param name="data">The data table to apply the rule to.</param>
        /// <returns>An enumerable collection of transformation results for each row.</returns>
        public override async Task<IEnumerable<TransformationResult?>> Apply(DataTable data)
        {
            if (data == null) return new List<TransformationResult?>();

            if (string.IsNullOrEmpty(this.SourceField))
            {
                var failureTemplate = TransformationResult.Failure(
                    originalValue: null, 
                    targetType: typeof(object), 
                    errorMessage: "FieldAccessRule is not configured: SourceField is missing.",
                    explicitTargetFieldType: typeof(object)
                    );
                return Enumerable.Repeat(failureTemplate, data.Rows.Count).Cast<TransformationResult?>();
            }

            var results = new List<TransformationResult?>();
            foreach (DataRow row in data.Rows)
            {
                // Create context for each row
                var rowContext = TransformationResult.Success(
                    originalValue: null, originalValueType: null,
                    currentValue: null, currentValueType: null,
                    appliedTransformations: new List<string>(),
                    record: row,
                    tableDefinition: null, // Or resolve from ParentTableDefinition or data.ExtendedProperties
                    sourceRecordContext: null,
                    targetFieldType: null // Will be inferred by Apply(ITransformationContext)
                );
                results.Add(await Apply(rowContext).ConfigureAwait(false));
            }
            return results;
        }

        /// <summary>
        /// Applies the field access rule in a context where no specific data table or row is provided initially.
        /// </summary>
        /// <returns>A collection containing a failure result, as context is required.</returns>
        public override async Task<IEnumerable<TransformationResult?>> Apply()
        {
            var emptyContext = TransformationResult.Success(
                originalValue: null, originalValueType: null,
                currentValue: null, currentValueType: null,
                appliedTransformations: new List<string>(),
                record: null,
                tableDefinition: null,
                sourceRecordContext: null,
                targetFieldType: null
            );
            var result = await Apply(emptyContext);
            return new List<TransformationResult?> { result };
        }

        /// <summary>
        /// Gets the value from a source record represented by a list of field descriptors.
        /// </summary>
        /// <param name="sourceRecordContextList">The source record, as a list of <see cref="ImportedRecordFieldDescriptor"/>.</param>
        /// <param name="targetField">The descriptor of the target field (used to determine target type if needed).</param>
        /// <returns>A <see cref="TransformationResult"/> containing the accessed value.</returns>
        public override TransformationResult GetValue(List<ImportedRecordFieldDescriptor> sourceRecordContextList, ImportedRecordFieldDescriptor targetField)
        {
            Type effectiveTargetType = targetField?.FieldType ?? typeof(object);

            var context = TransformationResult.Success(
                originalValue: null, 
                originalValueType: null,
                currentValue: null, 
                currentValueType: null, 
                appliedTransformations: new List<string>(),
                record: null, // No DataRow in this specific GetValue signature.
                tableDefinition: null, 
                sourceRecordContext: sourceRecordContextList,
                targetFieldType: effectiveTargetType
            );

            var task = Apply(context); 
            TransformationResult? result = task.ConfigureAwait(false).GetAwaiter().GetResult();

            return result ?? TransformationResult.Failure(
                originalValue: null,
                targetType: effectiveTargetType,
                errorMessage: "Failed to get value using FieldAccessRule, Apply returned null.",
                sourceRecordContext: sourceRecordContextList,
                explicitTargetFieldType: effectiveTargetType
            );
        }

        /// <summary>
        /// Creates a clone of this <see cref="FieldAccessRule"/> instance.
        /// </summary>
        /// <returns>A new <see cref="FieldAccessRule"/> instance with the same configuration.</returns>
        public override MappingRuleBase Clone()
        {
            var clone = new FieldAccessRule();
            base.CloneBaseProperties(clone);
            return clone;
        }
    }
}
