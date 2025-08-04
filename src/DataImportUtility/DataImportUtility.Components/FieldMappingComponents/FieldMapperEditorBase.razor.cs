using System.Collections.Immutable;

using DataImportUtility.Abstractions;
using DataImportUtility.Components.Abstractions;
using DataImportUtility.Models;
using DataImportUtility.Rules;

using Microsoft.AspNetCore.Components;

namespace DataImportUtility.Components.FieldMappingComponents;

/// <summary>
/// The base class for the FieldMapperEditor component.
/// </summary>
public abstract partial class FieldMapperEditorBase : DataFilePickerComponentBase
{
    /// <summary>
    /// The field mappings to configure.
    /// </summary>
    [Parameter, EditorRequired, AllowNull] public ImmutableArray<FieldMapping> FieldMappings { get; set; }
    /// <summary>
    /// The field descriptors for the imported record.
    /// </summary>
    [Parameter, EditorRequired, AllowNull] public ImmutableArray<ImportedRecordFieldDescriptor> FieldDescriptors { get; set; }
    /// <summary>
    /// The callback for when the field mappings are changed.
    /// </summary>
    [Parameter] public EventCallback<IEnumerable<FieldMapping>> OnDraftFieldMappingsChanged { get; set; }

    /// <summary>
    /// The options for editing a field transformation.
    /// </summary>
    protected EditTransformOptions? _editFieldTransformOptions;
    /// <summary>
    /// Whether to show the dialog for editing a field transformation.
    /// </summary>
    protected bool _showConfigureTransform;

    /// <summary>
    /// A copy of the field mappings to edit.
    /// </summary>
    /// <remarks>
    /// Changes are not committed to the provided <see cref="FieldMappings"/>. Instead, they are cloned and edited in this component. The <see cref="OnDraftFieldMappingsChanged"/> will notify parent components when the edited field mappings are changed.
    /// </remarks>
    protected List<FieldMapping> _editFieldMappings = [];

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        _editFieldMappings = FieldMappings
            .Select(fm => fm.Clone())
            .ToList();

        foreach (var fieldMapping in _editFieldMappings.Where(x => !x.IgnoreMapping))
        {
            await fieldMapping.UpdateValidationResults();
        }
    }

    /// <summary>
    /// Handles the change event for a mapping rule.
    /// </summary>
    /// <param name="e">The event arguments.</param>
    /// <param name="fieldMapping">The field mapping to update.</param>
    protected virtual Task HandleMappingRuleChanged(ChangeEventArgs e, FieldMapping fieldMapping)
    {
        var selectedMappingRuleType = Enum.TryParse<MappingRuleType>(e.Value?.ToString(), out var rt) ? rt : MappingRuleType.IgnoreRule;
        if (fieldMapping.MappingRuleType == selectedMappingRuleType)
        {
            return Task.CompletedTask;
        }

        // TODO: Cache the mapping rules so if the user changes between them, the values are not lost
        fieldMapping.MappingRule = selectedMappingRuleType.CreateNewInstance();

        // Special rule for ConstantValueRule to set SourceFieldTransformations to the first imported field.
        switch (fieldMapping.MappingRule)
        {
            case ConstantValueRule cvr when cvr.SourceFieldTransformations.Count == 0:
                {
                    var newFieldTransform = new FieldTransformation(FieldDescriptors.First());
                    cvr.SourceFieldTransformations = [newFieldTransform];
                }
                break;
            case CustomFieldlessRule cfr when cfr.SourceFieldTransformations.Count == 0:
                {

                    var newFieldTransform = new FieldTransformation(FieldDescriptors.First());
                    cfr.SourceFieldTransformations = [newFieldTransform];
                }
                break;
        }

        return NotifyFieldMappingChanged(fieldMapping);
    }

    /// <summary>
    /// Handles the change event for a selected field.
    /// </summary>
    /// <param name="e">The event arguments.</param>
    /// <param name="fieldMapping">The field mapping to update.</param>
    /// <param name="fieldTransform">The field transformation to update, if any.</param>
    protected virtual Task HandleSelectedFieldChanged(ChangeEventArgs e, FieldMapping fieldMapping, FieldTransformation? fieldTransform = null)
    {
        fieldMapping.MappingRule ??= new CopyRule();

        var selectedFieldName = e.Value?.ToString();
        if (string.IsNullOrWhiteSpace(selectedFieldName) && fieldMapping.MappingRule.SourceFieldTransformations.Count == 0)
        {
            return fieldMapping.UpdateValidationResults();
        }

        var selectedField = FieldDescriptors.FirstOrDefault(x => x.FieldName == selectedFieldName);

        if (fieldMapping.MappingRule.SourceFieldTransformations.Count == 0 || fieldTransform is null)
        {
            fieldMapping.MappingRule.AddFieldTransformation(selectedField);
        }
        else
        {
            fieldTransform.Field = selectedField;
        }

        return NotifyFieldMappingChanged(fieldMapping);
    }

    /// <summary>
    /// Handles the click event to add a source field transformation.
    /// </summary>
    /// <param name="fieldMapping">The field mapping to update.</param>
    protected static void HandleAddSourceFieldTransformClicked(FieldMapping fieldMapping)
    {
        if (fieldMapping.MappingRule is null) { return; }

        fieldMapping.MappingRule.AddFieldTransformation();
    }

    /// <summary>
    /// Handles the click event to remove a source field transformation.
    /// </summary>
    /// <param name="fieldMapping">The field mapping to update.</param>
    /// <param name="fieldTransform">The field transformation to remove.</param>
    protected static void HandleRemoveSourceFieldTransformClicked(FieldMapping fieldMapping, FieldTransformation fieldTransform)
    {
        fieldMapping.MappingRule?.RemoveFieldTransformation(fieldTransform);
    }

    /// <summary>
    /// Handles the click event to edit a field transformation.
    /// </summary>
    /// <param name="fieldMapping">The field mapping to update.</param>
    /// <param name="fieldTransform">The field transformation to edit.</param>
    protected virtual void HandleEditTransformClicked(FieldMapping fieldMapping, FieldTransformation fieldTransform)
    {
        _editFieldTransformOptions = new() { FieldMapping = fieldMapping, FieldTransform = fieldTransform };
        _showConfigureTransform = true;
    }

    /// <summary>
    /// Handles the click event to configure a custom field transformation.
    /// </summary>
    /// <param name="fieldMapping">The field mapping to update.</param>
    protected virtual void HandleConfigureCustomClicked(FieldMapping fieldMapping)
    {
        _editFieldTransformOptions = new() { FieldMapping = fieldMapping, FieldTransform = new() };
        _showConfigureTransform = true;
    }

    /// <summary>
    /// Notifies that a field mapping has changed.
    /// </summary>
    /// <param name="affectedMapping">The affected field mapping, if any.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    protected virtual Task NotifyFieldMappingChanged(FieldMapping? affectedMapping = null)
        => Task.WhenAll(
            affectedMapping?.UpdateValidationResults() ?? Task.CompletedTask,
            OnDraftFieldMappingsChanged.InvokeAsync(_editFieldMappings));

    /// <summary>
    /// The options for editing a field transformation.
    /// </summary>
    protected class EditTransformOptions
    {
        /// <summary>
        /// The field mapping the edit is for.
        /// </summary>
        public required FieldMapping FieldMapping { get; init; }
        /// <summary>
        /// The field transformation to edit.
        /// </summary>
        public required FieldTransformation FieldTransform { get; set; }
    }
}
