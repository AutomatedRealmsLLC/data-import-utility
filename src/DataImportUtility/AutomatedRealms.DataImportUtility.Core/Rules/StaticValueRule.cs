using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Models;

using System.Data;
using System.Text.Json.Serialization;


namespace AutomatedRealms.DataImportUtility.Core.Rules;

/// <summary>
/// A mapping rule that always returns a static, predefined value.
/// Useful for providing constant values as operands or direct outputs.
/// </summary>
public class StaticValueRule : MappingRuleBase
{
    /// <summary>
    /// The unique identifier string for this rule type.
    /// </summary>
    public static readonly string TypeIdString = "Core.StaticValueRule";

    private object? _staticValue;

    /// <summary>
    /// Gets or sets the static value this rule provides.
    /// Setting this value also updates the base <see cref="MappingRuleBase.RuleDetail"/>.
    /// </summary>
    public object? Value
    {
        get => _staticValue;
        set
        {
            _staticValue = value;
            RuleDetail = value?.ToString();
        }
    }

    /// <summary>
    /// Gets the display name of the mapping rule.
    /// </summary>
    [JsonIgnore]
    public override string DisplayName => "Static Value";

    /// <summary>
    /// Gets the short name of the mapping rule.
    /// </summary>
    [JsonIgnore]
    public override string ShortName => "Static";

    /// <summary>
    /// Gets the description of the mapping rule, incorporating the current static value.
    /// </summary>
    [JsonIgnore]
    public override string Description => $"Provides a static value: '{Value?.ToString() ?? "null"}'.";

    /// <summary>
    /// Indicates whether the mapping rule is empty or not configured.
    /// For a StaticValueRule, it's considered empty if its <see cref="Value"/> is null.
    /// </summary>
    [JsonIgnore]
    public override bool IsEmpty => Value is null;

    /// <summary>
    /// Initializes a new instance of the <see cref="StaticValueRule"/> class with a specific static value.
    /// </summary>
    /// <param name="value">The static value this rule should represent.</param>
    public StaticValueRule(object? value) : base(TypeIdString)
    {
        Value = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StaticValueRule"/> class with a specific static value and rule detail.
    /// </summary>
    /// <param name="value">The static value this rule should represent.</param>
    /// <param name="ruleDetail">Additional information about the rule (will set base.RuleDetail).</param>
    public StaticValueRule(object? value, string ruleDetail) : base(TypeIdString)
    {
        Value = value;
        RuleDetail = ruleDetail;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StaticValueRule"/> class.
    /// Parameterless constructor for serialization.
    /// </summary>
    public StaticValueRule() : base(TypeIdString)
    {
    }

    /// <summary>
    /// Creates a new object that is a copy of the current instance.
    /// </summary>
    /// <returns>A new object that is a copy of this instance.</returns>
    public override MappingRuleBase Clone()
    {
        var clone = new StaticValueRule(Value);
        CloneBaseProperties(clone);
        return clone;
    }

    /// <summary>
    /// Applies the mapping rule to the given transformation context.
    /// For a static value rule, this involves returning the configured static value,
    /// potentially modified by any configured transformations.
    /// </summary>
    /// <param name="context">The transformation context.</param>
    /// <returns>A <see cref="TransformationResult"/> containing the outcome of the rule application.</returns>
    public override async Task<TransformationResult?> Apply(ITransformationContext context)
    {
        var currentValue = Value;
        var currentValueType = Value?.GetType() ?? typeof(object);
        var appliedTransformations = new List<string> { $"StaticValueRule provided value: '{currentValue?.ToString() ?? "null"}'." };

        TransformationResult currentResult = TransformationResult.Success(
            originalValue: currentValue,
            originalValueType: currentValueType,
            currentValue: currentValue,
            currentValueType: currentValueType,
            appliedTransformations: appliedTransformations,
            record: context?.Record,
            tableDefinition: context?.TableDefinition,
            sourceRecordContext: context?.SourceRecordContext,
            targetFieldType: context?.TargetFieldType
        );

        foreach (var transformation in SourceFieldTransformations)
        {
            if (transformation is null) continue;
            currentResult = await transformation.ApplyTransformationAsync(currentResult).ConfigureAwait(false);
            if (currentResult.WasFailure)
            {
                break;
            }
        }
        return currentResult;
    }

    /// <summary>
    /// Applies the mapping rule without specific context. Returns the static value after transformations.
    /// </summary>
    public override async Task<IEnumerable<TransformationResult?>> Apply()
    {
        var result = await Apply((ITransformationContext)null!).ConfigureAwait(false);
        return [result];
    }

    /// <summary>
    /// Applies the mapping rule to each row in a DataTable.
    /// </summary>
    public override async Task<IEnumerable<TransformationResult?>> Apply(DataTable data)
    {
        if (data is null) { return []; }
        var results = new List<TransformationResult?>();
        foreach (DataRow row in data.Rows)
        {
            results.Add(await Apply(row).ConfigureAwait(false));
        }
        return results;
    }

    /// <summary>
    /// Applies the mapping rule to a DataRow.
    /// </summary>
    public override async Task<TransformationResult?> Apply(DataRow dataRow)
    {
        var context = new TransformationResult // TransformationResult implements ITransformationContext
        {
            Record = dataRow,
            // Other context properties can be set if available/relevant
        };
        return await Apply(context).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the static value, wrapped in a TransformationResult, without applying further transformations.
    /// The targetField's type is used to set the TargetFieldType in the result.
    /// </summary>
    public override TransformationResult GetValue(List<ImportedRecordFieldDescriptor> sourceRecord, ImportedRecordFieldDescriptor targetField)
    {
        return TransformationResult.Success(
            originalValue: Value,
            originalValueType: Value?.GetType() ?? typeof(object),
            currentValue: Value,
            currentValueType: Value?.GetType() ?? typeof(object),
            targetFieldType: targetField?.FieldType,
            sourceRecordContext: sourceRecord
        );
    }
}
