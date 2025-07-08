using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Models;

using System.Data;
using System.Collections;

namespace AutomatedRealms.DataImportUtility.Core.Rules;

/// <summary>
/// Implementation of a rule that copies a value from a source field,
/// potentially applying transformations and conditional logic.
/// </summary>
public class CopyRule : MappingRuleBase
{
    /// <summary>
    /// Gets the unique identifier for this type of mapping rule.
    /// </summary>
    public static readonly string TypeIdString = "Core.CopyRule";

    /// <summary>
    /// Error message when input is a collection.
    /// </summary>
    public const string InvalidInputCollectionMessage = "CopyRule cannot process collections directly. Input value is a collection.";

    /// <summary>
    /// Initializes a new instance of the <see cref="CopyRule"/> class.
    /// </summary>
    public CopyRule() : base(TypeIdString) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="CopyRule"/> class.
    /// </summary>
    /// <param name="sourceFieldName">The name of the source field to copy the value from.</param>
    public CopyRule(string sourceFieldName) : base(TypeIdString)
    {
        SourceField = sourceFieldName;
    }

    /// <summary>
    /// Gets the display name of the mapping rule.
    /// </summary>
    public override string DisplayName => $"Copy from '{SourceField ?? "[undefined]"}'";

    /// <summary>
    /// Gets the description of the mapping rule.
    /// </summary>
    public override string Description => $"Copies the value from the source field '{SourceField ?? "[undefined]"}'.";

    /// <summary>
    /// Gets the short name of the mapping rule.
    /// </summary>
    public override string ShortName => "Copy";

    /// <summary>
    /// Indicates whether the mapping rule is empty or not configured.
    /// For a CopyRule, it's empty if the SourceField is not set.
    /// </summary>
    public override bool IsEmpty => string.IsNullOrEmpty(SourceField);

    /// <summary>
    /// Applies the mapping rule to the provided transformation context.
    /// </summary>
    /// <param name="context">The transformation context containing the data row and other relevant information.</param>
    /// <returns>A transformation result, or null if the rule could not be applied.</returns>
    public override async Task<TransformationResult?> Apply(ITransformationContext context)
    {
        if (string.IsNullOrEmpty(SourceField))
        {
            return TransformationResult.Failure(
                originalValue: null,
                targetType: context.TargetFieldType ?? typeof(object),
                errorMessage: "Source field name is not configured for CopyRule.",
                record: context.Record,
                sourceRecordContext: context.SourceRecordContext,
                explicitTargetFieldType: context.TargetFieldType ?? typeof(object));
        }

        var dataRow = context.Record;
        if (dataRow is null || !dataRow.Table.Columns.Contains(SourceField))
        {
            return TransformationResult.Failure(
                originalValue: null,
                targetType: context.TargetFieldType ?? typeof(object),
                errorMessage: $"Source field '{SourceField}' not found in DataRow or DataRow is null.",
                record: dataRow,
                sourceRecordContext: context.SourceRecordContext,
                explicitTargetFieldType: context.TargetFieldType ?? typeof(object));
        }

        var sourceValue = dataRow[SourceField];
        var sourceValueType = sourceValue?.GetType();
        var targetType = context.TargetFieldType ?? sourceValueType ?? typeof(object);

        // Check if input is a collection (except for strings which are technically collections of char)
        if (IsCollection(sourceValue) && !(sourceValue is string))
        {
            return TransformationResult.Failure(
                originalValue: sourceValue,
                targetType: targetType,
                errorMessage: InvalidInputCollectionMessage,
                originalValueType: sourceValueType,
                currentValueType: sourceValueType,
                record: dataRow,
                sourceRecordContext: context.SourceRecordContext,
                explicitTargetFieldType: targetType);
        }

        var initialLog = new List<string> { $"CopyRule: Initial value from '{SourceField}' ('{sourceValue ?? "null"}')." };

        var currentProcessingResult = TransformationResult.Success(
            originalValue: sourceValue,
            originalValueType: sourceValueType,
            currentValue: sourceValue,
            currentValueType: sourceValueType,
            appliedTransformations: initialLog,
            record: dataRow,
            sourceRecordContext: context.SourceRecordContext,
            targetFieldType: targetType
        );

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
            $"CopyRule: Copied value from '{SourceField}' after all transformations. Final value: '{currentProcessingResult.CurrentValue ?? "null"}'."
        };

        return currentProcessingResult with { AppliedTransformations = finalLog };
    }

    /// <summary>
    /// Checks if the given value is a collection.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the value is a collection, false otherwise.</returns>
    private static bool IsCollection(object? value)
    {
        if (value == null)
        {
            return false;
        }

        return value is IEnumerable && !(value is string);
    }

    /// <summary>
    /// Clones this mapping rule.
    /// </summary>
    /// <returns>A new instance of <see cref="CopyRule"/> with copied values.</returns>
    public override MappingRuleBase Clone()
    {
        var clone = new CopyRule();
        CloneBaseProperties(clone);
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
        throw new NotImplementedException("This method is obsolete. Use Apply(ITransformationContext) instead, which can be constructed with a DataRow.");
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
