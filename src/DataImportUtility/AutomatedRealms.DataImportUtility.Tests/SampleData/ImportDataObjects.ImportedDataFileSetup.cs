using System.Data;
using System.Text.Json;

using AutomatedRealms.DataImportUtility.Abstractions.Models;

namespace AutomatedRealms.DataImportUtility.Tests.SampleData;

internal static partial class ImportDataObjects
{
    private static ImportedDataFile? _dataFile;
    /// <summary>
    /// The pre-populated data file for testing.
    /// </summary>
    internal static ImportedDataFile DataFile
    {
        get
        {
            PrepareDataFile();
            return _dataFile!;
        }
    }

    /// <summary>
    /// An easy way to get the <see cref="ImportedDataFile.DataSet"/> for the main data file.
    /// </summary>
    /// <remarks>
    /// This ensures the data file is prepared before returning the data table.
    /// </remarks>
    internal static DataSet MainDataSet => DataFile.DataSet!;

    /// <summary>
    /// An easy way to get the main data table (first data table) for the main data file from the <see cref="ImportedDataFile.DataSet"/>.
    /// </summary>
    /// <remarks>
    /// This ensures the data file is prepared before returning the data table.
    /// </remarks>
    internal static DataTable MainDataTable => MainDataSet.Tables[DataTableName]!;

    /// <summary>
    /// An easy way to get the field descriptors for the main data table.
    /// </summary>
    internal static List<ImportedRecordFieldDescriptor> MainFieldDescriptors => DataFile.TableDefinitions.Get(DataTableName).FieldDescriptors;

    /// <summary>
    /// An easy way to get the field mappings for the main data table.
    /// </summary>
    internal static List<FieldMapping> MainFieldMappings => DataFile.TableDefinitions.Get(DataTableName).FieldMappings;

    private static readonly object _dataFilePreparationLock = new();

    private static void PrepareDataFile()
    {
        lock (_dataFilePreparationLock)
        {
            if (_dataFile is not null) { return; }
            _dataFile = new()
            {
                FileName = FileAndDataSetName,
                Extension = "csv",
                HasHeader = true
            };
            var dataSet = new DataSet(FileAndDataSetName);
            _dataFile.SetTargetType(typeof(FakeTargetType));
            _dataFile.RefreshFieldDescriptors(false);
            _dataFile.RefreshFieldMappings(overwriteExisting: true);

            var importTable = new DataTable(DataTableName);
            dataSet.Tables.Add(importTable);
            var data = JsonDocument.Parse(ImportJson);
            var rows = data.RootElement.EnumerateArray();
            var columns = rows.First().EnumerateObject();
            foreach (var column in columns)
            {
                importTable.Columns.Add(column.Name);
                var fieldDesc = new ImportedRecordFieldDescriptor()
                {
                    ImportedDataFile = _dataFile,
                    ForTableName = importTable.TableName,
                    FieldName = column.Name,
                    FieldType = typeof(string)
                };
            }

            foreach (var row in rows.Skip(1))
            {
                var newRow = importTable.NewRow();

                foreach (var column in columns)
                {
                    var rowInfo = row.GetProperty(column.Name);
                    newRow[column.Name] = rowInfo.ValueKind switch
                    {
                        JsonValueKind.String => rowInfo.GetString(),
                        JsonValueKind.Number => rowInfo.GetDouble().ToString(),
                        JsonValueKind.True => true.ToString(),
                        JsonValueKind.False => false.ToString(),
                        JsonValueKind.Null => null,
                        _ => rowInfo.GetString()
                    };
                }

                importTable.Rows.Add(newRow);
            }

            _dataFile.SetData(dataSet, _dataFile.HasHeader);
        }
    }

    internal static ImportedDataFile Clone(this ImportedDataFile importDataFile)
    {
        var clone = new ImportedDataFile()
        {
            FileName = importDataFile.FileName,
            Extension = importDataFile.Extension,
            HasHeader = importDataFile.HasHeader
        };
        clone.SetData(importDataFile.DataSet!.Copy(), importDataFile.HasHeader);
        clone.RefreshFieldDescriptors(false);
        clone.SetTargetType(importDataFile.TargetType!);
        clone.RefreshFieldMappings(overwriteExisting: true);
        return clone;
    }
}
