using System.Collections.Immutable;
using System.Data;

using Microsoft.AspNetCore.Components;

using DataImportUtility.Abstractions;
using DataImportUtility.Components.Abstractions;
using DataImportUtility.Models;
using DataImportUtility.Rules;
using DataImportUtility.TransformOperations;

namespace DataImportUtility.Components.FieldMappingComponents;

/// <summary>
/// The content for the field mapper process.
/// </summary>
public partial class FieldMapperEditor : FileImportUtilityComponentBase
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

    private EditTransformOptions? _editFieldTransformOptions;
    private bool _showConfigureTransform;

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

        foreach(var fieldMapping in _editFieldMappings.Where(x => !x.IgnoreMapping))
        {
            await fieldMapping.UpdateValidationResults();
        }
    }

    private Task HandleMappingRuleChanged(ChangeEventArgs e, FieldMapping fieldMapping)
    {
        var selectedMappingRuleType = Enum.TryParse<MappingRuleType>(e.Value?.ToString(), out var rt) ? rt : MappingRuleType.IgnoreRule;
        if (fieldMapping.MappingRuleType == selectedMappingRuleType)
        {
            return Task.CompletedTask;
        }

        // TODO: Cache the mapping rules so if the user changes between them, the values are not lost
        fieldMapping.MappingRule = selectedMappingRuleType.CreateNewInstance();

        // Special rule for ConstantValueRule to set SourceFieldTranformations to the first imported field.
        if (fieldMapping.MappingRule is ConstantValueRule cvr)
        {
            var newFieldTransform = new FieldTransformation(FieldDescriptors.First());
            
            cvr.SourceFieldTranformations = [newFieldTransform];
        }

        return NotifyFieldMappingChanged(fieldMapping);
    }

    private Task HandleSelectedFieldChanged(ChangeEventArgs e, FieldMapping fieldMapping, FieldTransformation? fieldTransform = null)
    {
        fieldMapping.MappingRule ??= new CopyRule();

        var selectedFieldName = e.Value?.ToString();
        if (string.IsNullOrWhiteSpace(selectedFieldName) && fieldMapping.MappingRule.SourceFieldTranformations.Count == 0) 
        {
            return fieldMapping.UpdateValidationResults();
        }

        var selectedField = FieldDescriptors.FirstOrDefault(x => x.FieldName == selectedFieldName);

        if (fieldMapping.MappingRule.SourceFieldTranformations.Count == 0 || fieldTransform is null)
        {
            fieldMapping.MappingRule.AddFieldTransformation(selectedField);
        }
        else
        {
            fieldTransform.Field = selectedField;
        }

        return NotifyFieldMappingChanged(fieldMapping);
    }

    private static void HandleAddSourceFieldTransformClicked(FieldMapping fieldMapping)
    {
        if (fieldMapping.MappingRule is null) { return; }

        fieldMapping.MappingRule.AddFieldTransformation();
    }

    private static void HandleRemoveSourceFieldTransformClicked(FieldMapping fieldMapping, FieldTransformation fieldTransform)
    {
        fieldMapping.MappingRule?.RemoveFieldTransformation(fieldTransform);
    }

    private void HandleEditTransformClicked(FieldMapping fieldMapping, FieldTransformation fieldTransform)
    {

        _editFieldTransformOptions = new() { FieldMapping = fieldMapping, FieldTransform = fieldTransform };
        _showConfigureTransform = true;
    }

    private Task NotifyFieldMappingChanged(FieldMapping? affectedMapping = null)
        => Task.WhenAll(
            affectedMapping?.UpdateValidationResults() ?? Task.CompletedTask,
            OnDraftFieldMappingsChanged.InvokeAsync(_editFieldMappings));

    private class EditTransformOptions
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
