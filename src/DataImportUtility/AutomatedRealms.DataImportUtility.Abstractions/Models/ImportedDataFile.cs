using AutomatedRealms.DataImportUtility.Abstractions.CustomExceptions;
using AutomatedRealms.DataImportUtility.Abstractions.Helpers;

using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Reflection;
using System.Text.Json.Serialization;

namespace AutomatedRealms.DataImportUtility.Abstractions.Models; // Updated

/// <summary>
/// The result of reading a provided file.
/// </summary>
// TODO: Consider making this an interface to be able to create our own implementation in the tests
//       as opposed to using the helper class
public class ImportedDataFile
{
    #region Public Properties
    /// <summary>
    /// The file's name and extension.
    /// </summary>
    /// <remarks>
    /// This does not include the path.
    /// </remarks>
    public string? FileName { get; set; }
    /// <summary>
    /// The file's extension.
    /// </summary>
    /// <remarks>
    /// This is standardized to lower case and does not include the period.
    /// </remarks>
    public string? Extension { get; set; }
    /// <summary>
    /// Whether or not the file has a header row.
    /// </summary>
    public bool HasHeader { get; set; }
    /// <summary>
    /// The type of object to map to.
    /// </summary>
    [JsonIgnore]
    public Type? TargetType
    {
        get => _targetType;
        private set
        {
            if (value == _targetType) { return; }
            _targetType = value;
            TargetTypeString = value?.ToString();
        }
    }
    private Type? _targetType;

    /// <summary>
    /// The type of object to map to as a string.
    /// </summary>
    public string? TargetTypeString { get; set; }

    /// <summary>
    /// The data from the file.
    /// </summary>
    public DataSet? DataSet { get; private set; }

    /// <summary>    /// The table definitions for the data.
    /// </summary>
    /// <remarks>
    /// This represents the fields that are in the data as well as the mappings to a target type. 
    /// This is updated when the <see cref="SetData(DataSet?, bool, bool)"/> or 
    /// <see cref="RefreshFieldDescriptors(bool, string?)"/> methods are called.
    /// </remarks>
    public List<ImportTableDefinition> TableDefinitions { get; private set; } = []; // ImportTableDefinition will be in Core.Models

    /// <summary>
    /// The number of records in the data.
    /// </summary>
    public int RecordCount => DataSet?
        .Tables
        .OfType<DataTable>()
        .Sum(x => x.Rows.Count) ?? 0;
    #endregion Public Properties

    #region Private/Protected fields
    /// <summary>
    /// The field mappings to copy from for the target type.
    /// </summary>
    /// <remarks>
    /// This is generated when the <see cref="TargetType"/> is set and is used to generate the 
    /// <see cref="TableDefinitions"/> when the <see cref="RefreshFieldMappings(bool, bool)"/> method is 
    /// called.
    /// </remarks>
    private ImmutableList<FieldMapping>? _targetTypeFieldMappings; // FieldMapping is in Core.Models
    #endregion private/protected fields

