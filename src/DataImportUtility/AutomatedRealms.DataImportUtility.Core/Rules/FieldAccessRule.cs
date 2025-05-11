// Filepath: d:\git\AutomatedRealms\data-import-utility\src\DataImportUtility\AutomatedRealms.DataImportUtility.Core\Rules\FieldAccessRule.cs
using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Models;
using AutomatedRealms.DataImportUtility.Abstractions.Enums; // Required for ConditionalRule if used in GetConfiguredOperationAsync
using System.Collections.Immutable;
using System.Data;
using System.Threading.Tasks;

namespace AutomatedRealms.DataImportUtility.Core.Rules
{
    /// <summary>
    /// A rule that accesses a field's value from a DataRow within a TransformationResult context.
    /// </summary>
    public class FieldAccessRule : MappingRuleBase
    {
        private readonly string _fieldName;

        /// <summary>
        /// Gets or sets the name of the rule.
        /// </summary>
        public override string RuleName { get; set; }

        /// <summary>
        /// Gets the display name of the rule.
        /// </summary>
        public override string DisplayName => "Field Access Rule";

        /// <summary>
        /// Gets the description of the rule.
        /// </summary>
        public override string Description => "Accesses a specified field's value from the input DataRow via TransformationResult.Record.";

        /// <summary>
        /// Gets or sets the source field transformations. Not typically used by this rule.
        /// </summary>
        public override ImmutableList<FieldTransformation> SourceFieldTransformations { get; set; } = ImmutableList<FieldTransformation>.Empty;

        /// <summary>
        /// Gets or sets the target field. Not typically used by this rule.
        /// </summary>
        public override ImportedRecordFieldDescriptor? TargetField { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldAccessRule"/> class.
        /// </summary>
        /// <param name="fieldName">The name of the field to access.</param>
        /// <param name="ruleName">The name of this rule instance.</param>
        public FieldAccessRule(string fieldName, string ruleName = "FieldAccessRule")
        {
            _fieldName = fieldName;
            RuleName = ruleName;
        }

        /// <summary>
        /// Applies the rule to a DataRow. This overload extracts the field value directly from the row.
        /// </summary>
        /// <param name="row">The DataRow to apply the rule to.</param>
        /// <returns>A TransformationResult containing the field's value or a failure if the field is not found.</returns>
        public override Task<TransformationResult?> Apply(DataRow row)
        {
            if (row.Table.Columns.Contains(_fieldName))
            {
                var value = row[_fieldName];
                return Task.FromResult<TransformationResult?>(
                    TransformationResult.Success(value, value?.GetType(), value, value?.GetType(), RuleName, GetType().Name, row)
                );
            }
            return Task.FromResult<TransformationResult?>(
                TransformationResult.Failure(null, typeof(object), $"Field '{_fieldName}' not found in DataRow.", null, RuleName, GetType().Name, row)
            );
        }

        /// <summary>
        /// Applies the rule using a TransformationResult as context. This overload extracts the field value from sourceResult.Record.
        /// </summary>
        /// <param name="sourceResult">The source TransformationResult containing the DataRow in its Record property.</param>
        /// <returns>A TransformationResult containing the field's value or a failure.</returns>
        public override Task<TransformationResult> Apply(TransformationResult sourceResult)
        {
            if (sourceResult.Record == null)
            {
                return Task.FromResult(
                    TransformationResult.Failure(sourceResult.CurrentValue, sourceResult.CurrentValueType, "DataRow (Record) is null in sourceResult for FieldAccessRule.", sourceResult.OriginalValueType, RuleName, GetType().Name, sourceResult.Record, sourceResult.TableDefinition)
                );
            }
            if (sourceResult.Record.Table.Columns.Contains(_fieldName))
            {
                var value = sourceResult.Record[_fieldName];
                return Task.FromResult(
                    TransformationResult.Success(value, value?.GetType(), value, value?.GetType(), RuleName, GetType().Name, sourceResult.Record, sourceResult.TableDefinition)
                );
            }
            return Task.FromResult(
                TransformationResult.Failure(sourceResult.CurrentValue, sourceResult.CurrentValueType, $"Field '{_fieldName}' not found in DataRow from sourceResult.Record for FieldAccessRule.", sourceResult.OriginalValueType, RuleName, GetType().Name, sourceResult.Record, sourceResult.TableDefinition)
            );
        }

        /// <summary>
        /// Gets the configured comparison operation. FieldAccessRule does not use conditional rules itself.
        /// </summary>
        /// <param name="conditionalRule">The conditional rule.</param>
        /// <param name="row">The DataRow.</param>
        /// <returns>Null, as this rule does not process conditional rules.</returns>
        protected override Task<ComparisonOperationBase?> GetConfiguredOperationAsync(ConditionalRule conditionalRule, DataRow row)
        {
            return Task.FromResult<ComparisonOperationBase?>(null);
        }
    }
}
