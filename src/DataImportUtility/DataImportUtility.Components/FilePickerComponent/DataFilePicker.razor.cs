using DataImportUtility.Components.Abstractions;
using DataImportUtility.Components.Models;
using DataImportUtility.Components.State;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace DataImportUtility.Components.FilePickerComponent;

/// <summary>
/// The data file reader component.
/// </summary>
public partial class DataFilePicker : DataFilePickerComponentBase
{
    /// <summary>
    /// The default title for the data file picker.
    /// </summary>
    internal const string _defaultDataFilePickerTitle = "Select this or drag and drop a data file here";

    /// <summary>
    /// The callback for when a file is picked.
    /// </summary>
    [Parameter] public EventCallback<ImportDataFileRequest> OnFileRequestChanged { get; set; }
    /// <summary>
    /// Gets or sets whether the file reader is disabled.
    /// </summary>
    [Parameter] public bool Disabled { get; set; }
    /// <summary>
    /// The upload area label text.
    /// </summary>
    /// <remarks>
    /// Defaults to "Select this or drag and drop a data file here".
    /// </remarks>
    [Parameter] public string UploadAreaLabelText { get; set; } = _defaultDataFilePickerTitle;
    /// <summary>
    /// Whether to register this component to the <see cref="DataFileMapperState" /> (if one was provided).
    /// </summary>
    [Parameter] public bool RegisterSelfToState { get; set; } = true;

    private ImportDataFileRequest? _selectedFileRequest;

    private bool _fileHovered;
    private bool _hasHeaderRow = true;

    private bool DisableFilePicker => Disabled || DataFileMapperState?.FileReadState == FileReadState.Reading;

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        if (RegisterSelfToState && DataFileMapperState is not null) { DataFileMapperState.RegisterFilePicker(this); }
    }

    private Task HandleFileSelected(InputFileChangeEventArgs e)
    {
        if (DataFileMapperState is not null) { DataFileMapperState.ShowTransformPreview = false; }

        _selectedFileRequest = new ImportDataFileRequest()
        {
            File = e.File,
            HasHeaderRow = _hasHeaderRow
        };

        return NotifyFilePicked();
    }

    private Task HandleToggleHeaderRow()
    {
        _hasHeaderRow = !_hasHeaderRow;
        var tasks = new List<Task>();

        if (DataFileMapperState is not null)
        {
            tasks.Add(DataFileMapperState.UpdateHeaderRowFlag(_hasHeaderRow));
        }

        if (_selectedFileRequest is not null)
        {
            _selectedFileRequest.HasHeaderRow = _hasHeaderRow;
            tasks.Add(NotifyFilePicked(false));
        }

        return Task.WhenAll(tasks);
    }

    private Task NotifyFilePicked(bool includeInternal = true) => Task.WhenAll(
        OnFileRequestChanged.InvokeAsync(_selectedFileRequest),
        includeInternal ? OnFileRequestChangedInternal.InvokeAsync(_selectedFileRequest) : Task.CompletedTask);
}
