using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Reflection;
using System.Text.Json.Serialization;

using DataImportUtility.Abstractions;
using DataImportUtility.CustomExceptions;
using DataImportUtility.Helpers;

namespace DataImportUtility.Models;

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

    /// <summary>
    /// The table definitions for the data.
    /// </summary>
    /// <remarks>
    /// This represents the fields that are in the data as well as the mappings to a target type. 
    /// This is updated when the <see cref="SetData(DataSet?, bool, bool)"/> or 
    /// <see cref="RefreshFieldDescriptors(bool, string?)"/> methods are called.
    /// </remarks>
    public List<ImportTableDefinition> TableDefinitions { get; private set; } = [];

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
    private ImmutableList<FieldMapping>? _targetTypeFieldMappings;
    #endregion private/protected fields

    #region Public Methods
    /// <summary>
    /// Gets a new list of field mappings generated for the <see cref="TargetType"/>.
    /// </summary>
    /// <remarks>
    /// If the <see cref="TargetType"/> has not been set (is null), this will return null.
    /// </remarks>
    public IReadOnlyCollection<FieldMapping>? GetTargetTypeFieldMappingCollection()
    {
        if (TargetType is null) { return null; }

        _targetTypeFieldMappings ??= GenerateFieldsToMapTo();

        return _targetTypeFieldMappings.Select(x => x.Clone()).ToList();
    }

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

    /// <summary>
    /// Sets the target type for this imported data file.
    /// </summary>
    /// <param name="targetType">The type of the target object to map to.</param>
    /// <param name="ignoreFields">The fields to ignore when mapping to the target type.</param>
    /// <param name="requireFields">The fields to mark as required when mapping to the target type.</param>
    /// <param name="autoMatchFields">Try to auto match the imported fields to the target type.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when the target type does not have a parameterless constructor.
    /// </exception>
    public void SetTargetType(Type targetType, IEnumerable<string>? ignoreFields = null, IEnumerable<string>? requireFields = null, bool autoMatchFields = false)
    {
        TargetType = targetType;

        _targetTypeFieldMappings = null;

        // Check if the target type is a class and has a parameterless constructor
        if (targetType.GetConstructor(Type.EmptyTypes) is null)
        {
            throw new ArgumentException("The target type must have a parameterless constructor.");
        }

        _targetTypeFieldMappings = GenerateFieldsToMapTo(ignoreFields, requireFields);

        // Refresh the field mappings
        RefreshFieldMappings(preserveValidMappings: false, autoMatchFields: autoMatchFields);
    }

    /// <summary>
    /// Sets the data for this imported data file and initializes the field descriptors and mappings.
    /// </summary>
    /// <param name="dataSet">The generated <see cref="System.Data.DataSet"/> produced when reading from a source file.</param>
    /// <param name="preserveValidMappings">
    /// Whether or not to preserve the valid mappings that already exist. 
    /// </param>
    /// <param name="autoMatchFields">
    /// Try to auto match the imported fields to the target type.
    /// </param>
    /// <remarks>
    /// Valid mappings mappings are those that are for tables that exist in the new 
    /// data set and only field names that exist for that table in the new data set
    /// are preserved when <paramref name="preserveValidMappings"/> is true.
    /// 
    /// If the data is null, the field descriptors will be cleared, but the all of the 
    /// existing mappings will be preserved unless <paramref name="preserveValidMappings"/>
    /// is false.
    /// </remarks>
    public virtual void SetData(DataSet? dataSet, bool preserveValidMappings = true, bool autoMatchFields = false)
    {
        DataSet = dataSet;
        TableDefinitions.Clear();
        RefreshFieldDescriptors();

        if (dataSet is null)
        {
            if (!preserveValidMappings) { TableDefinitions.Clear(); }
            return;
        }

        RefreshFieldMappings(preserveValidMappings, autoMatchFields);
    }

    /// <summary>
    /// Refreshes the field descriptors for the data.
    /// </summary>
    /// <param name="onlyIfNotExists">
    /// Whether or not to only refresh the field descriptors if they do not already exist.
    /// </param>
    /// <param name="forTable">
    /// The name of the table to refresh the field descriptors for. If null, all tables will be refreshed.
    /// </param>
    /// <remarks>
    /// This method will not refresh the field descriptors if they already exist unless the
    /// <paramref name="onlyIfNotExists"/> parameter is set to false.
    /// </remarks>
    public virtual void RefreshFieldDescriptors(bool onlyIfNotExists = true, string? forTable = null)
    {
        if (DataSet is null) { return; }

        foreach (var curTable in DataSet.Tables.OfType<DataTable>().Where(x => forTable is null || forTable == x.TableName) ?? [])
        {
            if (onlyIfNotExists && TableDefinitions.TryGetFieldDescriptors(curTable.TableName, out var existDescriptors) && existDescriptors.Count > 0)
            {
                continue;
            }

            var updatedDescriptors = curTable.Columns
                .OfType<DataColumn>()
                .Select(x => new ImportedRecordFieldDescriptor()
                {
                    ImportedDataFile = this,
                    ForTableName = curTable.TableName,
                    FieldName = x.ColumnName,
                    FieldType = x.DataType
                })
                .ToList();

            if (!TableDefinitions.TryAdd(curTable.TableName, updatedDescriptors))
            {
                TableDefinitions.Get(curTable.TableName).FieldDescriptors = updatedDescriptors;
            }
        }
    }

    /// <summary>
    /// Refreshes the field mappings for the current <see cref="DataSet" />.
    /// </summary>
    /// <param name="preserveValidMappings">
    /// Whether or not to preserve the valid mappings that already exist.
    /// </param>
    /// <param name="autoMatchFields">
    /// Try to auto match the imported fields to the target type.
    /// </param>
    public virtual void RefreshFieldMappings(bool preserveValidMappings = true, bool autoMatchFields = false)
    {
        if (DataSet is null || TargetType is null)
        {
            if (TargetType is null)
            {
                _targetTypeFieldMappings = null;
            }
            return;
        }

        foreach (var curTable in DataSet.Tables.OfType<DataTable>() ?? [])
        {
            // This will always give back a new instance for each field mapping
            var fieldMappingSet = (GetTargetTypeFieldMappingCollection() ?? GenerateFieldsToMapTo()).ToArray();
            if (preserveValidMappings && TableDefinitions.TryGetFieldMappings(curTable.TableName, out var existMappings) && existMappings is not null)
            {
                TableDefinitions.Get(curTable.TableName).FieldMappings = MergeValidFieldMappings(curTable, fieldMappingSet, existMappings);
                continue;
            }

            if (!TableDefinitions.TryAdd(curTable.TableName, fieldMappings: [.. fieldMappingSet]))
            {
                TableDefinitions.Get(curTable.TableName).FieldMappings = [.. fieldMappingSet];
            }

            if (autoMatchFields)
            {
                TryMatchingFields(curTable.TableName);
            }
        }
    }

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

        if (!TableDefinitions.TryGetFieldMappings(tableName, out var fieldMappings))
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
    #endregion Public Methods

    #region Private Methods
    /// <summary>
    /// Generates the fields to map to for the specified target type.
    /// </summary>
    /// <param name="ignoreFields">The fields to ignore when mapping to the target type.</param>
    /// <param name="requiredFields">
    /// The fields to mark as required when mapping to the target type.
    /// </param>
    /// <returns>
    /// An array of <see cref="FieldMapping"/> objects that represent the fields to map to.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the target type has not been set.
    /// </exception>
    private ImmutableList<FieldMapping> GenerateFieldsToMapTo(IEnumerable<string>? ignoreFields = null, IEnumerable<string>? requiredFields = null)
    {
        if (TargetType is null)
        {
            throw new InvalidOperationException("The target type must be set before generating the fields to map to.");
        }

        if (TargetType.GetConstructor(Type.EmptyTypes) is null)
        {
            throw new ArgumentException("The target type must have a parameterless constructor.");
        }

        var retList = TargetType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => !(ignoreFields?.Contains(p.Name) ?? false))
            .Select(p => p.AsFieldMapping(requiredFields?.Contains(p.Name) ?? false));

        return retList
            .ToImmutableList();
    }

    /// <summary>
    /// Merges the valid field mappings with the new field mappings.
    /// </summary>
    /// <param name="curTable">The current table to merge the mappings for.</param>
    /// <param name="fieldMappingSet">The full field mapping set to merge in.</param>
    /// <param name="existMappings">The existing mappings to merge into.</param>
    /// <returns>
    /// A list of <see cref="FieldMapping"/> objects that represent the merged mappings.
    /// </returns>
    private static List<FieldMapping> MergeValidFieldMappings(DataTable curTable, FieldMapping[] fieldMappingSet, List<FieldMapping> existMappings)
    {
        // Make sure all the field mappings are still valid
        var validFieldMappings = existMappings.Where(x => curTable.Columns.Contains(x.FieldName))
            .ToList();

        var mergedMappings = new List<FieldMapping>();

        for (var i = 0; i < fieldMappingSet.Length; i++)
        {
            var curFieldMapping = fieldMappingSet[i];
            var foundMapping = validFieldMappings.FirstOrDefault(x => x.FieldName == curFieldMapping.FieldName);
            if (foundMapping is not null)
            {
                mergedMappings.Add(foundMapping);
                continue;
            }

            mergedMappings.Add(curFieldMapping);
        }

        return mergedMappings;
    }

    /// <summary>
    /// Tries to automatically associate the fields in the imported data table with the target type.
    /// </summary>
    /// <param name="tableName">The name of the table to try to match fields for.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when the Field Mappings or the Field Descriptors for the table do not exist.
    /// </exception>
    /// <exception cref="InvalidOperationException">Thrown when the target type has not been set.</exception>
    public Task TryMatchingFields(string tableName, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var dataTable = (DataSet?.Tables[tableName]) ?? throw new ArgumentException($"The table '{tableName}' does not exist in the data set.");

        if (TargetType is null)
        {
            throw new InvalidOperationException("The target type must be set before trying to match fields.");
        }

        if (!TableDefinitions.TryGetFieldMappings(tableName, out var mappableFields))
        {
            throw new ArgumentException($"The table '{tableName}' does not have any field mappings.");
        }

        RefreshFieldDescriptors(forTable: tableName);
        if (!TableDefinitions.TryGetFieldDescriptors(dataTable.TableName, out var fieldDescriptors))
        {
            throw new ArgumentException($"Failed to populate the field descriptors for the table '{tableName}'.");
        }

        foreach (var fieldDescriptor in fieldDescriptors)
        {
            var colNameInvariant = fieldDescriptor.FieldName.ToStandardComparisonString();

            var matchingField =
                mappableFields.FirstOrDefault(x => x.FieldName.ToStandardComparisonString() == colNameInvariant && x.FieldType == fieldDescriptor.FieldType)
                ?? mappableFields.FirstOrDefault(x => x.FieldName.ToStandardComparisonString() == colNameInvariant);

            if (matchingField is null) { continue; }

            matchingField.MappingRule ??= MappingRuleType.CopyRule.CreateNewInstance();

            if (matchingField.MappingRule!.SourceFieldTranformations.Any(x => x.Field is not null))
            {
                continue;
            }

            matchingField.MappingRule.AddFieldTransformation(fieldDescriptor);
        }

        return Task.CompletedTask;
    }

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

        if (!TableDefinitions.TryGetTableDefinition(tableName, out var tableDef))
        {
            RefreshFieldDescriptors(forTable: tableName);
            RefreshFieldMappings();
            if (!TableDefinitions.TryGetTableDefinition(tableName, out tableDef))
            {
                throw new InvalidOperationException($"Failed to populate the field descriptors for the table '{tableName}'.");
            }
        }

        tableDef.FieldMappings = incomingFieldMappings.ToList();
        var foundDescriptors = TableDefinitions.TryGetFieldDescriptors(tableName, out var fieldDescriptors);

        foreach(var fieldMapping in tableDef.FieldMappings)
        {
            fieldMapping.ValidationAttributes = _targetTypeFieldMappings!.First(x => x.FieldName == fieldMapping.FieldName).ValidationAttributes;
#if NET8_0_OR_GREATER
            foreach (var sourceFieldDef in (fieldMapping.MappingRule?.SourceFieldTranformations ?? []).Where(sfd => sfd?.Field is not null))
#else
            foreach (var sourceFieldDef in (fieldMapping.MappingRule?.SourceFieldTranformations ?? ImmutableList<FieldTransformation>.Empty).Where(sfd => sfd?.Field is not null))
#endif
                {
                sourceFieldDef.Field = !foundDescriptors ? null : fieldDescriptors!.FirstOrDefault(x => x.FieldName == sourceFieldDef.Field!.FieldName);
            }
        }
    }
    #endregion Private Methods
}

