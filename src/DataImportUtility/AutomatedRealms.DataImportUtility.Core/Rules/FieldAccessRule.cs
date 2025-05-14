using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Models;

using System.Data;
using System.Text.Json.Serialization;

namespace AutomatedRealms.DataImportUtility.Core.Rules;

/// <summary>
/// A rule that accesses a field's value from a DataRow or a list of field descriptors.
/// The primary purpose is to retrieve a value from a specified source field.
/// </summary>
public class FieldAccessRule : MappingRuleBase
{
    /// <summary>
    /// Static TypeId for this rule.
    /// </summary>
    public static readonly string TypeIdString = "Core.FieldAccessRule";

    /// <summary>
    /// Initializes a new instance of the <see cref="FieldAccessRule"/> class.
    /// </summary>
    /// <param name="sourceFieldName">The name of the field to access.</param>
    public FieldAccessRule(string sourceFieldName) : base(TypeIdString)
    {
        SourceField = sourceFieldName;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FieldAccessRule"/> class.
    /// </summary>
    /// <param name="sourceFieldName">The name of the field to access.</param>
    /// <param name="ruleDetail">Additional information about the rule.</param>
    public FieldAccessRule(string sourceFieldName, string ruleDetail) : base(TypeIdString)
    {
        SourceField = sourceFieldName;
        RuleDetail = ruleDetail;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FieldAccessRule"/> class.
    /// Parameterless constructor for serialization.
    /// </summary>
    public FieldAccessRule() : base(TypeIdString) { }

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
    public override bool IsEmpty => string.IsNullOrEmpty(SourceField);

    /// <summary>
    /// Applies the rule to a DataRow. This overload extracts the field value directly from the row.
    /// </summary>
    /// <param name="dataRow">The DataRow to apply the rule to.</param>
    /// <returns>A TransformationResult containing the field's value or a failure if the field is not found.</returns>
    public override async Task<TransformationResult?> Apply(DataRow dataRow)
    {
        var context = TransformationResult.Success(
            originalValue: null,
            originalValueType: null,
            currentValue: null,
            currentValueType: null,
            appliedTransformations: [],
            record: dataRow,
            tableDefinition: ParentTableDefinition,
            sourceRecordContext: null,
            targetFieldType: null
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
        if (string.IsNullOrEmpty(SourceField))
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
        var found = false;
        var actualSourceFieldName = SourceField;

        if (context.Record is not null)
        {
            if (context.Record.Table.Columns.Contains(actualSourceFieldName))
            {
                value = context.Record[actualSourceFieldName];
                valueType = context.Record.Table.Columns[actualSourceFieldName]?.DataType ?? value?.GetType();
                found = true;
            }
            else
            {
                return Task.FromResult<TransformationResult?>(TransformationResult.Failure(
                    originalValue: null,
                    targetType: context.TargetFieldType ?? typeof(object),
                    errorMessage: $"Field '{actualSourceFieldName}' not found in DataRow.",
                    record: context.Record,
                    tableDefinition: context.TableDefinition,
                    sourceRecordContext: context.SourceRecordContext,
                    explicitTargetFieldType: context.TargetFieldType
                ));
            }
        }
        else if (context.SourceRecordContext is not null)
        {
            var fieldDesc = context.SourceRecordContext.FirstOrDefault(f => f.FieldName.Equals(actualSourceFieldName, StringComparison.OrdinalIgnoreCase));
            if (fieldDesc is not null)
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
                    errorMessage: $"Field '{actualSourceFieldName}' not found in SourceRecordContext.",
                    record: context.Record,
                    tableDefinition: context.TableDefinition,
                    sourceRecordContext: context.SourceRecordContext,
                    explicitTargetFieldType: context.TargetFieldType
                ));
            }
        }

        if (found)
        {
            if (context.TargetFieldType is not null && value is not null && valueType != context.TargetFieldType)
            {
                try
                {
                    value = Convert.ChangeType(value, context.TargetFieldType, System.Globalization.CultureInfo.InvariantCulture);
                    valueType = context.TargetFieldType;
                }
                catch (Exception ex)
                {
                    return Task.FromResult<TransformationResult?>(TransformationResult.Failure(
                        originalValue: value,
                        targetType: context.TargetFieldType,
                        errorMessage: $"FieldAccessRule: Could not convert value from field '{actualSourceFieldName}' to target type '{context.TargetFieldType.Name}'. Error: {ex.Message}",
                        originalValueType: value?.GetType(),
                        record: context.Record,
                        tableDefinition: context.TableDefinition,
                        sourceRecordContext: context.SourceRecordContext,
                        explicitTargetFieldType: context.TargetFieldType
                    ));
                }
            }

            return Task.FromResult<TransformationResult?>(TransformationResult.Success(
                originalValue: value,
                originalValueType: valueType,
                currentValue: value,
                currentValueType: valueType,
                appliedTransformations: [$"Accessed field '{actualSourceFieldName}'."],
                record: context.Record,
                tableDefinition: context.TableDefinition,
                sourceRecordContext: context.SourceRecordContext,
                targetFieldType: context.TargetFieldType ?? valueType
            ));
        }

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
        if (data is null) throw new ArgumentNullException(nameof(data));

        if (string.IsNullOrEmpty(SourceField))
        {
            var failureResult = TransformationResult.Failure(
                originalValue: null,
                targetType: typeof(object),
                errorMessage: "FieldAccessRule is not configured: SourceField is missing.",
                explicitTargetFieldType: typeof(object)
            );
            return Enumerable.Repeat(failureResult, data.Rows.Count).Cast<TransformationResult?>();
        }

        var results = new List<TransformationResult?>();
        foreach (DataRow row in data.Rows)
        {
            var rowContext = TransformationResult.Success(
                originalValue: null, originalValueType: null,
                currentValue: null, currentValueType: null,
                appliedTransformations: [],
                record: row,
                tableDefinition: ParentTableDefinition,
                sourceRecordContext: null,
                targetFieldType: row.Table.Columns.Contains(SourceField) ? row.Table.Columns[SourceField]?.DataType : null
            );
            results.Add(await Apply(rowContext).ConfigureAwait(false));
        }
        return results;
    }

    /// <summary>
    /// Applies the field access rule in a context where no specific data table or row is provided initially.
    /// This typically implies an attempt to apply the rule without sufficient context, leading to failure.
    /// </summary>
    /// <returns>A collection containing a single failure result, as context is required for field access.</returns>
    public override Task<IEnumerable<TransformationResult?>> Apply()
    {
        var failureResult = TransformationResult.Failure(
            originalValue: null,
            targetType: typeof(object),
            errorMessage: "FieldAccessRule cannot be applied without a DataRow or SourceRecordContext.",
            explicitTargetFieldType: typeof(object)
        );
        return Task.FromResult(new List<TransformationResult?> { failureResult }.AsEnumerable());
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
            appliedTransformations: [],
            record: null,
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
