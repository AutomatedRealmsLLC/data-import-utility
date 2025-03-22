using System.Data;
using System.Timers;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

using DataImportUtility.Components.Abstractions;
using DataImportUtility.Components.DataSetComponents;
using DataImportUtility.Components.Extensions;
using DataImportUtility.Components.FieldMappingComponents.Wrappers;
using DataImportUtility.Components.JsInterop;
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
    [Inject, AllowNull] private IJSRuntime JsRuntime { get; set; }
    [Inject, AllowNull] private ILoggerFactory LoggerFactory { get; set; }
    [Inject, AllowNull] private IServiceProvider ServiceProvider { get; set; }

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

    private DataTableDisplay? _previewOutputTableRef;
    private DataTableDisplay? _importedDataTableRef;

    private FileMapperJsModule? _fileMapperJsModule;
    private FileMapperJsModule FileMapperJsModule => _fileMapperJsModule ??= new FileMapperJsModule(JsRuntime);

    private System.Timers.Timer _setJsHandlersTimer = new()
    {
        Interval = 200,
        AutoReset = false
    };

    private int? _hoveredRowIndex;

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        DataFileMapperState ??= new DataFileMapperState(loggerFactory: LoggerFactory);
        _myDataFileMapperState = DataFileMapperState;
        if (RegisterSelfToState) { _myDataFileMapperState.RegisterDataFileMapper(this); }

        _myDataFileMapperState.OnActiveDataTableChanged += HandleStateChanged;
        _myDataFileMapperState.OnDataFileChanged += HandleDataFileChanged;
        _myDataFileMapperState.OnFileReadStateChanged += HandleStateChanged;
        _myDataFileMapperState.OnFileReadError += HandleFileReadError;
        _myDataFileMapperState.OnFieldMapperDisplayModeChanged += HandleStateChanged;
        _myDataFileMapperState.OnFieldMappingsChanged += HandleStateChanged;
        _myDataFileMapperState.OnShowTransformPreviewChanged += HandleShowTransformPreviewChanged;
        _myDataFileMapperState.OnStatePropertyChanged += HandleStatePropertyChanged;
        _setJsHandlersTimer.Elapsed += HandleSetJsHandlers;
    }

    private Task HandleStatePropertyChanged(string propName)
    {
        if (propName == nameof(IDataFileMapperState.SelectedImportRows))
        {
            return InvokeAsync(StateHasChanged);
        }
        return Task.CompletedTask;
    }

    private Task HandleShowTransformPreviewChanged()
    {
        _setJsHandlersTimer.Stop();
        _setJsHandlersTimer.Start();
        return _myDataFileMapperState.ShowTransformPreview && _myDataFileMapperState.ActiveDataTable is not null
            ? UpdatePreview(_myDataFileMapperState.ActiveDataTable)
            : HandleStateChanged();
    }

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

        if (_importedDataTableRef is not null)
        {
            await FileMapperJsModule.RemoveScrollSynchronization(_importedDataTableRef.Id);
            await FileMapperJsModule.RemoveScrollMouseEventsSynchronization(_importedDataTableRef.Id);
        }
        _previewOutput = await LoadedDataFile.GenerateOutputDataTable(dataTable.TableName);

        _noPreviewAvailable = _previewOutput is null;
        await InvokeAsync(StateHasChanged);
    }

    private Task HandleSelectedDataTableChanged(DataTableDisplay? dataTableRef)
    {
        _importedDataTableRef = dataTableRef;
        _myDataFileMapperState.ActiveDataTable = dataTableRef?.DataTable;

        return Task.WhenAll(
            _previewOutputTableRef is not null && dataTableRef is not null
                ? FileMapperJsModule.SynchronizeElementScrolling(dataTableRef.Id, _previewOutputTableRef.Id).AsTask()
                : Task.CompletedTask,
            _previewOutputTableRef is not null && dataTableRef is not null
                ? FileMapperJsModule.SynchronizeTableRowHover(_previewOutputTableRef.Id, dataTableRef.Id).AsTask()
                : Task.CompletedTask,
            OnSelectedDataTableChangedInternal.InvokeAsync(dataTableRef?.DataTable),
            OnSelectedDataTableChanged.InvokeAsync(dataTableRef?.DataTable));
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

    private async void HandleSetJsHandlers(object? sender, ElapsedEventArgs e)
    {
        _setJsHandlersTimer.Stop();

        if (!_myDataFileMapperState.ShowTransformPreview || _importedDataTableRef is null)
        {
            await FileMapperJsModule.RemoveTableRowHoverSynchronization(".preview-data-table-wrapper");
            await FileMapperJsModule.RemoveTableRowHoverSynchronization(".data-table-display-wrapper");
            return;
        }

        if (_previewOutputTableRef is not { TableIsRendered: true })
        {
            _setJsHandlersTimer.Start();
            return;
        }

        await FileMapperJsModule.SynchronizeElementScrolling(".data-table-display-wrapper", ".preview-data-table-wrapper");
        await FileMapperJsModule.SynchronizeElementScrolling(".preview-data-table-wrapper", ".data-table-display-wrapper");
        await FileMapperJsModule.SynchronizeTableRowHover(".data-table-display-wrapper", ".preview-data-table-wrapper");
        await FileMapperJsModule.SynchronizeTableRowHover(".preview-data-table-wrapper", ".data-table-display-wrapper");
    }

    private Task HandleHoveredRowChanged(int? rowIndex)
    {
        if (_hoveredRowIndex == rowIndex) { return Task.CompletedTask; }
        _hoveredRowIndex = rowIndex;
        return Task.CompletedTask;
    }

    private void HandleToggleAllSelected()
    {
        if (_myDataFileMapperState.ActiveDataTable is null)
        {
            _myDataFileMapperState.SelectedImportRows.Clear();
            return;
        }

        var currentRowCount = _myDataFileMapperState.ActiveDataTable.Rows.Count;
        if (_myDataFileMapperState.SelectedImportRows.Count == currentRowCount)
        {
            _myDataFileMapperState.SelectedImportRows.Clear();
        }
        else
        {
            _myDataFileMapperState.SelectedImportRows.AddRange(Enumerable.Range(0, currentRowCount).Except(_myDataFileMapperState.SelectedImportRows));
        }

        return;
    }

    private Task HandleSelectedRowsChanged() => InvokeAsync(StateHasChanged);

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

        _setJsHandlersTimer.Stop();
        _setJsHandlersTimer.Dispose();
    }
}