    #region Public Methods
    /// <summary>
    /// Gets a new list of field mappings generated for the <see cref="TargetType"/>.
    /// </summary>
    /// <remarks>
    /// If the <see cref="TargetType"/> has not been set (is null), this will return null.
    /// </remarks>
    public ImmutableList<FieldMapping>? GetTargetTypeFieldMappings() // FieldMapping is in Core.Models
    {
        if (TargetType is null) { return null; }

        var forRet = new List<FieldMapping>();
        var props = TargetType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var prop in props)
        {
            var fieldMapping = new FieldMapping { FieldName = prop.Name, FieldType = prop.PropertyType };
            var validationAttributes = prop.GetCustomAttributes<ValidationAttribute>(true);
            if (validationAttributes?.Any() ?? false)
            {
                fieldMapping.ValidationAttributes = validationAttributes.ToImmutableArray();
                if (validationAttributes.Any(x => x is RequiredAttribute))
                {
                    fieldMapping.Required = true;
                }
            }
            forRet.Add(fieldMapping);
        }
        return forRet.ToImmutableList();
    }

    /// <summary>
    /// Sets the data for the file.
    /// </summary>
    /// <param name="dataSet">The data to set.</param>
    /// <param name="hasHeader">Whether or not the data has a header row.</param>
    /// <param name="refreshFieldMappings">Whether or not to refresh the field mappings.</param>
    public void SetData(DataSet? dataSet, bool hasHeader, bool refreshFieldMappings = true)
    {
        DataSet = dataSet;
        HasHeader = hasHeader;
        if (refreshFieldMappings)
        {
            RefreshFieldMappings(true, true);
        }
    }

    /// <summary>
    /// Sets the target type for this imported data file.
    /// </summary>
    /// <typeparam name="TTargetType">
    /// The type of the target object to map to.
    /// </typeparam>
    /// <param name="refreshFieldMappings">
    /// Whether or not to refresh the field mappings.
    /// </param>
    /// <param name="autoMatch">
    /// Whether or not to automatically match fields.
    /// </param>
    public void SetTargetType<TTargetType>(bool refreshFieldMappings = true, bool autoMatch = true) where TTargetType : new()
        => SetTargetType(typeof(TTargetType), refreshFieldMappings, autoMatch);


    /* Removing the version with the ignore/require fields for now 
     * TODO: Restore this when we have a better way to handle the ignore/require fields
    /// <summary>
    /// Sets the target type for this imported data file.
    /// </summary>
    /// <typeparam name="TTargetType">
    /// The type of the target object to map to.
    /// </typeparam>
    /// <param name="ignoreFields">
    /// The fields to ignore when mapping to the target type.
    /// </param>
    /// <param name="requireFields">
    /// The fields to mark as required when mapping to the target type.
    /// </param>
    public void SetTargetType<TTargetType>(IEnumerable<string>? ignoreFields = null, IEnumerable<string>? requireFields = null) where TTargetType : new()
        => SetTargetType(typeof(TTargetType), ignoreFields, requireFields);
    */

    /// <summary>
    /// Sets the target type for the file.
    /// </summary>
    /// <param name="targetType">The target type to set.</param>
    /// <param name="refreshFieldMappings">Whether or not to refresh the field mappings.</param>
    /// <param name="autoMatch">Whether or not to automatically match fields.</param>
    public void SetTargetType(Type? targetType, bool refreshFieldMappings = true, bool autoMatch = true)
    {
        TargetType = targetType;
        if (refreshFieldMappings)
        {
            RefreshFieldMappings(true, autoMatch);
        }
    }

    /// <summary>
    /// Refreshes the field mappings for the file.
    /// </summary>
    /// <param name="overwriteExisting">Whether or not to overwrite existing field mappings.</param>
    /// <param name="autoMatch">Whether or not to automatically match fields.</param>
    public void RefreshFieldMappings(bool overwriteExisting = false, bool autoMatch = false)
    {
        if (DataSet is null) { return; }

        _targetTypeFieldMappings = GetTargetTypeFieldMappings();

        foreach (DataTable table in DataSet.Tables)
        {
            var tableDefinition = TableDefinitions.FirstOrDefault(x => x.TableName == table.TableName);
            if (tableDefinition is null)
            {
                tableDefinition = new ImportTableDefinition { TableName = table.TableName };
                TableDefinitions.Add(tableDefinition);
            }

            tableDefinition.RefreshFieldDescriptors(table, HasHeader, _targetTypeFieldMappings, overwriteExisting, autoMatch);
        }
    }

    /// <summary>
    /// Refreshes the field descriptors for the specified table.
    /// </summary>
    /// <param name="overwriteExisting">Whether or not to overwrite existing field mappings.</param>
    /// <param name="tableName">The name of the table to refresh the field descriptors for.</param>
    public void RefreshFieldDescriptors(bool overwriteExisting = false, string? tableName = null)
    {
        if (DataSet is null) { return; }

        _targetTypeFieldMappings = GetTargetTypeFieldMappings();

        foreach (DataTable table in DataSet.Tables)
        {
            if (!string.IsNullOrWhiteSpace(tableName) && table.TableName != tableName) { continue; }

            var tableDefinition = TableDefinitions.FirstOrDefault(x => x.TableName == table.TableName);
            if (tableDefinition is null)
            {
                tableDefinition = new ImportTableDefinition { TableName = table.TableName };
                TableDefinitions.Add(tableDefinition);
            }

            tableDefinition.RefreshFieldDescriptors(table, HasHeader, _targetTypeFieldMappings, overwriteExisting);
        }
    }

    /// <summary>
    /// Gets the data from the file as a list of objects of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of object to map to.</typeparam>
    /// <param name="tableDefinition">The table definition to use for mapping.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The data from the file as a list of objects of the specified type.</returns>
    /// <exception cref="InvalidOperationException">The target type has not been set.</exception>
    /// <exception cref="MissingFieldMappingException">A required field is not mapped.</exception>
    public async Task<List<T>> GetData<T>(ImportTableDefinition tableDefinition, CancellationToken ct = default) where T : class, new()
    {
        if (TargetType is null)
        {
            throw new InvalidOperationException("Target type has not been set.");
        }

        var dataTable = DataSet?.Tables[tableDefinition.TableName];
        if (dataTable is null) { return []; }

        var forRet = new List<T>();
        foreach (DataRow row in dataTable.Rows)
        {
            if (ct.IsCancellationRequested) { break; }
            var obj = new T();
            foreach (var fieldMapping in tableDefinition.FieldMappings)
            {
                if (fieldMapping.IgnoreMapping) { continue; }
                var transformedResult = await fieldMapping.Apply(row);
                if (transformedResult is null || transformedResult.CurrentValue is null)
                {
                    if (fieldMapping.Required)
                    {
                        throw new MissingFieldMappingException([fieldMapping]);
                    }
                    continue;
                }

                var prop = TargetType.GetProperty(fieldMapping.FieldName);
                if (prop is null) { continue; }

                try
                {
                    var convertedValue = ReflectionHelpers.ChangeType(transformedResult.CurrentValue, prop.PropertyType);
                    prop.SetValue(obj, convertedValue);
                }
                catch (Exception ex) when (ex is FormatException || ex is InvalidCastException || ex is OverflowException)
                {
                    // TODO: Add logging here
                    // For now, just ignore the error and continue
                    // This is because the value may not be convertible to the target type
                    // and we don't want to stop the import process
                    // We should probably add a way to report these errors to the user
                    // and allow them to decide how to handle them
                    // For example, they could choose to ignore the error, replace the value
                    // with a default value, or stop the import process
                    // For now, we will just ignore the error and continue
                    // This means that the field will not be set on the object
                    // if the value cannot be converted to the target type
                    // This may not be the desired behavior in all cases
                    // but it is the safest behavior for now
                    // We should revisit this later
                    // See issue #123
                    // https://github.com/AutomatedRealms/DataImportUtility/issues/123
                    // TODO: Add a way to report these errors to the user
                    // TODO: Allow the user to decide how to handle these errors
                    // TODO: Add logging here
                }
            }
            forRet.Add(obj);
        }
        return forRet;
    }

    /// <summary>
    /// Gets the data from the file as a list of objects of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of object to map to.</typeparam>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The data from the file as a list of objects of the specified type.</returns>
    /// <remarks>
    /// This method will use the first table definition in the <see cref="TableDefinitions"/> list.
    /// </remarks>
    public Task<List<T>> GetData<T>(CancellationToken ct = default) where T : class, new()
        => GetData<T>(TableDefinitions.First(), ct);

    // NOTE: Some of this may have been replaced by the GetData methods above
    #region Apply Methods
    /// <summary>
    /// Applies the transformations to this imported data file and outputs the results as a 
    /// <see cref="DataTable"/>.
    /// </summary>
    /// <param name="tableName">The name of the table to apply the transformation to.</param>
    /// <param name="selectedRecords">The records to select from the table.</param>
    /// <returns>
    /// The transformed data as a <see cref="DataTable"/>.
    /// </returns>
    /// <remarks>
    /// If the <paramref name="selectedRecords"/> parameter is not provided, all records in the table will be selected.
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// Thrown when the table does not exist in the data set.
    /// </exception>
    public async Task<DataTable?> GenerateOutputDataTable(string tableName, List<int>? selectedRecords = null)
    {
        if (DataSet is null) { return null; }

        var table = DataSet.Tables[tableName]
            ?? throw new ArgumentException($"The table '{tableName}' does not exist in the data set.");

        if (!TableDefinitions.TryGetFieldMappings(tableName, out var fieldMappings) || fieldMappings is null)
        {
            throw new ArgumentException($"The table '{tableName}' does not have any field mappings.");
        }

        // Uses the helper/extension method to apply the transformation
        return await table.ApplyTransformation(fieldMappings, selectedRecords);
    }

    /// <summary>
    /// Applies the transformations to this imported data file and outputs the results as the specified
    /// type.
    /// </summary>
    /// <typeparam name="TTargetType">The type of the target object to map to.</typeparam>
    /// <param name="tableName">The name of the table to apply the transformation to.</param>
    /// <param name="selectedRecords">The records to select from the table.</param>
    /// <returns>A list of objects of the specified type.</returns>
    /// <remarks>
    /// Uses the helper/extension method to apply the transformation, which also applies validation
    /// to the data.  They can be found in the <see cref="ImportedDataFile.TableDefinitions"/>.<see cref="ImportTableDefinition.FieldMappings"/>
    /// objects.
    /// 
    /// If the <paramref name="selectedRecords"/> parameter is not provided, all records in the table will be selected.
    /// </remarks> 
    public async Task<IEnumerable<TTargetType>?> GenerateOutput<TTargetType>(string tableName, List<int>? selectedRecords = null)
        where TTargetType : new()
        => (await GenerateOutputDataTable(tableName, selectedRecords))?.ToObject<TTargetType>();

    /// <summary>
    /// Applies the transformations to this imported data file and outputs the results as a list of objects.
    /// </summary>
    /// <param name="tableName">The name of the table to apply the transformation to.</param>
    /// <param name="selectedRecords">The records to select from the table.</param>
    /// <returns>A list of objects.</returns>
    /// <remarks>
    /// If the <paramref name="selectedRecords"/> parameter is not provided, all records in the table will be selected.
    /// </remarks>
    public async Task<IEnumerable<object?>?> GenerateOutput(string tableName, List<int>? selectedRecords = null)
        => (await GenerateOutputDataTable(tableName, selectedRecords))?.ToObject<object?>();
    #endregion


    // NOTE: Another method that went away during the refactor:
    /// <summary>
    /// Replaces the field mappings for a table.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="incomingFieldMappings">The new field mappings to replace the existing ones.</param>
    /// <exception cref="InvalidOperationException">Thrown when the data file is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the table does not exist in the data set.</exception>
    public void ReplaceFieldMappings(string tableName, IEnumerable<FieldMapping> incomingFieldMappings)
    {
        if (DataSet is null)
        {
            throw new InvalidOperationException("Data file's DataSet is null.");
        }

        if (DataSet.Tables[tableName] is null)
        {
            throw new ArgumentException($"The table '{tableName}' does not exist in the data set.");
        }

        if (!TableDefinitions.TryGetTableDefinition(tableName, out var tableDef) || tableDef is null)
        {
            RefreshFieldDescriptors(tableName: tableName);
            RefreshFieldMappings();
            if (!TableDefinitions.TryGetTableDefinition(tableName, out tableDef) || tableDef is null)
            {
                throw new InvalidOperationException($"Failed to populate the field descriptors for the table '{tableName}'.");
            }
        }

        tableDef.FieldMappings = [.. incomingFieldMappings];
        var foundDescriptors = TableDefinitions.TryGetFieldDescriptors(tableName, out var fieldDescriptors);

        foreach (var fieldMapping in tableDef.FieldMappings)
        {
            fieldMapping.ValidationAttributes = _targetTypeFieldMappings!.First(x => x.FieldName == fieldMapping.FieldName).ValidationAttributes;
            foreach (var sourceFieldDef in (fieldMapping.MappingRule?.SourceFieldTransformations ?? []).Where(sfd => sfd?.Field is not null))
            {
                sourceFieldDef.Field = !foundDescriptors ? null : fieldDescriptors.FirstOrDefault(x => x.FieldName == sourceFieldDef.Field!.FieldName);
            }
        }
    }

    /// <summary>
    /// Clones the <see cref="ImportedDataFile" />.
    /// </summary>
    /// <returns>The cloned <see cref="ImportedDataFile" />.</returns>
    public ImportedDataFile Clone()
    {
        var forRet = (ImportedDataFile)MemberwiseClone();
        forRet.DataSet = DataSet?.Copy();
        forRet.TableDefinitions = [.. TableDefinitions.Select(x => x.Clone())];
        forRet._targetTypeFieldMappings = _targetTypeFieldMappings?.Select(x => x.Clone()).ToImmutableList(); return forRet;
    }
    #endregion Public Methods
}

