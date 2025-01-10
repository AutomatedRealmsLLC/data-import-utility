using System.ComponentModel;
using System.Data;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

using DataImportUtility.Abstractions;
using DataImportUtility.Components.Abstractions;
using DataImportUtility.Components.DataSetComponents;
using DataImportUtility.Components.FieldMappingComponents.Wrappers;
using DataImportUtility.Components.FilePickerComponent;
using DataImportUtility.Components.Models;
using DataImportUtility.Components.Services;
using DataImportUtility.Models;

namespace DataImportUtility.Components.State;

/// <summary>
/// The state for the data file mapper components.
/// </summary>
/// <param name="dataReaderService">
/// The data reader service to use.
/// </param>
/// <param name="loggerFactory">
/// The logger factory to use.
/// </param>
/// <remarks>
/// If the <paramref name="dataReaderService" /> is <see langword="null" />, a new instance of the
/// component library's <see cref="DataReaderService" /> will be created.
/// </remarks>
public class DataFileMapperState(IDataReaderService? dataReaderService = null, ILoggerFactory? loggerFactory = null) : BaseStateEventHandler, IDataFileMapperState, IDisposable
{
    private readonly IDataReaderService _dataReaderService = dataReaderService ?? new DataReaderService();
    private readonly ImportDataFileRequest _fileReadRequest = new();

    /// <inheritdoc />
    public string MapperStateId { get; } = Guid.NewGuid().ToString()[^5..];

    /// <inheritdoc />
    public ImportedDataFile? DataFile
    {
        get => _dataFile;
        private set
        {
            SetProperty(ref _dataFile, value);
            ActiveDataTable = value?.DataSet?.Tables.Count > 0
                ? value.DataSet.Tables[0]
                : null;
            OnDataFileChanged?.Invoke();
        }
    }
    private ImportedDataFile? _dataFile;

    /// <inheritdoc />
    public DataTable? ActiveDataTable
    {
        get => _activeDataTable;
        set
        {
            SetProperty(ref _activeDataTable, value);
            OnActiveDataTableChanged?.Invoke();
        }
    }
    private DataTable? _activeDataTable;

    /// <inheritdoc />
    public FileReadState FileReadState
    {
        get => _fileReadState;
        private set
        {
            SetProperty(ref _fileReadState, value);
            OnFileReadStateChanged?.Invoke();
        }
    }
    private FileReadState _fileReadState;

    /// <inheritdoc />
    public FieldMapperDisplayMode FieldMapperDisplayMode
    {
        get => _fieldMapperDisplayMode;
        set
        {
            SetProperty(ref _fieldMapperDisplayMode, value);
            OnFieldMapperDisplayModeChanged?.Invoke();
        }
    }
    private FieldMapperDisplayMode _fieldMapperDisplayMode;

    /// <inheritdoc />
    public bool ShowTransformPreview
    {
        get => _showTransformPreview;
        set
        {
            if (_showTransformPreview == value) { return; }
            SetProperty(ref _showTransformPreview, value);
            OnShowTransformPreviewChanged?.Invoke();
        }
    }
    private bool _showTransformPreview;

    private Type? _targetType;

    #region Events
    /// <inheritdoc />
    public event Func<Task>? OnActiveDataTableChanged;
    /// <inheritdoc />
    public event Func<Task>? OnDataFileChanged;
    /// <inheritdoc />
    public event Func<Task>? OnFieldMappingsChanged;
    /// <inheritdoc />
    public event Func<Exception, Task>? OnFileReadError;
    /// <inheritdoc />
    public event Func<Task>? OnFileReadStateChanged;
    /// <inheritdoc />
    public event Func<Task>? OnFieldMapperDisplayModeChanged;
    /// <inheritdoc />
    public event Func<Task>? OnShowTransformPreviewChanged;
    #endregion Events

    #region Public Methods
    /// <inheritdoc />
    public async Task UpdateAndShowTransformPreview()
    {
        if (DataFile is null || string.IsNullOrWhiteSpace(ActiveDataTable?.TableName)) { return; }
        await DataFile.GenerateOutputDataTable(ActiveDataTable.TableName);
        ShowTransformPreview = true;
    }
    #endregion Public Methods

