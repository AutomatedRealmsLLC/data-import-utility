using System.Collections.Immutable;

using DataImportUtility.Abstractions;
using DataImportUtility.Components.Abstractions;
using DataImportUtility.Models;
using DataImportUtility.ValueTransformations;

using Microsoft.AspNetCore.Components;

namespace DataImportUtility.Components.FieldMappingComponents.ValueTransformationComponents;

/// <summary>
/// Displays the configuration <see cref="ValueTransformationBase" /> to transform a value using.
/// </summary>
public partial class ValueTransformationConfiguration : DataFilePickerComponentBase
{
    private readonly List<ValueTransformationBase> _createdTransformations = [];

    /// <summary>
    /// The index for the current preview row.
    /// </summary>
    [Parameter] public uint PreviewRowIndex { get; set; }

    /// <summary>
    /// The output target to display the configuration for.
    /// </summary>
    [Parameter, EditorRequired] public FieldTransformation FieldTransformation { get; set; } = default!;
    /// <summary>
    /// The callback for when the output target is changed.
    /// </summary>
    [Parameter] public EventCallback<FieldTransformation> FieldTransformationChanged { get; set; }
    /// <summary>
    /// The index of the transformation rule.
    /// </summary>
    [Parameter, EditorRequired] public int TransformationRuleIndex { get; set; }
    /// <summary>
    /// The transformation result coming in from previous transformation operations.
    /// </summary>
    [Parameter, EditorRequired] public TransformationResult CurrentTransformationResult { get; set; } = default!;
    /// <summary>
    /// The callback for when a new transformation result is available.
    /// </summary>
    [Parameter] public EventCallback<TransformationResult> NewTransformationResultAvailable { get; set; }
    /// <summary>
    /// The imported record fields.
    /// </summary>
    [Parameter, EditorRequired] public ImmutableArray<ImportedRecordFieldDescriptor> FieldDescriptors { get; set; } = [];

    private ValueTransformationType _selectedTransformationType = ValueTransformationType.SubstringTransformation;

    private ValueTransformationBase _selectedTransformation = default!; // Initialized in OnInitializedAsync

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        _selectedTransformation = FieldTransformation.ValueTransformations[TransformationRuleIndex];
        _selectedTransformationType = _selectedTransformation.GetEnumValue();
    }

    private Task HandleTransformationChanged()
    {
        // check for an existing instance of the selected transformation type
        var transformOp = _createdTransformations.FirstOrDefault(x => x.GetType() == _selectedTransformationType.GetClassType());

        // Create a new instance of the selected transformation type and grab the current detail from the existing operation
        transformOp ??= _selectedTransformationType.CreateNewInstance()!;

        // Special handling for conditional transformation to initialize required properties
        if (transformOp is ConditionalTransformation conditionalTransform)
        {
            // Initialize comparison operation if needed
            conditionalTransform.ComparisonOperation ??= ComparisonOperationType.EqualsOperation.CreateNewInstance();

            // Initialize left operand if needed
            conditionalTransform.ComparisonOperation.LeftOperand ??= MappingRuleType.CopyRule.CreateNewInstance();

            // Add the default Left Operand field if it is not already set
            var defaultRule = conditionalTransform.ComparisonOperation.LeftOperand.SourceFieldTransformations.FirstOrDefault();
            if (defaultRule is null)
            {
                conditionalTransform.ComparisonOperation.LeftOperand.AddFieldTransformation(FieldTransformation.Field);
            }
            else
            {
                defaultRule.Field ??= FieldTransformation.Field;
            }

            // Initialize right operand if needed (for operations that require it)
            if (conditionalTransform.ComparisonOperation.GetEnumValue() != ComparisonOperationType.IsNullOperation
                && conditionalTransform.ComparisonOperation.GetEnumValue() != ComparisonOperationType.IsNotNullOperation
                && conditionalTransform.ComparisonOperation.GetEnumValue() != ComparisonOperationType.IsTrueOperation
                && conditionalTransform.ComparisonOperation.GetEnumValue() != ComparisonOperationType.IsFalseOperation)
            {
                conditionalTransform.ComparisonOperation.RightOperand ??= MappingRuleType.ConstantValueRule.CreateNewInstance();
            }

            // Initialize mapping rules
            conditionalTransform.TrueMappingRule ??= MappingRuleType.CopyRule.CreateNewInstance();
            conditionalTransform.FalseMappingRule ??= MappingRuleType.ConstantValueRule.CreateNewInstance();
        }

        FieldTransformation.ReplaceTransformation(_selectedTransformation, transformOp);

        _selectedTransformation = transformOp;

        return NotifyTransformationChanged();
    }

    private Task NotifyTransformationChanged()
        => FieldTransformationChanged.InvokeAsync(FieldTransformation);
}
