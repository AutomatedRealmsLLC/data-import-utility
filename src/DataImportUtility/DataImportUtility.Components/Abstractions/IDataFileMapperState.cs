using System.Collections.Immutable;
using System.ComponentModel;
using System.Data;
using AutomatedRealms.DataImportUtility.Abstractions; // For IProgressReporter
using AutomatedRealms.DataImportUtility.Core.Models; // For ImportedDataFile, FieldMapping, ImportTableDefinition
using AutomatedRealms.DataImportUtility.Components.Models; // For ImportDataFileRequest, FileReadState
using AutomatedRealms.DataImportUtility.Components.FieldMappingComponents.Wrappers; // For FieldMapperDisplayMode

namespace AutomatedRealms.DataImportUtility.Components.Abstractions;

/// <summary>
/// Defines the contract for the state management of the data file mapper component.
/// </summary>
public interface IDataFileMapperState : INotifyPropertyChanged, INotifyPropertyChanging
{
    /// <summary>
    /// Gets or sets the currently active data table being viewed or manipulated.
    /// </summary>
    DataTable? ActiveDataTable { get; set; }

    /// <summary>
    /// Gets the currently loaded data file, including its data and schema.
    /// </summary>
    ImportedDataFile? DataFile { get; }

    /// <summary>
    /// Gets the current state of the file reading process.
    /// </summary>
    FileReadState FileReadState { get; }

    /// <summary>
    /// Gets any error message that occurred during file processing.
    /// </summary>
    string? ErrorMessage { get; }

    /// <summary>
    /// Gets or sets a value indicating whether to show the transformed data preview.
    /// </summary>
    bool ShowTransformPreview { get; set; }

    /// <summary>
    /// Gets or sets the display mode for the field mapper.
    /// </summary>
    FieldMapperDisplayMode FieldMapperDisplayMode { get; set; }

    /// <summary>
    /// Gets or sets the list of selected rows from the import data.
    /// </summary>
    ImmutableList<DataRow>? SelectedImportRows { get; set; }

    /// <summary>
    /// Gets the active table definition, including field mappings.
    /// </summary>
    ImportTableDefinition? ActiveTableDefinition { get; }

    /// <summary>
    /// Occurs when the active data table changes.
    /// </summary>
    event Func<Task>? OnActiveDataTableChanged;

    /// <summary>
    /// Occurs when the data file changes (e.g., a new file is loaded).
    /// </summary>
    event Func<Task>? OnDataFileChanged;

    /// <summary>
    /// Occurs when the file read state changes.
    /// </summary>
    event Func<FileReadState, Task>? OnFileReadStateChanged;

    /// <summary>
    /// Occurs when an error is encountered during file reading or processing.
    /// </summary>
    event Func<Exception?, Task>? OnFileReadError;

    /// <summary>
    /// Occurs when the field mapper display mode changes.
    /// </summary>
    event Func<Task>? OnFieldMapperDisplayModeChanged;

    /// <summary>
    /// Occurs when the field mappings change.
    /// </summary>
    event Func<Task>? OnFieldMappingsChanged;

    /// <summary>
    /// Occurs when the visibility of the transform preview changes.
    /// </summary>
    event Func<Task>? OnShowTransformPreviewChanged;

    /// <summary>
    /// Occurs when a specific property in the state has changed.
    /// The string argument is the name of the property.
    /// </summary>
    event Func<string, Task>? OnStatePropertyChanged;

    /// <summary>
    /// Asynchronously sets the file to be processed.
    /// </summary>
    /// <param name="request">The request containing the file and import options.</param>
    /// <param name="progressReporter">An optional progress reporter to track the import process.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SetFileAsync(ImportDataFileRequest request, IProgressReporter? progressReporter = null);

    /// <summary>
    /// Sets the target type for data mapping.
    /// </summary>
    /// <typeparam name="T">The target type.</typeparam>
    /// <param name="autoMatchFields">Whether to automatically match fields between the source and target.</param>
    void SetTargetType<T>(bool autoMatchFields = true) where T : class, new();

    /// <summary>
    /// Replaces the field mappings for a specified table.
    /// </summary>
    /// <param name="tableName">The name of the table whose mappings are to be replaced.</param>
    /// <param name="incomingFieldMappings">The new set of field mappings.</param>
    void ReplaceFieldMappings(string tableName, IEnumerable<FieldMapping> incomingFieldMappings);

    /// <summary>
    /// Refreshes the field mappings, optionally overwriting existing ones or auto-matching.
    /// </summary>
    /// <param name="overwriteExisting">If true, existing mappings are overwritten.</param>
    /// <param name="autoMatch">If true, fields are automatically matched.</param>
    void RefreshFieldMappings(bool overwriteExisting = false, bool autoMatch = false);
}
