using System.Data;

using DataImportUtility.Abstractions;
using DataImportUtility.Helpers;
using DataImportUtility.Models;

namespace DataImportUtility.Components.Services;

/// <summary>
/// Service that reads data from a data source.
/// </summary>
public class DataReaderService : IDataReaderService
{
    private static readonly string[] _acceptedFileTypes = [
        "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "text/csv"
    ];

    /// <inheritdoc />
    /// <exception cref="ArgumentException">Throw when the file type is not CSV or Excel; or if the provided sheet name does not exist.</exception>
    /// <exception cref="ArgumentNullException">The file stream is null.</exception>
    public async Task<ImportedDataFile> ReadImportFile(IImportDataFileRequest dataRequest, CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested) { return await Task.FromCanceled<ImportedDataFile>(ct); }

        if (!_acceptedFileTypes.Contains(dataRequest.ContentType))
        {
            throw new ArgumentException("The file must be a CSV or Excel file.");
        }

        using var fileStream = dataRequest.OpenReadStream(cancellationToken: ct);

        ArgumentNullException.ThrowIfNull(fileStream);

        // Determine the file type and process it
        var dataSet = dataRequest.ContentType switch
        {
            "text/csv" => await FileReaderHelpers.ReadFromCsvFileAsync(fileStream, dataRequest.HasHeaderRow, ct),
            "application/vnd.ms-excel" or "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" => await FileReaderHelpers.ReadFromExcelFileAsync(fileStream, dataRequest.HasHeaderRow, ct),
            //".ods" => await ExcelFileReaderHelper.ReadOdsFileAsync(fileStream, request.HasHeaderRow, ct),
            _ => throw new NotImplementedException($"The data reader for content type '{dataRequest.ContentType}' has not been implemented.")
        };

        return await ProcessDataSet(dataRequest, dataSet, ct);
    }

    /// <inheritdoc />
    public Task TryMatchingFields(ImportedDataFile importedDataFile, string tableName, IEnumerable<FieldMapping> mappableFields, CancellationToken ct = default)
    {
        var dataTable = (importedDataFile.DataSet?.Tables[tableName]) ?? throw new ArgumentException($"The table '{tableName}' does not exist in the data set.");
        importedDataFile.RefreshFieldDescriptors(forTable: tableName);

        if (!importedDataFile.TableDefinitions.TryGetFieldDescriptors(dataTable.TableName, out var fieldDescriptors))
        {
            throw new ArgumentException($"Failed to populate the field descriptors for the table '{tableName}'.");
        }

        foreach (var fieldDescriptor in fieldDescriptors)
        {
            var colNameInvariant = fieldDescriptor.FieldName.ToStandardComparisonString();

            var matchingField =
                mappableFields.FirstOrDefault(x => x.FieldName.ToStandardComparisonString() == colNameInvariant && x.FieldType == fieldDescriptor.FieldType)
                ?? mappableFields.FirstOrDefault(x => x.FieldName.ToStandardComparisonString() == colNameInvariant);

            if (matchingField?.MappingRule is null || matchingField.MappingRule.SourceFieldTransformations.Any(x => x.Field is not null))
            {
                continue;
            }

            matchingField.MappingRule.AddFieldTransformation(fieldDescriptor);
        }

        return Task.CompletedTask;
    }

    #region Helpers
    private static Task<ImportedDataFile> ProcessDataSet(IImportDataFileRequest request, DataSet dataSet, CancellationToken ct = default)
    {
        var dataFile = new ImportedDataFile()
        {
            FileName = request.Name,
            Extension = Path.GetExtension(request.Name ?? string.Empty).ToLowerInvariant().Replace(".", null),
            HasHeader = request.HasHeaderRow
        };

        var isCsv = dataFile.Extension.Equals("csv", StringComparison.InvariantCultureIgnoreCase);

        if (dataSet.Tables.Count == 0)
        {
            return Task.FromResult(dataFile);
        }

        // If this is a CSV file, there is only one table, so return it
        if (isCsv)
        {
            var dataTable = dataSet.Tables[0];

            // Check that the table has data
            if (dataTable.Rows.Count == 0)
            {
                return Task.FromResult(dataFile);
            }

            dataFile.SetData(dataSet, true);
            return Task.FromResult(dataFile);
        }

        var sheetName = request.SheetName ?? string.Empty;
        var useDataTable = !string.IsNullOrEmpty(sheetName) && dataSet.Tables.Contains(sheetName)
            ? dataSet.Tables[sheetName]!
            : null;

        // If there was a sheet name specified
        if (!string.IsNullOrEmpty(sheetName))
        {
            // Check if it exists
            if (useDataTable is null)
            {
                throw new ArgumentException($"The file does not contain a sheet named '{request.SheetName}'.");
            }

            // Check if it has data
            if (useDataTable.Rows.Count == 0)
            {
                return Task.FromResult(dataFile);
            }

            // Since we are returning only one sheet, we can process it now
            // Remove the other tables
            dataSet.Tables.Clear();
            dataSet.Tables.Add(useDataTable);
            dataFile.SetData(dataSet, true);

            return Task.FromResult(dataFile);
        }

        // If we are here, no sheet name was specified
        // Only use tables with data
        List<DataTable> tablesToKeep = [];

        // Check for exactly one sheet with data
        foreach (DataTable dataTable in dataSet.Tables)
        {
            if (dataTable.Rows.Count > 0)
            {
                // Keeping track of at least one sheet that has data
                tablesToKeep.Add(dataTable);
            }
        }

        if (tablesToKeep.Count != dataSet.Tables.Count)
        {
            // Some or all sheets are empty
            dataSet.Tables.Clear();
            foreach (var dataTable in tablesToKeep)
            {
                dataSet.Tables.Add(dataTable);
            }
        }

        dataFile.SetData(dataSet, false);

        return Task.FromResult(dataFile);
    }
    #endregion Helpers
}
