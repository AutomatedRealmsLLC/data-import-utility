using AutomatedRealms.DataImportUtility.Abstractions.Models;

using System.Data;
using System.Text.Json.Serialization;

namespace AutomatedRealms.DataImportUtility.Abstractions;

/// <summary>
/// Base class for all mapping rules.
/// </summary>
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
    /// Gets the unique identifier for this type of mapping rule.
    /// This is used for serialization and deserialization.
    /// </summary>
    public string TypeId { get; protected set; }

    /// <summary>
    /// The additional information for the rule.
    /// </summary>
    public string? RuleDetail { get; set; }

    /// <summary>
    /// Gets or sets the source field.
    /// </summary>
    public string? SourceField { get; set; }

    /// <summary>
    /// Gets or sets the target field.
    /// </summary>
    public string? TargetField { get; set; }

    /// <summary>
    /// Gets or sets the parent table definition that this rule operates within.
    /// </summary>
    [JsonIgnore]
    public ImportTableDefinition? ParentTableDefinition { get; set; }

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
    /// Indicates whether the mapping rule is empty or not configured.
    /// </summary>
    public virtual bool IsEmpty => false;

    /// <summary>
    /// Initializes a new instance of the <see cref="MappingRuleBase"/> class.
    /// </summary>
    /// <param name="typeId">The unique identifier for this mapping rule type.</param>
    protected MappingRuleBase(string typeId)
    {
        if (string.IsNullOrWhiteSpace(typeId))
        {
            throw new ArgumentException("TypeId cannot be null or whitespace.", nameof(typeId));
        }
        TypeId = typeId;
    }

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
    IMappingRule IMappingRule.Clone() => this.Clone();

    /// <summary>
    /// Clones the mapping rule (ICloneable interface implementation).
    /// </summary>
    /// <returns>A cloned ICloneable.</returns>
    object ICloneable.Clone()
    {
        return Clone();
    }

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
        if (row is null)
        {
            throw new ArgumentNullException(nameof(row));
        }
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
        target.TypeId = this.TypeId;
        target.SourceFieldTransformations = [.. this.SourceFieldTransformations.Select(t => t.Clone())];
    }

    /// <summary>
    /// Disposes the mapping rule, releasing any resources.
    /// </summary>
    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
