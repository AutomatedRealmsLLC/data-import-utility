using DataImportUtility.Components.Abstractions;
using DataImportUtility.Components.Models;
using DataImportUtility.Components.State;

using Microsoft.AspNetCore.Components;

namespace DataImportUtility.Components.FilePickerComponent;

/// <summary>
/// The base class for the DataFilePicker component.
/// </summary>
public abstract class DataFilePickerComponentBase : FileImportUtilityComponentBase
{
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

    /// <summary>
    /// The main content to render in the component.
    /// </summary>
    public abstract RenderFragment RenderMainContent { get; }
}
