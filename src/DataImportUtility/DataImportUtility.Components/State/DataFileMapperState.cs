using System.Data;
using System.Text.Json.Serialization;

using DataImportUtility.Abstractions;
using DataImportUtility.Components.Abstractions;
using DataImportUtility.Components.DataSetComponents;
using DataImportUtility.Components.FieldMappingComponents.Wrappers;
using DataImportUtility.Components.Models;
using DataImportUtility.Components.Services;
using DataImportUtility.Models;
using DataImportUtility.Models.Validation;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace DataImportUtility.Components.State;

/// <summary>
/// The state for the data file mapper components.
/// </summary>
public class DataFileMapperState : BaseStateEventHandler, IDataFileMapperState, IDisposable
{
    private readonly IDataReaderService _dataReaderService;
    private readonly ILoggerFactory? _loggerFactory;
    private readonly ImportDataFileRequest _fileReadRequest = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DataFileMapperState"/> class.
    /// </summary>
    /// <param name="dataReaderService">The data reader service to use.</param>
    /// <param name="loggerFactory">The logger factory to use.</param>
    /// <remarks>
    /// If the <paramref name="dataReaderService" /> is <see langword="null" />, a new instance of the
    /// component library's <see cref="DataReaderService" /> will be created.
    /// </remarks>
    public DataFileMapperState(IDataReaderService? dataReaderService = null, ILoggerFactory? loggerFactory = null)
    {
        _dataReaderService = dataReaderService ?? new DataReaderService();
        _loggerFactory = loggerFactory;

        OnFileReadError += HandleFileReadErrorOcurred;
    }

    /// <inheritdoc />
    public virtual string MapperStateId { get; } = Guid.NewGuid().ToString()[^5..];

    /// <inheritdoc />
    public virtual ImportedDataFile? DataFile
    {
        get => _dataFile;
        protected set
        {
            if (SetProperty(ref _dataFile, value))
            {
                // Configure validation for UI scenarios when DataFile is set
                if (_dataFile is not null)
                {
                    ConfigureValidationForUI();
                    SubscribeToDataFileValidationEvents();
                }

                OnDataFileChanged?.Invoke();
                ActiveDataTable = value?.DataSet?.Tables.Count > 0
                    ? value.DataSet.Tables[0]
                    : null;
            }
        }
    }
    /// <summary>
    /// The backing field for the <see cref="DataFile" /> property.
    /// </summary>
    protected ImportedDataFile? _dataFile;

    /// <inheritdoc />
    public virtual string? Filename => DataFile?.FileName;

    /// <inheritdoc />
    [JsonIgnore]
    public Exception? FileReadException { get; private set; }

    /// <inheritdoc />
    public virtual DataTable? ActiveDataTable
    {
        get => _activeDataTable;
        set
        {
            if (SetProperty(ref _activeDataTable, value))
            {
                SelectedImportRows.Clear();
                OnActiveDataTableChanged?.Invoke();
            }
        }
    }
    /// <summary>
    /// The backing field for the <see cref="ActiveDataTable" /> property.
    /// </summary>
    protected DataTable? _activeDataTable;

    /// <inheritdoc />
    public virtual FileReadState FileReadState
    {
        get => _fileReadState;
        protected set
        {
            if (SetProperty(ref _fileReadState, value))
            {
                OnFileReadStateChanged?.Invoke();
            }
        }
    }
    /// <summary>
    /// The backing field for the <see cref="FileReadState" /> property.
    /// </summary>
    protected FileReadState _fileReadState;

    /// <inheritdoc />
    public virtual FieldMapperDisplayMode FieldMapperDisplayMode
    {
        get => _fieldMapperDisplayMode;
        set
        {
            if (SetProperty(ref _fieldMapperDisplayMode, value))
            {
                OnFieldMapperDisplayModeChanged?.Invoke();
            }
        }
    }
    /// <summary>
    /// The backing field for the <see cref="FieldMapperDisplayMode" /> property.
    /// </summary>
    protected FieldMapperDisplayMode _fieldMapperDisplayMode;

