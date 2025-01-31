using System.ComponentModel;
using System.Data;

using DataImportUtility.Components.DataSetComponents;
using DataImportUtility.Components.FieldMappingComponents.Wrappers;
using DataImportUtility.Components.FilePickerComponent;
using DataImportUtility.Components.State;
using DataImportUtility.Models;

namespace DataImportUtility.Components.Abstractions;

/// <summary>
/// The state for the data file mapper components.
/// </summary>
public interface IDataFileMapperState : INotifyPropertyChanged, INotifyPropertyChanging
{
    #region Events
    /// <summary>
    /// Event raised when the state changes.
    /// </summary>
    event Func<Task>? OnNotifyStateChanged;
    /// <summary>
    /// Event raised when a property on the state class has changed.
    /// </summary>
    event Func<string, Task>? OnStatePropertyChanged;
    #endregion Events

    /// <summary>
    /// The active data table.
    /// </summary>
    /// <remarks>
    /// Changing the value of this will raise the <see cref="OnActiveDataTableChanged" /> event.
    /// </remarks>
    DataTable? ActiveDataTable { get; set; }
    /// <summary>
    /// The data file to map.
    /// </summary>
    ImportedDataFile? DataFile { get; }
    /// <summary>
    /// The state of the file read process.
    /// </summary>
    FileReadState FileReadState { get; }
    /// <summary>
    /// The flag to show the field mapper dialog.
    /// </summary>
    FieldMapperDisplayMode FieldMapperDisplayMode { get; set; }
    /// <summary>
    /// The flag to show the transform preview.
    /// </summary>
    bool ShowTransformPreview { get; set; }
    /// <summary>
    /// The unique identifier for the mapper state.
    /// </summary>
    /// <remarks>
    /// This is used to group together components that use the same <see cref="IDataFileMapperState" />
    /// </remarks>
    string MapperStateId { get; }
    /// <summary>
    /// The selected rows in the Import table.
    /// </summary>
    List<int> SelectedImportRows { get; }

    /// <summary>
    /// The callback for when the active data table is changed.
    /// </summary>
    event Func<Task>? OnActiveDataTableChanged;
    /// <summary>
    /// The callback for when the data file is changed.
    /// </summary>
    event Func<Task>? OnDataFileChanged;
    /// <summary>
    /// The callback for when the field mappings change.
    /// </summary>
    event Func<Task>? OnFieldMappingsChanged;
    /// <summary>
    /// The callback for when an error occurrs while a file is read.
    /// </summary>
    event Func<Exception, Task>? OnFileReadError;
    /// <summary>
    /// The callback for when the file read state changes.
    /// </summary>
    event Func<Task>? OnFileReadStateChanged;
    /// <summary>
    /// The callback for when the show field mapper dialog flag changes.
    /// </summary>
    event Func<Task>? OnFieldMapperDisplayModeChanged;
    /// <summary>
    /// The callback for when the show transform preview flag changes.
    /// </summary>
    event Func<Task>? OnShowTransformPreviewChanged;

    /// <summary>
    /// Registers the data file mapper component.
    /// </summary>
    /// <typeparam name="TTargetType">The type of the target object to map to.</typeparam>
    /// <param name="importedDataFileDisplay">The data file mapper component to register.</param>
    void RegisterDataFileMapper<TTargetType>(DataFileMapper<TTargetType> importedDataFileDisplay)
        where TTargetType : class, new();

    /// <summary>
    /// Registers the data set display component.
    /// </summary>
    /// <param name="dataSetDisplay">
    /// The data set display component to register.
    /// </param>
    void RegisterDataSetDisplay(DataSetDisplay dataSetDisplay);

    /// <summary>
    /// Registers the file picker component.
    /// </summary>
    /// <param name="dataFilePicker">The file picker component to register.</param>
    void RegisterFilePicker(DataFilePicker dataFilePicker);

    /// <summary>
    /// Sets the field mappings for a table.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="incomingFieldMappings">The field mappings to set.</param>
    void ReplaceFieldMappings(string tableName, IEnumerable<FieldMapping> incomingFieldMappings);

    /// <summary>
    /// Updates the data file and shows the transform preview.
    /// </summary>
    Task UpdateAndShowTransformPreview();

    /// <summary>
    /// Updates the header row flag.
    /// </summary>
    /// <param name="hasHeaderRow">
    /// The new value for the header row flag.
    /// </param>
    /// <returns>
    /// A <see cref="Task" /> representing the asynchronous operation.
    /// </returns>
    Task UpdateHeaderRowFlag(bool hasHeaderRow);
}
