// Filepath: d:\git\AutomatedRealms\data-import-utility\src\DataImportUtility\AutomatedRealms.DataImportUtility.Core\Rules\StaticValueRule.cs
using System.Data;
using System.Text.Json.Serialization;

using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Enums;
using AutomatedRealms.DataImportUtility.Abstractions.Models;

namespace AutomatedRealms.DataImportUtility.Core.Rules
{
    /// <summary>
    /// A mapping rule that always returns a static, predefined value.
    /// Useful for providing constant values as operands or direct outputs.
    /// Note: This rule currently uses MappingRuleType.StaticValueRule.
    /// </summary>
    public class StaticValueRule : MappingRuleBase
    {
        private object? _staticValue;
        private Type? _staticValueType;

        /// <summary>
        /// Gets or sets the static value this rule provides.
        /// Setting this value also updates <see cref="RuleDetail"/> and internal type information.
        /// </summary>
        public object? Value
        {
            get => _staticValue;
            set
            {
                _staticValue = value;
                _staticValueType = value?.GetType();
                RuleDetail = value?.ToString();
            }
        }        /// <summary>
                 /// Gets or sets the string representation of the static value.
                 /// This is primarily for serialization and informational purposes.
                 /// </summary>
        public new string? RuleDetail { get; set; }

        /// <summary>
        /// Gets the type of the mapping rule.
        /// </summary>
        public override MappingRuleType RuleType => MappingRuleType.StaticValueRule;

        /// <summary>
        /// Gets the enum member name for the mapping rule type.
        /// </summary>
        public override string EnumMemberName => MappingRuleType.StaticValueRule.ToString();

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
        public override bool IsEmpty => Value == null;

