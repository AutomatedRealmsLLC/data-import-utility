using System;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AutomatedRealms.DataImportUtility.Abstractions; // For IProgressReporter
using AutomatedRealms.DataImportUtility.Core.Models; // For ImportedDataFile, FieldMapping, ImportTableDefinition
using AutomatedRealms.DataImportUtility.DataReader.Abstractions; // For IDataReaderService
using AutomatedRealms.DataImportUtility.Components.Abstractions;
using AutomatedRealms.DataImportUtility.Components.Models;
using AutomatedRealms.DataImportUtility.Components.FieldMappingComponents.Wrappers;
using Microsoft.Extensions.Logging;

namespace AutomatedRealms.DataImportUtility.Components.State;

/// <summary>
/// Manages the state for the data file mapping component.
/// Implements <see cref="IDataFileMapperState"/> and handles property change notifications
/// through <see cref="BaseStateEventHandler"/>.
/// </summary>
public class DataFileMapperState : BaseStateEventHandler, IDataFileMapperState, IDisposable
{
    private readonly ILogger<DataFileMapperState> _logger;
    private readonly IDataReaderService _dataReaderService;
    private Type? _targetType;
    private bool _disposedValue;

    /// <summary>
    /// Backing field for <see cref="DataFile"/> property.
    /// </summary>
    protected ImportedDataFile? _dataFileField;
    private DataTable? _activeDataTableField;
    private FileReadState _fileReadStateField = Models.FileReadState.NoFile;
    private string? _errorMessageField;
    private bool _showTransformPreviewField;
    private FieldMapperDisplayMode _fieldMapperDisplayModeField = FieldMappingComponents.Wrappers.FieldMapperDisplayMode.Hide;
    private ImmutableList<DataRow>? _selectedImportRowsField = ImmutableList<DataRow>.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataFileMapperState"/> class.
    /// </summary>
    /// <param name="dataReaderService">The service for reading data files. Must not be null.</param>
    /// <param name="loggerFactory">The logger factory. Must not be null.</param>
    /// <exception cref="ArgumentNullException">If dataReaderService or loggerFactory is null.</exception>
    public DataFileMapperState(IDataReaderService dataReaderService, ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory?.CreateLogger<DataFileMapperState>() ?? throw new ArgumentNullException(nameof(loggerFactory));
        _dataReaderService = dataReaderService ?? throw new ArgumentNullException(nameof(dataReaderService));
    }

    /// <inheritdoc />
    public virtual ImportedDataFile? DataFile
    {
        get => _dataFileField;
        protected set
        {
            if (SetField(ref _dataFileField, value))
            {
                var task = OnDataFileChanged?.Invoke();
                if (task is not null && !task.IsCompletedSuccessfully)
                {
                    HandleAsyncTaskError(task, nameof(OnDataFileChanged));
                }
                NotifyPropertyChanged(nameof(ActiveTableDefinition));
            }
        }
    }

    /// <inheritdoc />
    public DataTable? ActiveDataTable
    {
        get => _activeDataTableField;
        set
        {
            if (SetField(ref _activeDataTableField, value))
            {
                var task = OnActiveDataTableChanged?.Invoke();
                if (task is not null && !task.IsCompletedSuccessfully)
                {
                    HandleAsyncTaskError(task, nameof(OnActiveDataTableChanged));
                }
                NotifyPropertyChanged(nameof(ActiveTableDefinition));
            }
        }
    }

    /// <inheritdoc />
    public FileReadState FileReadState
    {
        get => _fileReadStateField;
        protected set => SetField(ref _fileReadStateField, value, nameof(FileReadState), async () => await InvokeAsync(OnFileReadStateChanged?.Invoke(_fileReadStateField)));
    }

    /// <inheritdoc />
    public string? ErrorMessage
    {
        get => _errorMessageField;
        protected set => SetField(ref _errorMessageField, value, nameof(ErrorMessage), async () => await InvokeAsync(OnFileReadError?.Invoke(string.IsNullOrEmpty(_errorMessageField) ? null : new Exception(_errorMessageField))));
    }

    /// <inheritdoc />
    public bool ShowTransformPreview
    {
        get => _showTransformPreviewField;
        set => SetField(ref _showTransformPreviewField, value, nameof(ShowTransformPreview), async () => await InvokeAsync(OnShowTransformPreviewChanged?.Invoke()));
    }

    /// <inheritdoc />
    public FieldMapperDisplayMode FieldMapperDisplayMode
    {
        get => _fieldMapperDisplayModeField;
        set => SetField(ref _fieldMapperDisplayModeField, value, nameof(FieldMapperDisplayMode), async () => await InvokeAsync(OnFieldMapperDisplayModeChanged?.Invoke()));
    }

