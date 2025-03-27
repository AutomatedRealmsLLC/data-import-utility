using System.Collections.Immutable;

using DataImportUtility.Abstractions;
using DataImportUtility.Models;
using DataImportUtility.Rules;
using DataImportUtility.ValueTransformations;

using Microsoft.AspNetCore.Components;

namespace DataImportUtility.Components.FieldMappingComponents.ValueTransformationComponents;

/// <summary>
/// The input for a conditional value transformation.
/// </summary>
public partial class ConditionalValueTransformationInput : ValueTransformationInputBase<ConditionalTransformation>
{
    private ComparisonOperationType _selectedComparisonType = ComparisonOperationType.EqualsOperation;
    private MappingRuleType _selectedLeftOperandType = MappingRuleType.CopyRule;
    private MappingRuleType _selectedRightOperandType = MappingRuleType.ConstantValueRule;
    private MappingRuleType _selectedTrueMappingType = MappingRuleType.CopyRule;
    private MappingRuleType _selectedFalseMappingType = MappingRuleType.ConstantValueRule;

    // Keep track of the most recent right operand in case the user changes the operator to something 
    // that doesn't use a right operand.  This will allow us to restore the most recent right operand
    // when changing back to something that does use the right operand.
    private MappingRuleBase _rightOperand = new ConstantValueRule();

    /// <summary>
    /// The imported record fields.
    /// </summary>
    [Parameter, EditorRequired] public ImmutableArray<ImportedRecordFieldDescriptor> FieldDescriptors { get; set; } = [];
    /// <summary>
    /// The transformation result coming in from previous transformation operations.
    /// </summary>
    [Parameter, EditorRequired, AllowNull] public TransformationResult CurrentTransformationResult { get; set; }

    private uint _prevPreviewRowIndex = 0;

    private TransformationResult? _leftOperandPreviewResult;
    private TransformationResult? _rightOperandPreviewResult;
    private TransformationResult? _trueMappingPreviewResult;
    private TransformationResult? _falseMappingPreviewResult;

    private bool NeedsRightOperand =>
        _selectedComparisonType != ComparisonOperationType.IsNullOperation &&
        _selectedComparisonType != ComparisonOperationType.IsNotNullOperation &&
        _selectedComparisonType != ComparisonOperationType.IsTrueOperation &&
        _selectedComparisonType != ComparisonOperationType.IsFalseOperation;

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        // Ensure all components are properly initialized
        EnsureComponentsInitialized();

        // Initialize selections from existing configuration if available
        if (ValueTransformation.ComparisonOperation is not null)
        {
            _selectedComparisonType = ValueTransformation.ComparisonOperation.GetEnumValue();

            if (ValueTransformation.ComparisonOperation.LeftOperand is not null)
            {
                _selectedLeftOperandType = ValueTransformation.ComparisonOperation.LeftOperand.GetEnumValue();
            }

            if (ValueTransformation.ComparisonOperation.RightOperand is not null && NeedsRightOperand)
            {
                _selectedRightOperandType = ValueTransformation.ComparisonOperation.RightOperand.GetEnumValue();
                _rightOperand = ValueTransformation.ComparisonOperation.RightOperand;
            }
        }

        if (ValueTransformation.TrueMappingRule is not null)
        {
            _selectedTrueMappingType = ValueTransformation.TrueMappingRule.GetEnumValue();
        }

        if (ValueTransformation.FalseMappingRule is not null)
        {
            _selectedFalseMappingType = ValueTransformation.FalseMappingRule.GetEnumValue();
        }

