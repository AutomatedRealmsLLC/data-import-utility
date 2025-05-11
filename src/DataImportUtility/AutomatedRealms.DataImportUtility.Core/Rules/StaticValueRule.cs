// Filepath: d:\git\AutomatedRealms\data-import-utility\src\DataImportUtility\AutomatedRealms.DataImportUtility.Core\Rules\StaticValueRule.cs
using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Models;
using AutomatedRealms.DataImportUtility.Abstractions.Enums; // Required for RuleType
using System;
using System.Data;
using System.Threading.Tasks;
using System.Collections.Generic; // Required for List<string> in TransformationResult

namespace AutomatedRealms.DataImportUtility.Core.Rules
{
    /// <summary>
    /// A mapping rule that always returns a static, predefined value.
    /// This is useful for providing constant values as operands in other rules or operations,
    /// particularly within the ComparisonOperationFactory.
    /// </summary>
    public class StaticValueRule : MappingRuleBase
    {
        private readonly object? _staticValue;
        private readonly Type? _staticValueType;

        /// <inheritdoc />
        public override RuleType RuleType => RuleType.StaticValue;

        /// <inheritdoc />
        public override string EnumMemberName => nameof(StaticValueRule);

        /// <inheritdoc />
        public override string DisplayName { get; }

        /// <inheritdoc />
        public override string Description { get; }

        /// <inheritdoc />
        // A StaticValueRule is considered empty if its underlying static value is null.
        public override bool IsEmpty => _staticValue == null;

        /// <summary>
        /// Initializes a new instance of the <see cref="StaticValueRule"/> class.
        /// </summary>
        /// <param name="value">The static value this rule should represent.</param>
        /// <param name="descriptionPrefix">An optional prefix for the description, often indicating context (e.g., "Conditional Comparison Value").</param>
        public StaticValueRule(object? value, string descriptionPrefix = "Static Value")
        {
            _staticValue = value;
            _staticValueType = value?.GetType();
            // Keep DisplayName generic for the type, specific context can be logged by caller.
            DisplayName = "Static Value"; 
            Description = $"{descriptionPrefix}: '{_staticValue ?? "null"}'.";
            // RuleDetail could store the string representation of the value if needed for other purposes.
            RuleDetail = _staticValue?.ToString(); 
        }

        /// <inheritdoc />
        public override Task<TransformationResult> GetValue(DataRow row, Type targetType)
        {
            // Conditional rules are not typically applied to StaticValueRule itself,
            // as it's a provider of a value, not a consumer that reacts to conditions.
            // If AreConditionalRulesMetAsync was called, it would use the (likely empty) ConditionalRules of this instance.

            try
            {
                object? finalValue = _staticValue;
                Type? finalValueType = _staticValueType;

                if (targetType != null) // Only attempt conversion if a targetType is specified
                {
                    if (_staticValue == null)
                    {
                        if (targetType.IsValueType && Nullable.GetUnderlyingType(targetType) == null)
                        {
                            return Task.FromResult(TransformationResult.Failure(
                                originalValue: _staticValue, targetType: targetType,
                                errorMessage: $"Cannot assign null static value to non-nullable target type '{targetType.Name}'.",
                                originalValueType: _staticValueType,
                                record: row, // Pass along context even if not directly used by this rule's core logic
                                tableDefinition: this.ParentTableDefinition
                            ));
                        }
                        // If targetType is nullable or a reference type, null is acceptable.
                        finalValueType = targetType;
                    }
                    else if (_staticValueType == null || !targetType.IsAssignableFrom(_staticValueType))
                    {
                        // Attempt conversion if types are different and not assignable
                        finalValue = Convert.ChangeType(_staticValue, targetType);
                        finalValueType = targetType;
                    }
                }
                
                return Task.FromResult(TransformationResult.Success(
                    originalValue: _staticValue,
                    originalValueType: _staticValueType,
                    currentValue: finalValue,
                    currentValueType: finalValueType,
                    appliedTransformations: new List<string> { "Retrieved static value." },
                    record: row,
                    tableDefinition: this.ParentTableDefinition
                ));
            }
            catch (Exception ex)
            {
                return Task.FromResult(TransformationResult.Failure(
                    originalValue: _staticValue, targetType: targetType,
                    errorMessage: $"Failed to convert static value from type '{_staticValueType?.Name ?? "unknown"}' to target type '{targetType?.Name ?? "unknown"}': {ex.Message}",
                    originalValueType: _staticValueType,
                    record: row,
                    tableDefinition: this.ParentTableDefinition
                ));
            }
        }
        
        /// <inheritdoc />
        /// <remarks>
        /// StaticValueRule itself does not support conditional execution based on other rules.
        /// It simply provides a value.
        /// </remarks>
        protected override Task<ComparisonOperationBase?> GetConfiguredOperationAsync(ConditionalRule conditionalRule, DataRow row)
        {
            // StaticValueRule does not have its own conditional logic configuration in the typical sense.
            // It's a provider of a value, not a rule that executes conditionally based on its own sub-rules.
            return Task.FromResult<ComparisonOperationBase?>(null);
        }

        // The base Clone() method using MemberwiseClone is generally sufficient for StaticValueRule,
        // as _staticValue is typically an immutable type (like string, int) or a type where
        // a shallow copy is acceptable in the contexts it's used (e.g., within ComparisonOperationFactory).
        // If _staticValue could be a complex mutable object requiring a deep clone, this would need overriding.
        // ParentTableDefinition, SourceFieldTransformations, and ConditionalRules are handled by base.Clone().
        // For StaticValueRule, SourceFieldTransformations and ConditionalRules are expected to be empty.
    }
}
