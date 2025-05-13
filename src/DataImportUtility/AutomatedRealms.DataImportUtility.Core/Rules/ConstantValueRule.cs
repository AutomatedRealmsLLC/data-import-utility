using System.Data;
using System.Text.Json.Serialization;

using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Models;

namespace AutomatedRealms.DataImportUtility.Core.Rules;

/// <summary>
/// A rule indicating the value should be constant.
/// </summary>
public partial class ConstantValueRule : MappingRuleBase
{
    /// <summary>
    /// Static TypeId for this rule.
    /// </summary>
    public static readonly string TypeIdString = "Core.ConstantValueRule";

    /// <summary>
    /// Gets or sets the constant value for this rule.
    /// This property shadows the base RuleDetail for strong typing if needed,
    /// but primarily uses the base class's RuleDetail for storage.
    /// </summary>
    public new string? RuleDetail
    {
        get => base.RuleDetail;
        set => base.RuleDetail = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConstantValueRule"/> class.
    /// </summary>
    public ConstantValueRule() : base(TypeIdString) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConstantValueRule"/> class with a specific constant value.
    /// </summary>
    /// <param name="constantValue">The constant value for this rule.</param>
    public ConstantValueRule(string? constantValue) : base(TypeIdString)
    {
        this.RuleDetail = constantValue;
    }

    /// <summary>
    /// Gets the display name of the mapping rule.
    /// </summary>
    [JsonIgnore]
    public override string DisplayName { get; } = "Constant Value";

    /// <summary>
    /// Gets the short name of the mapping rule.
    /// </summary>
    [JsonIgnore]
    public override string ShortName => "Constant";

    /// <summary>
    /// Gets the description of the mapping rule.
    /// </summary>
    [JsonIgnore]
    public override string Description { get; } = "Outputs a constant value (from RuleDetail) for each record.";

    /// <summary>
    /// Indicates whether the mapping rule is empty or not configured.
    /// For a ConstantValueRule, it's empty if RuleDetail is not set.
    /// </summary>
    [JsonIgnore]
    public override bool IsEmpty => string.IsNullOrWhiteSpace(RuleDetail);

    /// <summary>
    /// Applies the constant value rule to the provided data row.
    /// This overload is maintained for compatibility or specific scenarios but delegates to the context-based Apply.
    /// </summary>
    /// <param name="dataRow">The data row to apply the rule to.</param>
    /// <returns>A transformation result containing the constant value.</returns>
    public override async Task<TransformationResult?> Apply(DataRow dataRow)
    {
        var context = TransformationResult.Success(
            originalValue: null,
            originalValueType: typeof(object),
            currentValue: null, 
            currentValueType: typeof(object),
            appliedTransformations: new List<string>(),
            record: dataRow,
            tableDefinition: ParentTableDefinition, 
            sourceRecordContext: null,
            targetFieldType: RuleDetail?.GetType() ?? typeof(string)
        );
        return await Apply(context);
    }

    /// <summary>
    /// Applies the constant value rule using the provided transformation context.
    /// </summary>
    /// <param name="context">The transformation context.</param>
    /// <returns>A transformation result containing the constant value.</returns>
    public override Task<TransformationResult?> Apply(ITransformationContext context) 
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        Type valueType = context.TargetFieldType ?? RuleDetail?.GetType() ?? typeof(string);
        object? typedValue = RuleDetail;

        if (RuleDetail != null && context.TargetFieldType != null && context.TargetFieldType != typeof(string))
        {
            try
            {
                typedValue = Convert.ChangeType(RuleDetail, context.TargetFieldType, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                return Task.FromResult<TransformationResult?>(TransformationResult.Failure(
                    originalValue: RuleDetail,
                    targetType: context.TargetFieldType, 
                    errorMessage: $"ConstantValueRule: Could not convert constant value '{RuleDetail}' to target type '{context.TargetFieldType.Name}'. Error: {ex.Message}",
                    appliedTransformations: new List<string> { DisplayName },
                    record: context.Record,
                    tableDefinition: context.TableDefinition,
                    sourceRecordContext: context.SourceRecordContext,
                    originalValueType: RuleDetail?.GetType() ?? typeof(string),
                    explicitTargetFieldType: context.TargetFieldType 
                ));
            }
        }
        
        var result = TransformationResult.Success(
            originalValue: RuleDetail,
            originalValueType: RuleDetail?.GetType() ?? typeof(string),
            currentValue: typedValue,
            currentValueType: valueType,
            appliedTransformations: new List<string> { DisplayName },
            record: context.Record,
            tableDefinition: context.TableDefinition,
            sourceRecordContext: context.SourceRecordContext,
            targetFieldType: context.TargetFieldType
        );
        return Task.FromResult<TransformationResult?>(result);
    }

    /// <summary>
    /// Clones this ConstantValueRule instance.
    /// </summary>
    /// <returns>A new instance of ConstantValueRule with the same constant value.</returns>
    public override MappingRuleBase Clone()
    {
        var clone = new ConstantValueRule(this.RuleDetail);
        CloneBaseProperties(clone); 
        return clone;
    }

    /// <summary>
    /// Applies the mapping rule without a specific data context. 
    /// For a constant value, this will produce a single result with the constant.
    /// </summary>
    /// <returns>A collection containing a single transformation result with the constant value.</returns>
    public override Task<IEnumerable<TransformationResult?>> Apply()
    {
        // Create a minimal context for the constant value.
        var context = TransformationResult.Success(
            originalValue: RuleDetail, 
            originalValueType: RuleDetail?.GetType() ?? typeof(string),
            currentValue: RuleDetail, 
            currentValueType: RuleDetail?.GetType() ?? typeof(string),
            appliedTransformations: new List<string> { DisplayName },
            targetFieldType: RuleDetail?.GetType() ?? typeof(string)
        );
        var singleResult = Apply(context).Result; // Await the task
        return Task.FromResult(new List<TransformationResult?> { singleResult }.AsEnumerable());
    }

    /// <summary>
    /// Applies the mapping rule to each row in the provided data table.
    /// </summary>
    /// <param name="data">The data table.</param>
    /// <returns>A collection of transformation results, one for each row.</returns>
    public override async Task<IEnumerable<TransformationResult?>> Apply(DataTable data)
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }
        var results = new List<TransformationResult?>();
        foreach (DataRow row in data.Rows)
        {
            results.Add(await Apply(row)); 
        }
        return results.AsEnumerable();
    }

    /// <summary>
    /// Gets the value for the target field based on this constant rule.
    /// </summary>
    /// <param name="sourceRecord">The source record context (largely ignored for constant value).</param>
    /// <param name="targetField">The target field descriptor, used to determine target type.</param>
    /// <returns>A transformation result containing the constant value, converted to the target field type if possible.</returns>
    public override TransformationResult GetValue(List<ImportedRecordFieldDescriptor> sourceRecord, ImportedRecordFieldDescriptor targetField)
    {
        // Use targetField.FieldType (not DataType)
        Type targetType = targetField.FieldType ?? RuleDetail?.GetType() ?? typeof(string);
        object? finalValue = RuleDetail;

        if (RuleDetail != null && targetField.FieldType != null && targetField.FieldType != typeof(string))
        {
            try
            {
                finalValue = Convert.ChangeType(RuleDetail, targetField.FieldType, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                 return TransformationResult.Failure(
                    originalValue: RuleDetail,
                    targetType: targetField.FieldType, // Use FieldType
                    errorMessage: $"ConstantValueRule.GetValue: Could not convert constant value '{RuleDetail}' to target type '{targetField.FieldType.Name}'. Error: {ex.Message}",
                    originalValueType: RuleDetail?.GetType() ?? typeof(string),
                    sourceRecordContext: sourceRecord,
                    explicitTargetFieldType: targetField.FieldType // Use FieldType
                );
            }
        }

        return TransformationResult.Success(
            originalValue: RuleDetail, 
            originalValueType: RuleDetail?.GetType() ?? typeof(string),
            currentValue: finalValue, 
            currentValueType: finalValue?.GetType() ?? targetType,
            sourceRecordContext: sourceRecord,
            targetFieldType: targetField.FieldType // Use FieldType
        );
    }
}