        /// <summary>
        /// Gets the order value for the enum member.
        /// </summary>
        public override int EnumMemberOrder => 7;        /// <summary>
                                                         /// Initializes a new instance of the <see cref="StaticValueRule"/> class with a specific static value.
                                                         /// </summary>
                                                         /// <param name="value">The static value this rule should represent.</param>
        public StaticValueRule(object? value)
        {
            this.Value = value; // Uses the property setter to initialize related fields
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StaticValueRule"/> class with a specific static value and rule detail.
        /// </summary>
        /// <param name="value">The static value this rule should represent.</param>
        /// <param name="ruleDetail">Additional information about the rule.</param>
        public StaticValueRule(object? value, string ruleDetail)
        {
            this.Value = value; // Uses the property setter to initialize related fields
            this.RuleDetail = ruleDetail;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StaticValueRule"/> class.
        /// Parameterless constructor for serialization.
        /// </summary>
        public StaticValueRule()
        {
            // Value will be null by default, can be set via property or deserialization.
        }

        /// <summary>
        /// Gets the <see cref="MappingRuleType"/> enum value for this rule.
        /// </summary>
        /// <returns>The <see cref="MappingRuleType"/> enum value.</returns>
        public override MappingRuleType GetEnumValue() => this.RuleType;

        private TransformationResult CreateTransformationResult(Type? targetType, ITransformationContext? context = null)
        {
            DataRow? contextRow = context?.Record;
            List<ImportedRecordFieldDescriptor>? sourceRecordContext = context?.SourceRecordContext;

            try
            {
                object? finalValue = _staticValue;
                Type? finalValueType = _staticValueType;
                var log = new List<string> { $"Retrieved static value: '{_staticValue?.ToString() ?? "null"}'." };

                if (targetType != null)
                {
                    if (_staticValue == null)
                    {
                        if (targetType.IsValueType && Nullable.GetUnderlyingType(targetType) == null)
                        {
                            return TransformationResult.Failure(
                                originalValue: _staticValue,
                                targetType: targetType, // This is the intended target type for conversion
                                errorMessage: $"Cannot assign null static value to non-nullable target type '{targetType.Name}'.",
                                originalValueType: _staticValueType,
                                record: contextRow,
                                sourceRecordContext: sourceRecordContext,
                                explicitTargetFieldType: targetType // Pass it as explicitTargetFieldType
                            );
                        }
                        finalValueType = targetType;
                        log.Add($"Static value is null, target type '{targetType.Name}' allows null.");
                    }
                    else if (_staticValueType == null || !targetType.IsAssignableFrom(_staticValueType))
                    {
                        log.Add($"Attempting conversion of static value from type '{_staticValueType?.Name ?? "unknown"}' to target type '{targetType.Name}'.");
                        finalValue = Convert.ChangeType(_staticValue, targetType);
                        finalValueType = targetType;
                        log.Add($"Conversion successful. New value: '{finalValue?.ToString() ?? "null"}'.");
                    }
                    else
                    {
                        log.Add($"Static value type '{_staticValueType?.Name}' is assignable to target type '{targetType.Name}'. No conversion needed.");
                    }
                }

                return TransformationResult.Success(
                    originalValue: _staticValue,
                    originalValueType: _staticValueType,
                    currentValue: finalValue,
                    currentValueType: finalValueType,
                    appliedTransformations: log,
                    record: contextRow,
                    sourceRecordContext: sourceRecordContext,
                    targetFieldType: targetType // Pass the targetType used for conversion logic
                );
            }
            catch (Exception ex)
            {
                return TransformationResult.Failure(
                    originalValue: _staticValue,
                    targetType: targetType, // This is the intended target type for conversion
                    errorMessage: $"Failed to process static value for target type '{targetType?.Name ?? "unknown"}': {ex.Message}",
                    originalValueType: _staticValueType,
                    record: contextRow,
                    sourceRecordContext: sourceRecordContext,
                    explicitTargetFieldType: targetType // Pass it as explicitTargetFieldType
                );
            }
        }

        /// <inheritdoc />
        public override Task<TransformationResult?> Apply(ITransformationContext context)
        {
            Type? targetType = context?.TargetFieldType ?? _staticValueType;
            return Task.FromResult<TransformationResult?>(CreateTransformationResult(targetType, context));
        }

        /// <summary>
        /// Applies the static value rule in a context where no specific data table or row is provided.
        /// </summary>
        /// <returns>A collection containing a single transformation result with the static value.</returns>
        public override async Task<IEnumerable<TransformationResult?>> Apply()
        {
            var context = TransformationResult.Success(null, null, null, null, new List<string>(), targetFieldType: _staticValueType);
            return new List<TransformationResult?> { await Apply(context) };
        }

        /// <summary>
        /// Applies the static value rule to all rows in the provided data table.
        /// </summary>
        /// <param name="data">The data table (not directly used by this rule for value retrieval, but for context).</param>
        /// <returns>An enumerable collection of transformation results, one for each row, each containing the static value.</returns>
        public override async Task<IEnumerable<TransformationResult?>> Apply(DataTable data)
        {
            if (data == null) return new List<TransformationResult?>();

            var results = new List<TransformationResult?>();
            foreach (DataRow row in data.Rows)
            {
                var rowContext = TransformationResult.Success(null, null, null, null, new List<string>(), record: row, targetFieldType: _staticValueType);
                results.Add(await Apply(rowContext));
            }
            return results;
        }

        /// <summary>
        /// Applies the static value rule to the provided data row.
        /// </summary>
        /// <param name="dataRow">The data row (not directly used by this rule for value retrieval, but for context).</param>
        /// <returns>A transformation result containing the static value.</returns>
        public override async Task<TransformationResult?> Apply(DataRow dataRow)
        {
            var context = TransformationResult.Success(null, null, null, null, new List<string>(), record: dataRow, targetFieldType: _staticValueType);
            return await Apply(context);
        }

        /// <summary>
        /// Gets the static value, attempting conversion if a target field type is specified.
        /// </summary>
        /// <param name="sourceRecordContext">The source record context (passed to CreateTransformationResult).</param>
        /// <param name="targetField">The descriptor of the target field, used to determine the target type for potential conversion.</param>
        /// <returns>A <see cref="TransformationResult"/> containing the static value, possibly converted.</returns>
        public override TransformationResult GetValue(List<ImportedRecordFieldDescriptor> sourceRecordContext, ImportedRecordFieldDescriptor targetField)
        {
            var context = TransformationResult.Success(null, null, null, null, new List<string>(), sourceRecordContext: sourceRecordContext, targetFieldType: targetField?.FieldType);
            return CreateTransformationResult(targetField?.FieldType, context);
        }

        /// <summary>
        /// Creates a clone of this <see cref="StaticValueRule"/> instance.
        /// </summary>
        /// <returns>A new <see cref="StaticValueRule"/> instance with the same static value.</returns>
        public override MappingRuleBase Clone()
        {
            var clone = new StaticValueRule
            {
                Value = this.Value
            };
            base.CloneBaseProperties(clone);
            return clone;
        }
    }
}