/// <summary>
/// The result of reading a provided file.
/// </summary>
/// <typeparam name="TTargetType">
/// The type of the target object to map to.
/// </typeparam>
public class ImportedDataFile<TTargetType> : ImportedDataFile
    where TTargetType : new()
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ImportedDataFile{TTargetType}"/> class.
    /// </summary>
    /// <remarks>
    /// Public Properties of the <typeparamref name="TTargetType"/> that have the 
    /// <see cref="RequiredAttribute"/> attribute will be marked as required.
    /// </remarks>
    public ImportedDataFile() : base()
    {
        SetTargetType<TTargetType>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImportedDataFile{TTargetType}"/> class.
    /// </summary>
    /// <param name="ignoreFields">The fields to ignore when mapping to the target type.</param>
    /// <param name="requireFields">The fields to mark as required when mapping to the target type.</param>
    /// <remarks>
    /// Public Properties of the <typeparamref name="TTargetType"/> that have the 
    /// <see cref="RequiredAttribute"/> attribute will be marked as required.
    /// </remarks>
    public ImportedDataFile(IEnumerable<string>? ignoreFields, IEnumerable<string>? requireFields) : base()
    {
        SetTargetType<TTargetType>(ignoreFields, requireFields);
    }

    /// <summary>
    /// Applies the transformations to this imported data file and outputs the results as the specified
    /// type.
    /// </summary>
    /// <param name="tableName">The name of the table to apply the transformation to.</param>
    /// <param name="selectedRecords">The records to select from the table.</param>
    /// <returns>A list of objects of the specified type.</returns>
    /// <remarks>
    /// If the <paramref name="selectedRecords"/> parameter is not provided, all records in the table will be selected.
    /// </remarks>
    public new async Task<IEnumerable<TTargetType>?> GenerateOutput(string tableName, List<int>? selectedRecords = null)
        => (await GenerateOutputDataTable(tableName, selectedRecords))?.ToObject<TTargetType>();
}