        await UpdateTransformationPreview();
    }

    /// <inheritdoc />
    protected override Task OnParametersSetAsync()
    {
        if (_prevPreviewRowIndex != PreviewRowIndex)
        {
            _prevPreviewRowIndex = PreviewRowIndex;
            return UpdateTransformationPreview();
        }

        return Task.CompletedTask;
    }

    private void EnsureComponentsInitialized()
    {
        // Initialize the comparison operation if needed
        ValueTransformation.ComparisonOperation ??= _selectedComparisonType.CreateNewInstance();

        // Initialize the left operand if needed
        ValueTransformation.ComparisonOperation.LeftOperand ??= _selectedLeftOperandType.CreateNewInstance();

        // Initialize the right operand if needed and applicable
        if (NeedsRightOperand)
        {
            ValueTransformation.ComparisonOperation.RightOperand ??= _selectedRightOperandType.CreateNewInstance();
        }

        // Initialize the true mapping rule if needed
        ValueTransformation.TrueMappingRule ??= _selectedTrueMappingType.CreateNewInstance();

        // Initialize the false mapping rule if needed
        ValueTransformation.FalseMappingRule ??= _selectedFalseMappingType.CreateNewInstance();
    }

    private Task HandleComparisonTypeChanged()
    {
        var newComparisonOp = _selectedComparisonType.CreateNewInstance();

        // Preserve any existing operands if possible
        if (ValueTransformation.ComparisonOperation is not null)
        {
            newComparisonOp.LeftOperand = ValueTransformation.ComparisonOperation.LeftOperand;

            if (NeedsRightOperand)
            {
                newComparisonOp.RightOperand = _rightOperand;
            }
            else if (ValueTransformation.ComparisonOperation.RightOperand is not null)
            {
                _rightOperand = ValueTransformation.ComparisonOperation.RightOperand;
            }
        }

        ValueTransformation.ComparisonOperation = newComparisonOp;
        EnsureComponentsInitialized();

        return Task.WhenAny(
            HandleOperationDetailChanged(),
            UpdateTransformationPreview());
    }

    private Task HandleLeftOperandTypeChanged()
    {
        if (ValueTransformation.ComparisonOperation is null)
        {
            return Task.CompletedTask;
        }

        ValueTransformation.ComparisonOperation.LeftOperand = _selectedLeftOperandType.CreateNewInstance();
        return Task.WhenAny(
            HandleOperationDetailChanged(),
            UpdateTransformationPreview());
    }

    private Task HandleRightOperandTypeChanged()
    {
        if (ValueTransformation.ComparisonOperation is null || !NeedsRightOperand)
        {
            return Task.CompletedTask;
        }

        ValueTransformation.ComparisonOperation.RightOperand = _selectedRightOperandType.CreateNewInstance();
        return Task.WhenAny(
            HandleOperationDetailChanged(),
            UpdateTransformationPreview());
    }

    private Task HandleTrueMappingTypeChanged()
    {
        ValueTransformation.TrueMappingRule = _selectedTrueMappingType.CreateNewInstance();
        return Task.WhenAny(
            HandleOperationDetailChanged(),
            UpdateTransformationPreview());
    }

    private Task HandleFalseMappingTypeChanged()
    {
        ValueTransformation.FalseMappingRule = _selectedFalseMappingType.CreateNewInstance();
        return Task.WhenAny(
            HandleOperationDetailChanged(),
            UpdateTransformationPreview());
    }

    private Task HandleLeftOperandChanged(MappingRuleBase mappingRule) =>
        Task.WhenAny(
            HandleOperationDetailChanged(),
            UpdateTransformationPreview());

    private Task HandleRightOperandChanged(MappingRuleBase mappingRule) =>
        Task.WhenAny(
            HandleOperationDetailChanged(),
            UpdateTransformationPreview());

    private Task HandleTrueMappingChanged(MappingRuleBase mappingRule) =>
        Task.WhenAny(
            HandleOperationDetailChanged(),
            UpdateTransformationPreview());

    private Task HandleFalseMappingChanged(MappingRuleBase mappingRule) =>
        Task.WhenAny(
            HandleOperationDetailChanged(),
            UpdateTransformationPreview());

    private async Task UpdateTransformationPreview()
    {
        _leftOperandPreviewResult = null;
        _rightOperandPreviewResult = null;
        _trueMappingPreviewResult = null;
        _falseMappingPreviewResult = null;
        if (ValueTransformation.ComparisonOperation is not null)
        {
            if (ValueTransformation.ComparisonOperation.LeftOperand is not null)
            {
                // Get the current row's value for the left operand's field
                var currentPreviewField = ValueTransformation.ComparisonOperation.LeftOperand.SourceFieldTransformations?.FirstOrDefault()?.Field;
                if (currentPreviewField is not null && currentPreviewField.ValueSet.Length > PreviewRowIndex)
                {
                    var curValue = currentPreviewField.ValueSet[PreviewRowIndex]?.ToString();
                    _leftOperandPreviewResult = await ValueTransformation.ComparisonOperation.LeftOperand.Apply(new TransformationResult() { OriginalValue = curValue, Value = curValue });
                }
            }
            if (NeedsRightOperand && ValueTransformation.ComparisonOperation.RightOperand is not null)
            {
                // Get the current row's value for the right operand's field
                var currentPreviewField = ValueTransformation.ComparisonOperation.RightOperand.SourceFieldTransformations?.FirstOrDefault()?.Field;
                if (currentPreviewField is not null && currentPreviewField.ValueSet.Length > PreviewRowIndex)
                {
                    var curValue = currentPreviewField.ValueSet[PreviewRowIndex]?.ToString();
                    _rightOperandPreviewResult = await ValueTransformation.ComparisonOperation.RightOperand.Apply(new TransformationResult() { OriginalValue = curValue, Value = curValue });
                }
            }
        }
        if (ValueTransformation.TrueMappingRule is not null)
        {
            // Get the current row's value for the true mapping rule's field
            var currentPreviewField = ValueTransformation.TrueMappingRule.SourceFieldTransformations?.FirstOrDefault()?.Field;
            if (currentPreviewField is not null && currentPreviewField.ValueSet.Length > PreviewRowIndex)
            {
                var curValue = currentPreviewField.ValueSet[PreviewRowIndex]?.ToString();
                _trueMappingPreviewResult = await ValueTransformation.TrueMappingRule.Apply(new TransformationResult() { OriginalValue = curValue, Value = curValue });
            }
        }
        if (ValueTransformation.FalseMappingRule is not null)
        {
            // Get the current row's value for the false mapping rule's field
            var currentPreviewField = ValueTransformation.FalseMappingRule.SourceFieldTransformations?.FirstOrDefault()?.Field;
            if (currentPreviewField is not null && currentPreviewField.ValueSet.Length > PreviewRowIndex)
            {
                var curValue = currentPreviewField.ValueSet[PreviewRowIndex]?.ToString();
                _falseMappingPreviewResult = await ValueTransformation.FalseMappingRule.Apply(new TransformationResult() { OriginalValue = curValue, Value = curValue });
            }
        }
    }
}