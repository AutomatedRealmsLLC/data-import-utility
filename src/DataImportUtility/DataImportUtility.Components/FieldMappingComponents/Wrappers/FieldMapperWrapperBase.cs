using System.Collections.Immutable;

using DataImportUtility.Components.Abstractions;
using DataImportUtility.Components.Extensions;
using DataImportUtility.Models;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace DataImportUtility.Components.FieldMappingComponents.Wrappers;

/// <summary>
/// The base class for all field mapper wrappers.
/// </summary>
public abstract class FieldMapperWrapperBase : FileImportUtilityComponentBase
{
    [Inject] private IServiceProvider ServiceProvider { get; set; } = default!;

    /// <summary>
    /// Whether to show the dialog.
    /// </summary>
    [Parameter] public bool Show { get; set; } = true;
    /// <summary>
    /// The callback for when the show property is changed.
    /// </summary>
    [Parameter] public EventCallback<bool> ShowChanged { get; set; }
    /// <summary>
    /// The field mappings to configure.
    /// </summary>
    /// <remarks>
    /// This collection is cloned when the component is initialized and changes are not committed to the <see cref="FieldMappings" /> directly.
    /// </remarks>
    [Parameter, EditorRequired] public ImmutableArray<FieldMapping> FieldMappings { get; set; } = default!;
    /// <summary>
    /// The field mappings to configure.
    /// </summary>
    /// <remarks>
    /// This collection is cloned when the component is initialized and changes are not committed to the <see cref="FieldMappings" /> directly.
    /// </remarks>
    [Parameter, EditorRequired] public ImmutableArray<ImportedRecordFieldDescriptor> FieldDescriptors { get; set; } = default!;
    /// <summary>
    /// The callback for when the set of edit field mappings are changed.
    /// </summary>
    [Parameter] public EventCallback<IEnumerable<FieldMapping>> OnDraftFieldMappingsChanged { get; set; }
    /// <summary>
    /// The callback for when the commit button is clicked.
    /// </summary>
    [Parameter] public EventCallback<IEnumerable<FieldMapping>> OnCommitClicked { get; set; }
    /// <summary>
    /// The callback for when the cancel button is clicked.
    /// </summary>
    [Parameter] public EventCallback OnCancelClicked { get; set; }

    /// <summary>
    /// The field mapper editor component to use for editing the field mappings.
    /// </summary>
    protected Type _fieldMapperEditorType = typeof(FieldMapperEditor);

    /// <summary>
    /// A copy of the field mappings to edit.
    /// </summary>
    /// <remarks>
    /// Changes are not committed until the <see cref="HandleCommitClicked" /> method is called and the parent component is notified using the <see cref="OnCommitClicked" /> event.
    /// </remarks>
    protected List<FieldMapping> _editFieldMappings = [];

    /// <summary>
    /// The missing required fields.
    /// </summary>
    protected IEnumerable<FieldMapping> MissingReqFields => _editFieldMappings.Where(x => x.Required && x.IgnoreMapping);

    /// <inheritdoc />
    protected override Task OnInitializedAsync()
    {
        _editFieldMappings = [.. FieldMappings];

        // Use the field mapper editor type from the UI Options if one is specified
        var uiOptions = ServiceProvider.GetService<DataFileMapperUiOptions>();

        if (uiOptions?.FieldMapperEditorComponentType is not null)
        {
            _fieldMapperEditorType = uiOptions.FieldMapperEditorComponentType;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Render the field mapper editor component.
    /// </summary>
    public RenderFragment RenderFieldMapperEditor => builder =>
    {
        var curElem = 0;
        builder.OpenComponent(curElem++, _fieldMapperEditorType);
        // Set the parameters
        builder.AddAttribute(curElem++, nameof(FieldMapperEditorBase.FieldMappings), FieldMappings);
        builder.AddAttribute(curElem++, nameof(FieldMapperEditorBase.FieldDescriptors), FieldDescriptors);
        builder.AddAttribute(curElem++, nameof(FieldMapperEditorBase.ApplyDefaultCss), ApplyDefaultCss);
        builder.AddAttribute(curElem++, nameof(FieldMapperEditorBase.OnDraftFieldMappingsChanged), EventCallback.Factory.Create<IEnumerable<FieldMapping>>(this, HandleFieldMappingChanged));
        builder.CloseComponent();
    };

    /// <summary>
    /// Handles when the field mappings are changed.
    /// </summary>
    /// <param name="updatedMappings">
    /// The updated field mappings.
    /// </param>
    protected Task HandleFieldMappingChanged(IEnumerable<FieldMapping> updatedMappings)
    {
        _editFieldMappings = updatedMappings.ToList();
        return OnDraftFieldMappingsChanged.InvokeAsync(_editFieldMappings);
    }

    /// <summary>
    /// Commits the field mapping changes.
    /// </summary>
    protected Task HandleCommitClicked()
    {
        // Validate that all of the required fields are mapped
        if (MissingReqFields.Any())
        {

        }

        // TODO: Validate the value length constraints

        // TODO: Validate the Map From fields have valid rules definitions while removing empty ones

        return Task.WhenAll(
            OnCommitClicked.InvokeAsync(_editFieldMappings),
            Close());
    }

    /// <summary>
    /// Hides the field mapper wrapper.
    /// </summary>
    protected Task Close()
    {
        Show = false;
        return ShowChanged.InvokeAsync(Show);
    }
}
