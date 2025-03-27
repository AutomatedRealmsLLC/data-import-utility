using System.Collections.Immutable;

using DataImportUtility.Abstractions;
using DataImportUtility.Components.FieldMappingComponents.ValueTransformationComponents;
using DataImportUtility.Models;
using DataImportUtility.Rules;

using Microsoft.AspNetCore.Components;

namespace DataImportUtility.Components.FieldMappingComponents.OperandComponents;

/// <summary>
/// The input for a mapping rule configuration within a value transformation.
/// </summary>
public partial class MappingRuleConfigPanel : ValueTransformationInputBase<ValueTransformationBase>
{
    /// <summary>
    /// Gets or sets the mapping rule.
    /// </summary>
    [Parameter, EditorRequired, AllowNull]
    public MappingRuleBase MappingRule { get; set; }
    /// <summary>
    /// The imported record fields.
    /// </summary>
    [Parameter, EditorRequired] public ImmutableArray<ImportedRecordFieldDescriptor> FieldDescriptors { get; set; } = [];
    /// <summary>
    /// The name to display in the value transformation configuration.
    /// </summary>
    [Parameter, EditorRequired] public string FieldConfigurationDisplayName { get; set; } = "Operand";
    /// <summary>
    /// Gets or sets the callback for when the mapping rule changes.
    /// </summary>
    [Parameter]
    public EventCallback<MappingRuleBase> MappingRuleChanged { get; set; }

    private bool _showFieldTransformConfig;
    private FieldMapping? _activeFieldMapping;
    private FieldTransformation? _activeFieldTransform;

    /// <inheritdoc />
    protected override Task OnInitializedAsync()
    {
        if (MappingRule is CopyRule copyRule && copyRule.FieldTransformation?.Field is null && FieldDescriptors.Any())
        {
            // Initialize with the first field if none is set
            copyRule.FieldTransformation = new FieldTransformation(FieldDescriptors.First());
        }
        else if (MappingRule is CombineFieldsRule combineRule && combineRule.SourceFieldTransformations.IsEmpty && FieldDescriptors.Any())
        {
            // Initialize with the first field if none is set
            combineRule.AddFieldTransformation(FieldDescriptors.First());
        }

        return base.OnInitializedAsync();
    }

    /// <summary>
    /// Handles the field selection change event.
    /// </summary>
    /// <param name="e">The change event arguments.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private Task HandleFieldSelectionChanged(ChangeEventArgs e)
    {
        if (MappingRule is CopyRule copyRule)
        {
            var selectedFieldName = e.Value?.ToString();
            var selectedField = FieldDescriptors.FirstOrDefault(f => f.FieldName == selectedFieldName);

            if (selectedField is not null)
            {
                copyRule.FieldTransformation = new FieldTransformation(selectedField);
            }
            else
            {
                copyRule.FieldTransformation = new FieldTransformation();
            }

            return NotifyMappingRuleChanged();
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Handles the rule detail change event.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private Task HandleRuleDetailChanged()
    {
        return NotifyMappingRuleChanged();
    }

    /// <summary>
    /// Handles the combine field change event.
    /// </summary>
    /// <param name="e">The change event arguments.</param>
    /// <param name="fieldTransform">The field transformation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private Task HandleCombineFieldChanged(ChangeEventArgs e, FieldTransformation fieldTransform)
    {
        var selectedFieldName = e.Value?.ToString();
        var selectedField = FieldDescriptors.FirstOrDefault(f => f.FieldName == selectedFieldName);

        if (selectedField is not null)
        {
            fieldTransform.Field = selectedField;
        }
        else
        {
            fieldTransform.Field = null;
        }

        return NotifyMappingRuleChanged();
    }

    /// <summary>
    /// Handles the remove field event.
    /// </summary>
    /// <param name="fieldTransform">The field transformation to remove.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private Task HandleRemoveField(FieldTransformation fieldTransform)
    {
        if (MappingRule is CombineFieldsRule combineRule)
        {
            combineRule.RemoveFieldTransformation(fieldTransform);
            return NotifyMappingRuleChanged();
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Handles the add field event.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private Task HandleAddField()
    {
        if (MappingRule is CombineFieldsRule combineRule)
        {
            if (FieldDescriptors.Any())
            {
                combineRule.AddFieldTransformation(FieldDescriptors.First());
            }
            else
            {
                combineRule.AddFieldTransformation();
            }

            return NotifyMappingRuleChanged();
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Handles the edit transform clicked event.
    /// </summary>
    /// <param name="fieldTransform">The field transformation to edit.</param>
    private void HandleEditTransformClicked(FieldTransformation fieldTransform)
    {
        _activeFieldTransform = fieldTransform;
        // Create a temporary field mapping that wraps the current MappingRule
        _activeFieldMapping = new FieldMapping
        {
            FieldName = FieldConfigurationDisplayName,
            MappingRule = MappingRule
        };
        _showFieldTransformConfig = true;
    }

    /// <summary>
    /// Handles the transformation config closed event.
    /// </summary>
    private void HandleTransformationConfigClosed()
    {
        _showFieldTransformConfig = false;
    }

    /// <summary>
    /// Handles the click event to configure a custom field transformation.
    /// </summary>
    protected virtual void HandleConfigureCustomClicked()
    {
        _activeFieldMapping = new()
        {
            FieldName = FieldConfigurationDisplayName,
            MappingRule = MappingRule
        };
        _showFieldTransformConfig = true;
    }

    /// <summary>
    /// Notifies that the mapping rule has changed.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private Task NotifyMappingRuleChanged()
    {
        return MappingRuleChanged.HasDelegate
            ? MappingRuleChanged.InvokeAsync(MappingRule)
            : Task.CompletedTask;
    }
}
