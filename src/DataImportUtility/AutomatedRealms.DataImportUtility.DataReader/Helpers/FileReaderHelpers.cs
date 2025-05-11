using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ExcelDataReader;
using System;

namespace AutomatedRealms.DataImportUtility.DataReader.Helpers; // Updated namespace

// Excel file reading adapted from https://medium.com/@bonzer.web/read-excel-file-data-into-a-data-table-c-net-core-9dfb705e83f2
/// <summary>
/// A helper class for reading Excel and other structured files.
/// </summary>
public static class FileReaderHelpers
{
    /// <summary>
    /// Reads an Excel file asynchronously.
    /// </summary>
    /// <param name="filePath">The path to the Excel file.</param>
    /// <param name="useHeaderRow">Whether to use the first row as the header row.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>
    /// The data set comprised of the worksheets as data tables.
    /// </returns>
    public static Task<DataSet> ReadFromExcelFileAsync(this string filePath, bool useHeaderRow = false, CancellationToken ct = default)
    {
        using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
        return ReadFromExcelFileAsync(stream, useHeaderRow, ct);
    }

    /// <summary>
    /// Reads an Excel file from a stream asynchronously.
    /// </summary>
    /// <param name="stream">The stream containing the Excel file.</param>
    /// <param name="useHeaderRow">Whether to use the first row as the header row.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>
    /// The data set comprised of the worksheets as data tables.
    /// </returns>
    public static async Task<DataSet> ReadFromExcelFileAsync(this Stream stream, bool useHeaderRow = false, CancellationToken ct = default)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var encoding = Encoding.GetEncoding("UTF-8");
        try
        {
            using var memoryStream = new MemoryStream();

            // Corrected loop for async stream copy
            byte[] buffer = new byte[81920]; // Standard buffer size
            int bytesRead;
            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, ct).ConfigureAwait(false)) > 0)
            {
                await memoryStream.WriteAsync(buffer, 0, bytesRead, ct).ConfigureAwait(false);
                if (ct.IsCancellationRequested) ct.ThrowIfCancellationRequested();
            }

            memoryStream.Position = 0;

            using var reader = ExcelReaderFactory.CreateReader(
                memoryStream,
                new ExcelReaderConfiguration()
                {
                    FallbackEncoding = encoding
                });

            return reader.AsDataSet(new ExcelDataSetConfiguration
            {
                ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = useHeaderRow },
                UseColumnDataType = true
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            throw;
        }
    }

    /// <summary>
    /// Reads an Excel file.
    /// </summary>
    /// <param name="filePath">The path to the Excel file.</param>
    /// <param name="useHeaderRow">Whether to use the first row as the header row.</param>
    /// <returns>
    /// The data set comprised of the worksheets as data tables.
    /// </returns>
    public static DataSet ReadFromExcelFile(this string filePath, bool useHeaderRow = false)
    {
        using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
        return ReadFromExcelFile(stream, useHeaderRow);
    }

    /// <summary>
    /// Reads an Excel file from a stream.
    /// </summary>
    /// <param name="stream">The stream containing the Excel file.</param>
    /// <param name="useHeaderRow">Whether to use the first row as the header row.</param>
    /// <returns>
    /// The data set comprised of the worksheets as data tables.
    /// </returns>
    public static DataSet ReadFromExcelFile(this Stream stream, bool useHeaderRow = false)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var encoding = Encoding.GetEncoding("UTF-8");
        try
        {
            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            memoryStream.Position = 0;

            using var reader = ExcelReaderFactory.CreateReader(
                memoryStream,
                new ExcelReaderConfiguration()
                {
                    FallbackEncoding = encoding
                });

            return reader.AsDataSet(new ExcelDataSetConfiguration
            {
                ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = useHeaderRow },
                UseColumnDataType = true
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            throw;
        }
    }

    /// <summary>
    /// Reads a CSV file asynchronously.
    /// </summary>
    /// <param name="filePath">The path to the CSV file.</param>
    /// <param name="useHeaderRow">Whether to use the first row as the header row.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>
    /// The data set comprised of the CSV file as a data table.
    /// </returns>
    public static Task<DataSet> ReadFromCsvFileAsync(this string filePath, bool useHeaderRow = false, CancellationToken ct = default)
    {
        using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
        return ReadFromCsvFileAsync(stream, useHeaderRow, ct);
    }

    /// <summary>
    /// Reads a CSV file from a stream asynchronously.
    /// </summary>
    /// <param name="stream">The stream containing the CSV file.</param>
    /// <param name="useHeaderRow">Whether to use the first row as the header row.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>
    /// The data set comprised of the CSV file as a data table.
    /// </returns>
    public static async Task<DataSet> ReadFromCsvFileAsync(this Stream stream, bool useHeaderRow = false, CancellationToken ct = default)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var encoding = Encoding.GetEncoding("UTF-8");
        try
        {
            using var memoryStream = new MemoryStream();

            // Corrected loop for async stream copy
            byte[] buffer = new byte[81920]; // Standard buffer size
            int bytesRead;
            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, ct).ConfigureAwait(false)) > 0)
            {
                await memoryStream.WriteAsync(buffer, 0, bytesRead, ct).ConfigureAwait(false);
                if (ct.IsCancellationRequested) ct.ThrowIfCancellationRequested();
            }

            memoryStream.Position = 0;

            using var reader = ExcelReaderFactory.CreateCsvReader(
                memoryStream,
                new ExcelReaderConfiguration()
                {
                    FallbackEncoding = encoding,
                    AutodetectSeparators = [',', ';', '\t', '|', ':']
                });

            return reader.AsDataSet(new ExcelDataSetConfiguration
            {
                ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = useHeaderRow },
                UseColumnDataType = true
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            throw;
        }
    }

    /// <summary>
    /// Reads a CSV file.
    /// </summary>
    /// <param name="filePath">The path to the CSV file.</param>
    /// <param name="useHeaderRow">Whether to use the first row as the header row.</param>
    /// <returns>
    /// The data set comprised of the CSV file as a data table.
    /// </returns>
    public static DataSet ReadFromCsvFile(this string filePath, bool useHeaderRow = false)
    {
        using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
        return ReadFromCsvFile(stream, useHeaderRow);
    }

    /// <summary>
    /// Reads a CSV file from a stream.
    /// </summary>
    /// <param name="stream">The stream containing the CSV file.</param>
    /// <param name="useHeaderRow">Whether to use the first row as the header row.</param>
    /// <returns>
    /// The data set comprised of the CSV file as a data table.
    /// </returns>
    public static DataSet ReadFromCsvFile(this Stream stream, bool useHeaderRow = false)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var encoding = Encoding.GetEncoding("UTF-8");
        try
        {
            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            memoryStream.Position = 0;

            using var reader = ExcelReaderFactory.CreateCsvReader(
                memoryStream,
                new ExcelReaderConfiguration()
                {
                    FallbackEncoding = encoding,
                    AutodetectSeparators = [',', ';', '\t', '|', ':']
                });

            return reader.AsDataSet(new ExcelDataSetConfiguration
            {
                ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = useHeaderRow },
                UseColumnDataType = true
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            throw;
        }
    }
}
