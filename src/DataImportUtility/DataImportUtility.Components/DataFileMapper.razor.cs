using System.ComponentModel.DataAnnotations;
using System.Data;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

using DataImportUtility.Components.Abstractions;
using DataImportUtility.Components.FieldMappingComponents.Wrappers;
using DataImportUtility.Components.State;
using DataImportUtility.Models;

namespace DataImportUtility.Components;

/// <summary>
/// This class is used to map data fields from a source to a target type.
/// </summary>
/// <typeparam name="TTargetType">The type of the target object to map to.</typeparam>
/// <remarks>
/// You can provide your own <see cref="IDataFileMapperState"/> using a <see cref="CascadingValue{IDataFileMapperState}"/>. This allows you to control the state of the data file mapper more directly. If you do not provide your own, a new <see cref="DataFileMapperState" /> will be created and managed internally.
/// </remarks>
public partial class DataFileMapper<TTargetType> : FileImportUtilityComponentBase, IDisposable
    where TTargetType : class, new()
{
    [Inject, AllowNull] private ILoggerFactory LoggerFactory { get; set; }

    private const string _noPreviewMessage = "No preview available.";

    /// <summary>
    /// Whether to register this component to the <see cref="DataFileMapperState" />.
    /// </summary>
    [Parameter] public bool RegisterSelfToState { get; set; } = true;
    /// <summary>
    /// The callback for when the selected data table changes.
    /// </summary>
    [Parameter] public EventCallback<DataTable> OnSelectedDataTableChanged { get; set; }
    /// <summary>
    /// The callback for when the show field mapper button is clicked.
    /// </summary>
    [Parameter] public EventCallback<DataTable> OnShowFieldMapperClicked { get; set; }
    /// <summary>
    /// The fields that are required to be mapped.
    /// </summary>
    [Parameter] public IEnumerable<string>? RequiredFields { get; set; }
    /// <summary>
    /// Fields that should not be displayed for mapping.
    /// </summary>
    [Parameter] public IEnumerable<string>? IgnoreFields { get; set; }

    #region UI Parameters
    /// <summary>
    /// Whether or not to wrap the component in a div.
    /// </summary>
    [Parameter] public bool UseWrappingDiv { get; set; } = true;
    #endregion

    /// <summary>
    /// The callback for when the selected data table changes.
    /// </summary>
    /// <remarks>
    /// This is used internally with the <see cref="DataFileMapperState"/>. It should not be used directly.
    /// </remarks>
    internal EventCallback<DataTable> OnSelectedDataTableChangedInternal { get; set; }
    /// <summary>
    /// The callback for when the show field mapper button is clicked.
    /// </summary>
    /// <remarks>
    /// This is used internally with the <see cref="DataFileMapperState"/>. It should not be used directly.
    /// </remarks>
    internal EventCallback<DataTable> OnShowFieldMapperClickedInternal { get; set; }

    [AllowNull]
    private IDataFileMapperState _myDataFileMapperState; // Just so we don't have to null check every time.

    private ImportedDataFile? LoadedDataFile => _myDataFileMapperState?.DataFile;
    private DataTable? _previewOutput;
    private bool _noPreviewAvailable;
    private string? _errorMessage;

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        DataFileMapperState ??= new DataFileMapperState(loggerFactory: LoggerFactory);
        _myDataFileMapperState = DataFileMapperState;
        if (RegisterSelfToState) { _myDataFileMapperState.RegisterDataFileMapper(this); }

        _myDataFileMapperState.OnActiveDataTableChanged += HandleStateChanged;
        _myDataFileMapperState.OnDataFileChanged += HandleDataFileChanged; ;
        _myDataFileMapperState.OnFileReadStateChanged += HandleStateChanged;
        _myDataFileMapperState.OnFileReadError += HandleFileReadError;
        _myDataFileMapperState.OnFieldMapperDisplayModeChanged += HandleStateChanged;
        _myDataFileMapperState.OnFieldMappingsChanged += HandleStateChanged;
        _myDataFileMapperState.OnShowTransformPreviewChanged += HandleShowTransformPreviewChanged;
    }

    private Task HandleShowTransformPreviewChanged()
        => _myDataFileMapperState.ShowTransformPreview && _myDataFileMapperState.ActiveDataTable is not null
            ? UpdatePreview(_myDataFileMapperState.ActiveDataTable)
            : HandleStateChanged();

    private Task HandleDataFileChanged()
    {
        _myDataFileMapperState.DataFile?.SetTargetType<TTargetType>();

        return HandleStateChanged();
    }

    private Task HandleStateChanged()
        => InvokeAsync(StateHasChanged);

    private Task HandleFileReadError(Exception ex)
    {
        _errorMessage = "There was an error reading the file.";
        _errorMessage += $"<br />{ex.Message}<br /><pre>{ex.StackTrace}</pre>";
        return Task.CompletedTask;
    }

    private void HandleFilePicked()
    {
        _errorMessage = null;
    }

    private Task HandleShowFieldMapperDialog(DataTable dataTable)
        => Task.WhenAll(
            OnShowFieldMapperClickedInternal.InvokeAsync(dataTable),
            OnShowFieldMapperClicked.InvokeAsync(dataTable));

    private async Task HandleTogglePreviewOutput(DataTable dataTable)
    {
        _noPreviewAvailable = false;
        if (_myDataFileMapperState.ShowTransformPreview)
        {
            _myDataFileMapperState.ShowTransformPreview = false;
            return;
        }

        await UpdatePreview(dataTable);
        _myDataFileMapperState.ShowTransformPreview = !_noPreviewAvailable;
    }

    private async Task UpdatePreview(DataTable dataTable)
    {
        if (!(LoadedDataFile?.TableDefinitions?.ContainsTable(dataTable.TableName) ?? false))
        {
            return;
        }

        _previewOutput = await LoadedDataFile.GenerateOutputDataTable(dataTable.TableName);
        _noPreviewAvailable = _previewOutput is null;
        await InvokeAsync(StateHasChanged);
    }

    private Task HandleSelectedDataTableChanged(DataTable? dataTable)
    {
        _myDataFileMapperState.ActiveDataTable = dataTable;
        return Task.WhenAll(
            OnSelectedDataTableChangedInternal.InvokeAsync(dataTable),
            OnSelectedDataTableChanged.InvokeAsync(dataTable));
    }

    private void HandleShowFieldMapperChanged(bool newShow)
    {
        if (newShow) { return; }
        _myDataFileMapperState.FieldMapperDisplayMode = FieldMapperDisplayMode.Hide;
    }

    private void HandleFieldMappingsChangesCommitted(IEnumerable<FieldMapping> incomingFieldMappings)
    {
        if (_myDataFileMapperState.ActiveDataTable is null)
        {
            Console.Error.WriteLine("No active data table was selected in the state.");
            return;
        }

        _myDataFileMapperState.ReplaceFieldMappings(_myDataFileMapperState.ActiveDataTable.TableName, incomingFieldMappings);
        _myDataFileMapperState.FieldMapperDisplayMode = FieldMapperDisplayMode.Hide;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _myDataFileMapperState.OnActiveDataTableChanged -= HandleStateChanged;
        _myDataFileMapperState.OnDataFileChanged -= HandleDataFileChanged; ;
        _myDataFileMapperState.OnFileReadStateChanged -= HandleStateChanged;
        _myDataFileMapperState.OnFileReadError -= HandleFileReadError;
        _myDataFileMapperState.OnFieldMapperDisplayModeChanged -= HandleStateChanged;
        _myDataFileMapperState.OnFieldMappingsChanged -= HandleStateChanged;
        _myDataFileMapperState.OnShowTransformPreviewChanged -= HandleShowTransformPreviewChanged;
    }
}