    /// <inheritdoc />
    public virtual bool ShowTransformPreview
    {
        get => _showTransformPreview;
        set
        {
            if (SetProperty(ref _showTransformPreview, value))
            {
                OnShowTransformPreviewChanged?.Invoke();
            }
        }
    }
    /// <summary>
    /// The backing field for the <see cref="ShowTransformPreview" /> property.
    /// </summary>
    protected bool _showTransformPreview;

    /// <summary>
    /// The generated preview data table from the last preview generation.
    /// This is automatically updated when preview regeneration occurs in the state layer.
    /// </summary>
    public virtual DataTable? PreviewDataTable
    {
        get => _previewDataTable;
        protected set => SetProperty(ref _previewDataTable, value);
    }
    /// <summary>
    /// The backing field for the <see cref="PreviewDataTable" /> property.
    /// </summary>
    protected DataTable? _previewDataTable;

    /// <inheritdoc />
    public List<int> SelectedImportRows { get; } = [];

    /// <summary>
    /// The current target type.
    /// </summary>
    protected Type? _targetType;

    #region Events
    /// <inheritdoc />
    public virtual event Func<Task>? OnActiveDataTableChanged;
    /// <inheritdoc />
    public virtual event Func<Task>? OnDataFileChanged;
    /// <inheritdoc />
    public virtual event Func<Task>? OnFieldMappingsChanged;
    /// <inheritdoc />
    public virtual event Func<Exception, Task>? OnFileReadError;
    /// <inheritdoc />
    public virtual event Func<Task>? OnFileReadStateChanged;
    /// <inheritdoc />
    public virtual event Func<Task>? OnFieldMapperDisplayModeChanged;
    /// <inheritdoc />
    public virtual event Func<Task>? OnShowTransformPreviewChanged;
    /// <inheritdoc />
    public virtual event Func<ValidationStateChangedEventArgs, Task>? OnValidationStateChanged;
    #endregion Events

    #region Validation Integration
    /// <inheritdoc />
    public virtual Dictionary<string, FieldValidationState> GetValidationState(string tableName)
    {
        return DataFile?.GetValidationState(tableName) ?? [];
    }

    /// <inheritdoc />
    public virtual async Task RefreshValidationAsync(string tableName, IEnumerable<string>? fieldNames = null)
    {
        if (DataFile is not null)
        {
            await DataFile.RefreshValidationAsync(tableName, fieldNames);
        }
    }

    /// <inheritdoc />
    public virtual bool HasAnyValidationErrors(string? tableName = null)
    {
        return DataFile?.HasAnyValidationErrors(tableName) ?? false;
    }

    /// <summary>
    /// Configures the validation system for optimal UI scenarios.
    /// Sets reactive mode with UI-optimized settings for responsive validation feedback.
    /// </summary>
    protected virtual void ConfigureValidationForUI()
    {
        if (DataFile is null) return;

        // Configure reactive validation mode with UI-optimized settings
        DataFile.ValidationConfiguration = new ValidationConfiguration
        {
            Mode = ValidationMode.Reactive,
            ReactiveSettings = new ReactiveModeSettings
            {
                DebounceDelay = TimeSpan.FromMilliseconds(300), // Responsive but not excessive
                ValidateOnMappingChange = true, // Essential for real-time field mapping feedback
                ValidateOnDataChange = true, // Important for data file changes
                ValidateOnTemplateChange = true // Important for template loading scenarios
            }
        };

        // Reconfigure the validation system with new settings
        DataFile.ReconfigureValidation();
    }

    /// <summary>
    /// Subscribes to validation events from the DataFile to propagate them to UI components.
    /// </summary>
    protected virtual void SubscribeToDataFileValidationEvents()
    {
        if (DataFile is null) return;

        DataFile.OnValidationStateChanged += HandleDataFileValidationStateChanged;
    }

