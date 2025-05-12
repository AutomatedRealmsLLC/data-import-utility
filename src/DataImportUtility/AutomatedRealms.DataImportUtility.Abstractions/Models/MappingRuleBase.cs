using System.Data;
using System.Text.Json.Serialization;

using AutomatedRealms.DataImportUtility.Abstractions.Enums;

namespace AutomatedRealms.DataImportUtility.Abstractions.Models
{
    /// <summary>
    /// Base class for all mapping rules.
    /// </summary>
    //[JsonConverter(typeof(MappingRuleBaseConverter))]
    public abstract class MappingRuleBase : IMappingRule
    {
        /// <summary>
        /// The event that is raised when the definition of the rule changes.
        /// </summary>
        public event Func<Task>? OnDefinitionChanged;

        /// <summary>
        /// The unique identifier for the rule.
        /// </summary>
        public Guid Id { get; } = Guid.NewGuid();

        /// <summary>
        /// The value the generated enum member display in the MappingRuleType.
        /// </summary>
        public abstract int EnumMemberOrder { get; }

        /// <summary>
        /// The additional information for the rule.
        /// </summary>
        public string? RuleDetail { get; set; }

        /// <summary>
        /// Gets or sets the source field.
        /// </summary>
        public string? SourceField { get; set; }        /// <summary>
                                                        /// Gets or sets the target field.
                                                        /// </summary>
        public string? TargetField { get; set; }

        /// <summary>
        /// Gets or sets the parent table definition that this rule operates within.
        /// </summary>
        [JsonIgnore]
        public ImportTableDefinition? ParentTableDefinition { get; set; }

        /// <summary>
        /// Gets or sets the rule type (using MappingRuleType).
        /// </summary>
        public abstract MappingRuleType RuleType { get; }

        /// <summary>
        /// Gets the rule type for the IMappingRule interface.
        /// </summary>
        RuleType IMappingRule.RuleType => MapToRuleType(RuleType);

        /// <summary>
        /// Gets or sets the enum member name.
        /// </summary>
        public abstract string EnumMemberName { get; }

        /// <summary>
        /// Gets or sets the source field transformations.
        /// </summary>
        public List<ValueTransformationBase> SourceFieldTransformations { get; set; } = [];

        /// <summary>
        /// Gets the display name of the mapping rule.
        /// </summary>
        [JsonIgnore]
        public abstract string DisplayName { get; }

        /// <summary>
        /// Gets the description of the mapping rule.
        /// </summary>
        [JsonIgnore]
        public abstract string Description { get; }

        /// <summary>
        /// Gets the short name of the mapping rule.
        /// </summary>
        [JsonIgnore]
        public abstract string ShortName { get; }

        /// <summary>
        /// Gets the type of the mapping rule.
        /// </summary>
        /// <returns>The mapping rule type.</returns>
        public abstract MappingRuleType GetEnumValue();

        /// <summary>
        /// Indicates whether the mapping rule is empty or not configured.
        /// </summary>
        public virtual bool IsEmpty => false; // Default to false, overrides will specify actual logic

        /// <summary>
        /// Applies the mapping rule.
        /// </summary>
        /// <returns>A collection of transformation results.</returns>
        public abstract Task<IEnumerable<TransformationResult?>> Apply();

        /// <summary>
        /// Applies the mapping rule to the provided data table.
        /// </summary>
        /// <param name="data">The data table.</param>
        /// <returns>A collection of transformation results.</returns>
        public abstract Task<IEnumerable<TransformationResult?>> Apply(DataTable data);

        /// <summary>
        /// Applies the mapping rule to the provided data row.
        /// </summary>
        /// <param name="dataRow">The data row.</param>
        /// <returns>A transformation result.</returns>
        public abstract Task<TransformationResult?> Apply(DataRow dataRow);

        /// <summary>
        /// Applies the mapping rule to the provided transformation context.
        /// </summary>
        /// <param name="context">The transformation context.</param>
        /// <returns>A transformation result.</returns>
        public abstract Task<TransformationResult?> Apply(ITransformationContext context);

        /// <summary>
        /// Clones the mapping rule.
        /// </summary>
        /// <returns>A clone of the mapping rule.</returns>
        public abstract MappingRuleBase Clone();

        /// <summary>
        /// Clones the mapping rule (IMappingRule interface implementation).
        /// </summary>
        /// <returns>A cloned IMappingRule.</returns>
        IMappingRule IMappingRule.Clone() => Clone();

        /// <summary>
        /// Gets the value based on the source record and target field.
        /// </summary>
        /// <param name="sourceRecord">The source record.</param>
        /// <param name="targetField">The target field descriptor.</param>
        /// <returns>A transformation result.</returns>
        public abstract TransformationResult GetValue(List<ImportedRecordFieldDescriptor> sourceRecord, ImportedRecordFieldDescriptor targetField);

        /// <summary>
        /// Gets the value based on the data row and target type.
        /// </summary>
        /// <param name="row">The data row to get the value from.</param>
        /// <param name="targetType">The type to convert the value to.</param>
        /// <returns>A transformation result.</returns>
        public virtual async Task<TransformationResult> GetValue(DataRow row, Type targetType)
        {
            // Default implementation can be overridden by concrete classes
            if (row == null)
            {
                throw new ArgumentNullException(nameof(row));
            }

            // This could be implemented by converting DataRow to List<ImportedRecordFieldDescriptor> 
            // and calling the existing GetValue method, or concrete implementations can handle it directly
            var result = await Apply(row);
            return result ?? TransformationResult.Failure(
                originalValue: null,
                targetType: targetType,
                errorMessage: "Rule.Apply(DataRow) returned null",
                originalValueType: null,
                currentValueType: null);
        }

        /// <summary>
        /// Helper method to clone base properties. Derived classes should call this in their Clone implementation.
        /// </summary>
        /// <param name="target">The target mapping rule to clone properties to.</param>
        protected virtual void CloneBaseProperties(MappingRuleBase target)
        {
            target.SourceField = this.SourceField;
            target.TargetField = this.TargetField;
            target.RuleDetail = this.RuleDetail;
            // Ensure ValueTransformationBase has a Clone() method that returns ValueTransformationBase or derived.
            target.SourceFieldTransformations = [.. this.SourceFieldTransformations.Select(t => t.Clone())];
            // RuleType and EnumMemberName are abstract and should be defined by the concrete type itself.
        }        /// <summary>
                 /// Maps from MappingRuleType to RuleType.
                 /// </summary>
                 /// <param name="mappingRuleType">The MappingRuleType to map.</param>
                 /// <returns>The corresponding RuleType.</returns>
        protected static RuleType MapToRuleType(MappingRuleType mappingRuleType)
        {
            return mappingRuleType switch
            {
                Enums.MappingRuleType.CopyRule => Enums.RuleType.CopyRule,
                Enums.MappingRuleType.IgnoreRule => Enums.RuleType.IgnoreRule,
                Enums.MappingRuleType.CombineFieldsRule => Enums.RuleType.CombineFieldsRule,
                Enums.MappingRuleType.ConstantValueRule => Enums.RuleType.ConstantValueRule,
                Enums.MappingRuleType.CustomFieldlessRule => Enums.RuleType.CustomFieldlessRule,
                Enums.MappingRuleType.StaticValueRule => Enums.RuleType.StaticValue,
                _ => Enums.RuleType.None
            };
        }

        /// <summary>
        /// Disposes the mapping rule, releasing any resources.
        /// </summary>
        public virtual void Dispose()
        {
            // Default empty implementation - derived classes can override if needed
            GC.SuppressFinalize(this);
        }
    }
}
