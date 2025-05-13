using System.Data;
using System.Text.Json.Serialization;

using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Models;

namespace AutomatedRealms.DataImportUtility.Core.Rules;

/// <summary>
/// A rule indicating the field should be ignored and not output to the destination.
/// </summary>
public class IgnoreRule : MappingRuleBase
{
    /// <summary>
    /// Static TypeId for this rule.
    /// </summary>
    public static readonly string TypeIdString = "Core.IgnoreRule";

    /// <summary>
    /// Initializes a new instance of the <see cref="IgnoreRule"/> class.
    /// </summary>
    public IgnoreRule() : base(TypeIdString) { }

    /// <inheritdoc />
    [JsonIgnore]
    public override string DisplayName { get; } = "Ignore";

    /// <inheritdoc />
    [JsonIgnore]
    public override string ShortName => "Ignore";

    /// <inheritdoc />
    [JsonIgnore]
    public override string Description { get; } = "Do not output this field to the destination.";

    /// <inheritdoc />
    [JsonIgnore]
    public override bool IsEmpty => false; // An ignore rule is never considered empty in terms of configuration.

    /// <inheritdoc />
    public override async Task<TransformationResult?> Apply(DataRow dataRow)
    {
        var context = TransformationResult.Success(
            originalValue: null,
            originalValueType: null,
            currentValue: null,
            currentValueType: typeof(object), // Placeholder, will be nullified by IgnoreRule logic
            appliedTransformations: new List<string>(),
            record: dataRow,
            tableDefinition: null, // Or ParentTableDefinition
            sourceRecordContext: null,
            targetFieldType: null // Ignored field doesn't have a target type in the same way
        );
        return await Apply(context);
    }

    /// <summary>
    /// Applies the ignore rule using the provided transformation context.
    /// The result will indicate that the field is ignored.
    /// </summary>
    /// <param name="context">The transformation context.</param>
    /// <returns>A transformation result indicating the field was ignored.</returns>
    public override Task<TransformationResult?> Apply(ITransformationContext context)
    {
        // For an IgnoreRule, the CurrentValue is effectively null or not applicable.
        // The key is the AppliedTransformations message.
        return Task.FromResult<TransformationResult?>(TransformationResult.Success(
            originalValue: null, // No original value is processed or relevant for ignore
            originalValueType: null,
            currentValue: null, // Current value is null as it's ignored
            currentValueType: context.TargetFieldType ?? typeof(object), // Retain target type for context if available
            appliedTransformations: ["Field ignored by IgnoreRule."],
            record: context.Record,
            tableDefinition: context.TableDefinition,
            sourceRecordContext: context.SourceRecordContext,
            targetFieldType: context.TargetFieldType
        ));
    }

    /// <inheritdoc />
    public override async Task<IEnumerable<TransformationResult?>> Apply(DataTable data)
    {
        if (data == null) return new List<TransformationResult?>();
        var results = new List<TransformationResult?>();
        foreach (DataRow row in data.Rows)
        {
            var rowContext = TransformationResult.Success(
                originalValue: null, originalValueType: null,
                currentValue: null, currentValueType: typeof(object),
                appliedTransformations: new List<string>(),
                record: row,
                tableDefinition: null, // Or ParentTableDefinition
                sourceRecordContext: null,
                targetFieldType: null
            );
            results.Add(await Apply(rowContext).ConfigureAwait(false));
        }
        return results;
    }

    /// <inheritdoc />
    public override async Task<IEnumerable<TransformationResult?>> Apply()
    {
        // Apply for an IgnoreRule without specific record context.
        var emptyContext = TransformationResult.Success(
            originalValue: null, originalValueType: null,
            currentValue: null, currentValueType: typeof(object),
            appliedTransformations: new List<string>(),
            record: null,
            tableDefinition: null,
            sourceRecordContext: null,
            targetFieldType: null
        );
        var result = await Apply(emptyContext);
        return new List<TransformationResult?> { result }.AsEnumerable();
    }

    /// <inheritdoc />
    public override TransformationResult GetValue(List<ImportedRecordFieldDescriptor> sourceRecord, ImportedRecordFieldDescriptor targetField)
    {
        Type effectiveTargetType = targetField?.FieldType ?? typeof(object);
        var context = TransformationResult.Success(
            originalValue: null,
            originalValueType: null,
            currentValue: null,
            currentValueType: effectiveTargetType,
            appliedTransformations: new List<string>(),
            record: null,
            tableDefinition: null,
            sourceRecordContext: sourceRecord,
            targetFieldType: effectiveTargetType
        );

        var task = Apply(context);
        TransformationResult? result = task.ConfigureAwait(false).GetAwaiter().GetResult();

        return result ?? TransformationResult.Failure(
            originalValue: null,
            targetType: effectiveTargetType,
            errorMessage: "IgnoreRule.Apply returned null unexpectedly in GetValue.",
            sourceRecordContext: sourceRecord,
            explicitTargetFieldType: effectiveTargetType
        );
    }

    /// <inheritdoc />
    public override MappingRuleBase Clone()
    {
        var clone = new IgnoreRule();
        base.CloneBaseProperties(clone);
        return clone;
    }
}