/// <summary>
/// The result of reading a provided file with a specific target type.
/// </summary>
/// <typeparam name="TTargetType">
/// The type of the target object to map to.
/// </typeparam>
public class ImportedDataFile<TTargetType> : ImportedDataFile
    where TTargetType : class, new()
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ImportedDataFile{TTargetType}"/> class.
    /// </summary>
    public ImportedDataFile()
    {
        SetTargetType(typeof(TTargetType));
    }

    /// <summary>
    /// Gets the data from the file as a list of objects of the target type.
    /// </summary>
    /// <param name="tableDefinition">The table definition to use for mapping.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The data from the file as a list of objects of the target type.</returns>
    public Task<List<TTargetType>> GetData(ImportTableDefinition tableDefinition, CancellationToken ct = default)
        => GetData<TTargetType>(tableDefinition, ct);

    /// <summary>
    /// Gets the data from the file as a list of objects of the target type.
    /// </summary>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The data from the file as a list of objects of the target type.</returns>
    public Task<List<TTargetType>> GetData(CancellationToken ct = default)
        => GetData<TTargetType>(ct);

    /// <summary>
    /// Clones the <see cref="ImportedDataFile{TTargetType}" />.
    /// </summary>
    /// <returns>The cloned <see cref="ImportedDataFile{TTargetType}" />.</returns>
    public new ImportedDataFile<TTargetType> Clone()
    {
        return (ImportedDataFile<TTargetType>)base.Clone();
    }
}