    /// <inheritdoc />
    public ImmutableList<DataRow>? SelectedImportRows
    {
        get => _selectedImportRowsField;
        set => SetField(ref _selectedImportRowsField, value, nameof(SelectedImportRows));
    }

    /// <inheritdoc />
    public ImportTableDefinition? ActiveTableDefinition => DataFile?.TableDefinitions?.FirstOrDefault(td => td.TableName == ActiveDataTable?.TableName);

    /// <inheritdoc />
    public event Func<Task>? OnActiveDataTableChanged;
    /// <inheritdoc />
    public event Func<Task>? OnDataFileChanged;
    /// <inheritdoc />
    public event Func<FileReadState, Task>? OnFileReadStateChanged;
    /// <inheritdoc />
    public event Func<Exception?, Task>? OnFileReadError;
    /// <inheritdoc />
    public event Func<Task>? OnFieldMapperDisplayModeChanged;
    /// <inheritdoc />
    public event Func<Task>? OnFieldMappingsChanged;
    /// <inheritdoc />
    public event Func<Task>? OnShowTransformPreviewChanged;

    /// <inheritdoc />
    public virtual async Task SetFileAsync(ImportDataFileRequest request, IProgressReporter? progressReporter = null)
    {
        if (request?.File is null)
        {
            DataFile = null;
            ActiveDataTable = null;
            FileReadState = Models.FileReadState.NoFile;
            ErrorMessage = "No file was selected.";
            return;
        }

        FileReadState = Models.FileReadState.Reading;
        ErrorMessage = null;

        try
        {
            var importedFile = await _dataReaderService.ReadFileAsync(request, progressReporter);
            DataFile = importedFile;

            if (_targetType is not null && DataFile is not null)
            {
                DataFile.SetTargetType(_targetType, autoMatchFields: true);
            }

            ActiveDataTable = DataFile?.DataSet?.Tables?.Count > 0 ? DataFile.DataSet.Tables[0] : null;
            FileReadState = Models.FileReadState.Success; // Corrected
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading file {FileName}", request.Name);
            DataFile = null;
            ActiveDataTable = null;
            ErrorMessage = ex.Message;
            FileReadState = Models.FileReadState.Error; // Corrected
        }
    }

    /// <inheritdoc />
    public virtual void SetTargetType<T>(bool autoMatchFields = true) where T : class, new()
    {
        _targetType = typeof(T);
        if (DataFile is not null)
        {
            DataFile.SetTargetType(_targetType, autoMatchFields);
            var task = OnFieldMappingsChanged?.Invoke();
            if (task is not null && !task.IsCompletedSuccessfully)
            {
                HandleAsyncTaskError(task, nameof(OnFieldMappingsChanged));
            }
            NotifyPropertyChanged(nameof(ActiveTableDefinition));
        }
    }

    /// <inheritdoc />
    public virtual void ReplaceFieldMappings(string tableName, IEnumerable<FieldMapping> incomingFieldMappings)
    {
        var tableDef = DataFile?.TableDefinitions?.FirstOrDefault(td => td.TableName == tableName);
        if (tableDef is not null)
        {
            tableDef.ReplaceFieldMappings(incomingFieldMappings);
            var task = OnFieldMappingsChanged?.Invoke();
            if (task is not null && !task.IsCompletedSuccessfully)
            {
                HandleAsyncTaskError(task, nameof(OnFieldMappingsChanged));
            }
        }
    }

    /// <inheritdoc />
    public virtual void RefreshFieldMappings(bool overwriteExisting = false, bool autoMatch = false)
    {
        if (DataFile is not null)
        {
            DataFile.RefreshFieldMappings(overwriteExisting, autoMatch);
            var task = OnFieldMappingsChanged?.Invoke();
            if (task is not null && !task.IsCompletedSuccessfully)
            {
                HandleAsyncTaskError(task, nameof(OnFieldMappingsChanged));
            }
            NotifyPropertyChanged(nameof(ActiveTableDefinition));
        }
    }

    private async Task InvokeAsync(Task? taskToAwait)
    {
        if (taskToAwait is not null)
        {
            try
            {
                await taskToAwait;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invoking event handler.");
            }
        }
    }

    private void HandleAsyncTaskError(Task task, string eventName)
    {
        task.ContinueWith(t =>
        {
            if (t.IsFaulted && t.Exception is not null)
            {
                _logger.LogError(t.Exception, "Error invoking event {EventName}", eventName);
            }
        }, TaskScheduler.Default);
    }

    /// <summary>
    /// Protected implementation of Dispose pattern.
    /// </summary>
    /// <param name="disposing">True if disposing managed resources, false otherwise.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                // Dispose managed state (managed objects).
            }
            _disposedValue = true;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
