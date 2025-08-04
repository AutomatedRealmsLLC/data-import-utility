using DataImportUtility.Components.Models;

using Microsoft.AspNetCore.Components.Forms;

namespace DataImportUtility.Components.FilePickerComponent;

/// <summary>
/// The data file reader component.
/// </summary>
public partial class DataFilePicker : DataFilePickerComponentBase
{
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