    #region Component Callback Registrations
    /// <inheritdoc />
    public void RegisterFilePicker(DataFilePicker dataFilePicker)
    {
        dataFilePicker.OnFileRequestChangedInternal = new EventCallbackFactory().Create<ImportDataFileRequest>(this, HandleFilePicked);
    }

    /// <summary>
    /// Registers the data set display component.
    /// </summary>
    /// <param name="importedDataFileDisplay">
    /// The data set display component to register.
    /// </param>
    public void RegisterDataFileMapper<TTargetType>(DataFileMapper<TTargetType> importedDataFileDisplay)
        where TTargetType : class, new()
    {
        importedDataFileDisplay.OnSelectedDataTableChangedInternal = new EventCallbackFactory().Create<DataTable>(this, HandleSelectedDataTableChanged);
        importedDataFileDisplay.OnShowFieldMapperClickedInternal = new EventCallbackFactory().Create<DataTable>(this, HandleShowFieldMapperDialog);
        _targetType = typeof(TTargetType);
    }

    /// <inheritdoc />
    public void RegisterDataSetDisplay(DataSetDisplay dataSetDisplay)
    {
        dataSetDisplay.OnSelectedDataTableChangedInternal = new EventCallbackFactory().Create<DataTable?>(this, HandleSelectedDataTableChanged);
        dataSetDisplay.OnShowFieldMapperClickedInternal = new EventCallbackFactory().Create<DataTable>(this, HandleShowFieldMapperDialog);
    }
    #endregion

    #region File Reader Handlers and other related
    private Task HandleFilePicked(ImportDataFileRequest request)
    {
        FileReadState = FileReadState.FileSelected;
        _fileReadRequest.File = request.File;
        _fileReadRequest.HasHeaderRow = request.HasHeaderRow;

        return PerformFileReadRequest();
    }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">
    /// Thrown when the <see cref="DataFile" /> or its <see cref="ImportedDataFile.DataSet"/> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">Thrown when the table does not exist in the data set.</exception>
    public void ReplaceFieldMappings(string tableName, IEnumerable<FieldMapping> incomingFieldMappings)
    {
        if (DataFile is null)
        {
            throw new InvalidOperationException("DataFile is null.");
        }

        DataFile.ReplaceFieldMappings(tableName, incomingFieldMappings);
        OnFieldMappingsChanged?.Invoke();
    }

    /// <inheritdoc />
    public Task UpdateHeaderRowFlag(bool hasHeaderRow)
    {
        _fileReadRequest.HasHeaderRow = hasHeaderRow;

        return PerformFileReadRequest();
    }

    private async Task PerformFileReadRequest()
    {
        if (_fileReadRequest.File is null) { return; }
        DataFile = null;

        FileReadState = FileReadState.Reading;

        try
        {
            DataFile = await _dataReaderService.ReadImportFile(_fileReadRequest);
            // TODO: Use the IgnoreFields and RequiredFields properties here
            if (_targetType is not null)
            {
                DataFile.SetTargetType(_targetType, autoMatchFields: true);
            }
            FileReadState = FileReadState.Success;
        }
        catch (Exception ex)
        {
            FileReadState = FileReadState.Error;
            OnFileReadError?.Invoke(ex);
            DataFile = null;

            if (loggerFactory is null) { return; }

            var logger = loggerFactory.CreateLogger<DataFileMapperState>();
            logger.LogError(ex, "An error occurred while reading the file.");
        }
    }
    #endregion File Reader Handlers and other related

    #region Data Display Handlers and other related
    private void HandleSelectedDataTableChanged(DataTable? table)
    {
        ActiveDataTable = table;
    }

    private void HandleShowFieldMapperDialog(DataTable table)
    {
        ActiveDataTable = table;
        // TODO: Allow setting the FieldMapperDisplayMode in this method
        FieldMapperDisplayMode = FieldMapperDisplayMode.Flyout;
    }
    #endregion Data Display Handlers and other related

    /// <inheritdoc />
    public virtual void Dispose()
    {
        OnDataFileChanged = null;
        OnActiveDataTableChanged = null;
        OnFileReadError = null;
        OnFileReadStateChanged = null;
        OnFieldMapperDisplayModeChanged = null;
        OnFieldMappingsChanged = null;
        OnShowTransformPreviewChanged = null;
    }
}