    /// <summary>
    /// Handles validation state changes from the DataFile and propagates them to UI components.
    /// </summary>
    /// <param name="eventArgs">The validation state change event arguments.</param>
    protected virtual async Task HandleDataFileValidationStateChanged(ValidationStateChangedEventArgs eventArgs)
    {
        // Update the state version to indicate component state has changed
        StateVersion = Guid.NewGuid();

        // Propagate the validation state change to UI components
        if (OnValidationStateChanged is not null)
        {
            await OnValidationStateChanged.Invoke(eventArgs);
        }

        // Also trigger general state change notification
        await NotifyStateChangedAsync();
    }
    #endregion Validation Integration

    #region Public Methods
    /// <inheritdoc />
    public virtual async Task UpdateAndShowTransformPreview()
    {
        if (DataFile is null || string.IsNullOrWhiteSpace(ActiveDataTable?.TableName)) { return; }

        // Generate the preview data and store it in the state
        PreviewDataTable = await DataFile.GenerateOutputDataTable(ActiveDataTable.TableName);
        ShowTransformPreview = true;
    }
    #endregion Public Methods

    #region Component Callback Registrations
    /// <inheritdoc />
    public virtual void RegisterFilePicker(FileImportUtilityComponentBase dataFilePicker)
    {
        dataFilePicker.OnFileRequestChangedInternal = new EventCallbackFactory().Create<ImportDataFileRequest>(this, HandleFilePicked);
    }

    /// <summary>
    /// Registers the data set display component.
    /// </summary>
    /// <param name="importedDataFileDisplay">
    /// The data set display component to register.
    /// </param>
    public virtual void RegisterDataFileMapper<TTargetType>(DataFileMapper<TTargetType> importedDataFileDisplay)
        where TTargetType : class, new()
    {
        importedDataFileDisplay.OnSelectedDataTableChangedInternal = new EventCallbackFactory().Create<DataTable>(this, HandleSelectedDataTableChanged);
        importedDataFileDisplay.OnShowFieldMapperClickedInternal = new EventCallbackFactory().Create<DataTable>(this, HandleShowFieldMapperDialog);
        _targetType = typeof(TTargetType);
    }

    /// <inheritdoc />
    public virtual void RegisterDataSetDisplay(DataSetDisplay dataSetDisplay)
    {
        dataSetDisplay.OnSelectedDataTableChangedInternal = new EventCallbackFactory().Create<DataTable?>(this, HandleSelectedDataTableChanged);
        dataSetDisplay.OnShowFieldMapperClickedInternal = new EventCallbackFactory().Create<DataTable>(this, HandleShowFieldMapperDialog);
    }
    #endregion

    #region File Reader Handlers and other related
    /// <summary>
    /// Handles the file picked event.
    /// </summary>
    /// <param name="request">The file picked request.</param>
    protected virtual Task HandleFilePicked(ImportDataFileRequest request)
    {
        FileReadState = FileReadState.FileSelected;
        _fileReadRequest.File = request.File;
        _fileReadRequest.HasHeaderRow = request.HasHeaderRow;

        return PerformFileReadRequest();
    }

