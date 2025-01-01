using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using DataImportUtility.Models;

namespace DataImportUtility.Abstractions;
/// <summary>
/// A service for reading data from a data source.
/// </summary>
public interface IDataReaderService
{
    /// <summary>
    /// Reads the data from the specified file.
    /// </summary>
    /// <param name="dataRequest">The request containing the file to read.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>
    /// The data read from the file along with the information about the fields in the file.
    /// </returns>
    Task<ImportedDataFile> ReadImportFile(IImportDataFileRequest dataRequest, CancellationToken ct = default);
    /// <summary>
    /// Tries to match the fields in the data set to the mappable fields.
    /// </summary>
    /// <param name="importedDataFile">The data set to match the fields to.</param>
    /// <param name="tableName">The name of the data table to match the fields to.</param>
    /// <param name="mappableFields">The available fields to match to the data set.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <exception cref="ArgumentException">The table name does not exist in the data set.</exception>
    Task TryMatchingFields(ImportedDataFile importedDataFile, string tableName, IEnumerable<FieldMapping> mappableFields, CancellationToken ct = default);
}
