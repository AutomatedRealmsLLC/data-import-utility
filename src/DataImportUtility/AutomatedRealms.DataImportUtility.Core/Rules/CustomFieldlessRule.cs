using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Models;

using System.Data;

namespace AutomatedRealms.DataImportUtility.Core.Rules;

/// <summary>
/// A custom rule that does not necessarily require a direct source field for its primary operation,
/// but can be configured with transformations and conditional logic. Its behavior is primarily
/// driven by its configured transformations.
/// </summary>
public class CustomFieldlessRule : MappingRuleBase
{
    /// <summary>
    /// Gets the unique identifier for this type of mapping rule.
    /// </summary>
    public static readonly string TypeIdString = "Core.CustomFieldlessRule";

    /// <summary>
    /// Gets or sets an optional detail or initial value for the rule, which might be used by transformations.
    /// This shadows the base RuleDetail to allow setting it directly on this type if needed,
    /// though typically transformations would operate on context or pre-defined values.
    /// </summary>
    public new string? RuleDetail { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomFieldlessRule"/> class.
    /// </summary>
    public CustomFieldlessRule() : base(TypeIdString) { }

    /// <summary>
    /// Gets the display name of the mapping rule.
    /// </summary>
    public override string DisplayName => "Custom Fieldless Rule";

    /// <summary>
    /// Gets the short name of the mapping rule.
    /// </summary>
    public override string ShortName => "Custom...";

    /// <summary>
    /// Gets the description of the mapping rule.
    /// </summary>
    public override string Description => "A custom rule that executes logic defined by its transformations and conditions, not necessarily tied to a single source field.";

    /// <summary>
    /// Indicates whether the mapping rule is empty or not configured.
    /// For a CustomFieldlessRule, it's empty if it has no value transformations configured.
    /// </summary>
    public override bool IsEmpty => !SourceFieldTransformations.Any();

    /// <summary>
    /// Applies the custom fieldless rule using the provided transformation context.
    /// Its behavior is primarily driven by its configured <see cref="MappingRuleBase.SourceFieldTransformations"/>.
    /// </summary>
    /// <param name="context">The transformation context.</param>
    /// <returns>A transformation result, or null if the rule could not be applied or if transformations fail.</returns>
    public override async Task<TransformationResult?> Apply(ITransformationContext context)
    {
        // Use RuleDetail from this class, or from base if this one is null.
        object? initialValue = this.RuleDetail ?? base.RuleDetail;
        Type? initialValueType = initialValue?.GetType();
        Type targetType = context.TargetFieldType ?? typeof(object);

        var log = new List<string> { $"CustomFieldlessRule: Starting with initial value: '{initialValue ?? "null"}'." };

        TransformationResult currentProcessingResult = TransformationResult.Success(
            originalValue: initialValue,
            originalValueType: initialValueType,
            currentValue: initialValue,
            currentValueType: initialValueType,
            appliedTransformations: log,
            record: context.Record,
            sourceRecordContext: context.SourceRecordContext,
            targetFieldType: targetType
        );

        if (!SourceFieldTransformations.Any())
        {
            var updatedLog = new List<string>(currentProcessingResult.AppliedTransformations ?? [])
            {
                "CustomFieldlessRule: No transformations configured. Returning initial value."
            };
            return currentProcessingResult with { AppliedTransformations = updatedLog };
        }

        foreach (var transformation in SourceFieldTransformations)
        {
            currentProcessingResult = await transformation.ApplyTransformationAsync(currentProcessingResult);
            if (currentProcessingResult.WasFailure)
            {
                return currentProcessingResult; // Propagate failure
            }
        }

        var finalLog = new List<string>(currentProcessingResult.AppliedTransformations ?? [])
        {
            "CustomFieldlessRule: Finished applying all transformations."
        };
        return currentProcessingResult with { AppliedTransformations = finalLog };
    }

    /// <summary>
    /// Clones this mapping rule.
    /// </summary>
    /// <returns>A new instance of <see cref="CustomFieldlessRule"/> with copied values.</returns>
    public override MappingRuleBase Clone()
    {
        var clone = new CustomFieldlessRule(); // Calls base(TypeIdString) constructor
        // Clone base properties first. This will clone base.RuleDetail to clone.RuleDetail (the base property).
        this.CloneBaseProperties(clone);
        // Now, specifically set the RuleDetail for the CustomFieldlessRule instance.
        clone.RuleDetail = this.RuleDetail;
        return clone;
    }

    #region Obsolete Abstract Implementations
    // These methods are now obsolete due to refactoring in MappingRuleBase.
    // They are implemented here to satisfy the abstract class requirements but should not be used directly.
    // Prefer using Apply(ITransformationContext context) for applying rules.

    /// <inheritdoc/>
    /// <remarks>This method is obsolete. Use <see cref="Apply(ITransformationContext)"/> instead.</remarks>
    public override Task<TransformationResult?> Apply(DataRow dataRow)
    {
        throw new NotImplementedException("This method is obsolete. Use Apply(ITransformationContext) instead. A TransformationContext can be created manually if needed, but this method should not be called directly.");
    }

    /// <inheritdoc/>
    /// <remarks>This method is obsolete. Use <see cref="Apply(ITransformationContext)"/> instead.</remarks>
    public override Task<IEnumerable<TransformationResult?>> Apply(DataTable dataTable)
    {
        throw new NotImplementedException("This method is obsolete. Iterate through rows and use Apply(ITransformationContext) for each DataRow instead.");
    }

    /// <inheritdoc/>
    /// <remarks>This method is obsolete. Use <see cref="Apply(ITransformationContext)"/> instead.</remarks>
    public override Task<IEnumerable<TransformationResult?>> Apply()
    {
        throw new NotImplementedException("This method is obsolete. Use Apply(ITransformationContext) instead, providing a context.");
    }

    /// <inheritdoc/>
    /// <remarks>This method is obsolete or its usage is unclear in this context. The primary rule application logic is in <see cref="Apply(ITransformationContext)"/>.</remarks>
    public override TransformationResult GetValue(List<ImportedRecordFieldDescriptor> sourceRecord, ImportedRecordFieldDescriptor targetField)
    {
        throw new NotImplementedException("This method is obsolete or its usage is unclear in this context. Consider using Apply(ITransformationContext) and accessing TransformationResult.CurrentValue.");
    }
    #endregion
}