    /// <summary>
    /// Handles the file read error that occurred during the file reading process.
    /// </summary>
    /// <param name="ex">The exception that occurred during file reading.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected Task HandleFileReadErrorOcurred(Exception ex)
    {
        FileReadException = ex;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">
    /// Thrown when the <see cref="DataFile" /> or its <see cref="ImportedDataFile.DataSet"/> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">Thrown when the table does not exist in the data set.</exception>
    public virtual void ReplaceFieldMappings(string tableName, IEnumerable<FieldMapping> incomingFieldMappings)
    {
        if (DataFile is null)
        {
            throw new InvalidOperationException("DataFile is null.");
        }

        DataFile.ReplaceFieldMappings(tableName, incomingFieldMappings);
        StateVersion = Guid.NewGuid();
        OnFieldMappingsChanged?.Invoke();
    }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">
    /// Thrown when the <see cref="DataFile" /> or its <see cref="ImportedDataFile.DataSet"/> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">Thrown when the table does not exist in the data set.</exception>
    public virtual async Task ReplaceFieldMappingsAsync(string tableName, IEnumerable<FieldMapping> incomingFieldMappings)
    {
        if (DataFile is null)
        {
            throw new InvalidOperationException("DataFile is null.");
        }

        await DataFile.ReplaceFieldMappingsAsync(tableName, incomingFieldMappings);
        StateVersion = Guid.NewGuid();

        // If preview is showing, automatically regenerate it with new field mappings
        if (ShowTransformPreview && ActiveDataTable is not null)
        {
            PreviewDataTable = await DataFile.GenerateOutputDataTable(ActiveDataTable.TableName);
        }

        await (OnFieldMappingsChanged?.Invoke() ?? Task.CompletedTask);
    }

    /// <inheritdoc />
    public virtual Task UpdateHeaderRowFlag(bool hasHeaderRow)
    {
        _fileReadRequest.HasHeaderRow = hasHeaderRow;

        return PerformFileReadRequest();
    }

    /// <summary>
    /// Performs the file read request.
    /// </summary>
    protected virtual async Task PerformFileReadRequest()
    {
        if (_fileReadRequest.File is null) { return; }
        DataFile = null;

        FileReadState = FileReadState.Reading;

        try
        {
            DataFile = await _dataReaderService.ReadImportFile(_fileReadRequest);
            // TODO: Use the IgnoreFields and RequiredFields properties here

            // Refresh the ImportedRecordFieldDescriptors
            DataFile.RefreshFieldDescriptors(false);
            await DataFile.RefreshFieldMappingsAsync(false);

            // Update target type auto-mappings
            if (_targetType is not null)
            {
                await DataFile.SetTargetTypeAsync(_targetType, autoMatchFields: true);
            }

            FileReadState = FileReadState.Success;
        }
        catch (Exception ex)
        {
            FileReadState = FileReadState.Error;
            DataFile = null;
            StateVersion = Guid.NewGuid();
            OnFileReadError?.Invoke(ex);

            if (_loggerFactory is null) { return; }

            var logger = _loggerFactory.CreateLogger<DataFileMapperState>();
            logger.LogError(ex, "An error occurred while reading the file.");
        }
    }
    #endregion File Reader Handlers and other related

    #region Data Display Handlers and other related
    /// <summary>
    /// Handles the selected data table changed event.
    /// </summary>
    /// <param name="table">The selected data table.</param>
    protected virtual void HandleSelectedDataTableChanged(DataTable? table)
    {
        ActiveDataTable = table;
    }

    /// <summary>
    /// Handles the show field mapper dialog event.
    /// </summary>
    /// <param name="table">The table to show the field mapper dialog for.</param>
    protected virtual void HandleShowFieldMapperDialog(DataTable table)
    {
        ActiveDataTable = table;
        // TODO: Allow setting the FieldMapperDisplayMode in this method
        FieldMapperDisplayMode = FieldMapperDisplayMode.Flyout;
    }
    #endregion Data Display Handlers and other related

    /// <inheritdoc />
    public virtual void Dispose()
    {
        // Unsubscribe from DataFile validation events
        if (DataFile is not null)
        {
            DataFile.OnValidationStateChanged -= HandleDataFileValidationStateChanged;
        }

        OnFileReadError -= HandleFileReadErrorOcurred;

        OnDataFileChanged = null;
        OnActiveDataTableChanged = null;
        OnFileReadError = null;
        OnFileReadStateChanged = null;
        OnFieldMapperDisplayModeChanged = null;
        OnFieldMappingsChanged = null;
        OnShowTransformPreviewChanged = null;
        OnValidationStateChanged = null;

        GC.SuppressFinalize(this);
    }
}
