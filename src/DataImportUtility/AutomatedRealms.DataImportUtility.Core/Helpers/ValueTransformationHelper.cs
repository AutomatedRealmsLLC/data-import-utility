using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations; // Added for ValidationResult and Validator
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutomatedRealms.DataImportUtility.Abstractions.Enums;
using AutomatedRealms.DataImportUtility.Abstractions.Models; // Using Abstractions models
using AutomatedRealms.DataImportUtility.Core.ValueTransformations;

namespace AutomatedRealms.DataImportUtility.Core.Helpers;

/// <summary>
/// Extension methods for applying value transformations.
/// </summary>
public static partial class ValueTransformationHelper
{
    /// <summary>
    /// Gets the values of a column in a data table.
    /// </summary>
    /// <param name="sourceTable">The source table to get the values from.</param>
    /// <param name="forField">The name of the field to get the values for.</param>
    /// <returns>
    /// An array of objects that represent the values of the specified field in the data 
    /// table in the order they appear in the table at the time the method was called.
    /// </returns>
    public static object[] GetColumnValues(this DataTable sourceTable, string forField)
    {
        return sourceTable.Rows
            .OfType<DataRow>()
            .Select((r, i) => (Index: i, Value: r[forField]))
            .OrderBy(x => x.Index)
            .Select(x => x.Value)
            .ToArray();
    }

    /// <summary>
    /// Applies a transformation rule to a Data Table.
    /// </summary>
    /// <typeparam name="TTargetType">The type of object to transform the data into.</typeparam>
    /// <param name="table">The table to apply the transformation to.</param>
    /// <param name="fieldMapping">The field mapping to use for the transformation.</param>
    /// <param name="selectedRecords">The records to select from the table.</param>
    /// <returns>A list of objects of the specified type.</returns>
    /// <remarks>
    /// If the <paramref name="selectedRecords"/> parameter is not provided, all records in the table will be selected.
    /// </remarks>
    public static async Task<List<TTargetType>> ApplyTransformation<TTargetType>(this DataTable table, List<FieldMapping> fieldMapping, List<int>? selectedRecords = null) where TTargetType : new()
        => (await table.ApplyTransformation(fieldMapping, selectedRecords)).ToObject<TTargetType>();

    /// <summary>
    /// Applies a transformation rule to a Data Table.
    /// </summary>
    /// <param name="table">The table to apply the transformation to.</param>
    /// <param name="fieldMapping">The field mapping to use for the transformation.</param>
    /// <param name="selectedRecords">The records to select from the table.</param>
    /// <returns>A new DataTable with the transformation applied.</returns>
    /// <remarks>
    /// If the <paramref name="selectedRecords"/> parameter is not provided, all records in the table will be selected.
    /// </remarks>
    public static async Task<DataTable> ApplyTransformation(this DataTable table, List<FieldMapping> fieldMapping, List<int>? selectedRecords = null)
    {
        var destTable = new DataTable();

        // Only add columns that are in the field mapping
        foreach (var field in fieldMapping)
        {
            var typeToUse = Nullable.GetUnderlyingType(field.FieldType) ?? field.FieldType;
            var newCol = destTable.Columns.Add(field.FieldName, typeToUse);
            newCol.AllowDBNull = true;
        }

        // Add rows to the destination table
        foreach (var row in table.Rows.OfType<DataRow>().Where((_, i) => selectedRecords is null || selectedRecords.Contains(i)))
        {
            await row.TransformAndAdd(destTable, fieldMapping);
        }

        return destTable;
    }

    /// <summary>
    /// Transforms a source row and adds it to a destination data table as a new row.
    /// </summary>
    /// <param name="sourceRow">The source row to transform.</param>
    /// <param name="destDataTable">The destination data table to add the transformed row to.</param>
    /// <param name="fieldMappings">The field mapping to use for the transformation.</param>
    public static async Task TransformAndAdd(this DataRow sourceRow, DataTable destDataTable, List<FieldMapping> fieldMappings)
    {
        var newRow = destDataTable.NewRow();
        await sourceRow.ApplyTransformation(newRow, fieldMappings);
        destDataTable.Rows.Add(newRow);
    }

    /// <summary>
    /// Applies a transformation rule to a source row and adds the transformed values to a destination row.
    /// </summary>
    /// <param name="sourceRow">The source row to transform.</param>
    /// <param name="destRow">The destination row to add the transformed values to.</param>
    /// <param name="fieldMappings">The field mapping to use for the transformation.</param>
    public static async Task ApplyTransformation(this DataRow sourceRow, DataRow destRow, List<FieldMapping> fieldMappings)
    {
        var mappedFieldRules = fieldMappings.MappedFieldsOnly();

        // Check for missing fields in the destination table
        mappedFieldRules.CheckForMissingFields(sourceRow.Table, destRow.Table);

        foreach (var fieldMap in mappedFieldRules)
        {
            var transformedResult = await fieldMap.Apply(sourceRow);

            // Validate the current transformed result
            if (!ValidateFieldValue(fieldMap, transformedResult?.CurrentValue, out var validationResults))
            {
                destRow.SetColumnError(destRow.Table.Columns[fieldMap.FieldName]!, $"{string.Join($". {Environment.NewLine}", validationResults!.Select(x => x.ErrorMessage))}.".Replace("..", "."));
            }

            if (transformedResult?.CurrentValue is null || (string.IsNullOrWhiteSpace(transformedResult.CurrentValue.ToString()) && destRow.Table.Columns[fieldMap.FieldName]!.DataType != typeof(string)))
            {
                destRow[fieldMap.FieldName] = DBNull.Value;
            }
            else
            {
                destRow[fieldMap.FieldName] = transformedResult.CurrentValue;
            }
        }
    }

    private static bool ValidateFieldValue(FieldMapping fieldMap, object? valueToValidate, out List<ValidationResult> validationResults)
    {
        validationResults = new List<ValidationResult>();
        if (fieldMap.ValidationAttributes.IsEmpty)
        {
            return true;
        }

        var validationContext = new ValidationContext(valueToValidate ?? new object(), serviceProvider: null, items: null)
        {
            DisplayName = fieldMap.FieldName,
            MemberName = fieldMap.FieldName // Setting MemberName for context
        };

        bool isValid = true;
        foreach (var attribute in fieldMap.ValidationAttributes)
        {
            var result = attribute.GetValidationResult(valueToValidate, validationContext);
            if (result != ValidationResult.Success && result != null)
            {
                isValid = false;
                validationResults.Add(result);
            }
        }
        return isValid;
    }

    // Adapted from https://stackoverflow.com/questions/19673502/how-to-convert-datarow-to-an-object/47141701#47141701
    /// <summary>
    /// Converts a DataTable to a list of objects of the specified type.
    /// </summary>
    /// <typeparam name="TTargetType">The type of object to convert the data into.</typeparam>
    /// <param name="table">The table to convert to objects.</param>
    /// <param name="selectedRecords">The records to select from the table.</param>
    /// <returns>A list of objects of the specified type.</returns>
    /// <remarks>
    /// If the <paramref name="selectedRecords"/> parameter is not provided, all records in the table will be selected.
    /// </remarks>
    public static List<TTargetType> ToObject<TTargetType>(this DataTable table, List<int>? selectedRecords = null) where TTargetType : new()
        => table.Rows
            .OfType<DataRow>()
            .Where((x, i) => selectedRecords is null || selectedRecords.Contains(i))
            .Select(x => x.ToObject<TTargetType>())
            .ToList();

    /// <summary>
    /// Converts a DataRow to an object of the specified type.
    /// </summary>
    /// <typeparam name="TTargetType">The type of object to convert the data into.</typeparam>
    /// <param name="row">The row to convert to an object.</param>
    /// <returns>An object of the specified type.</returns>
    public static TTargetType ToObject<TTargetType>(this DataRow row)
        where TTargetType : new()
    {
        var output = new TTargetType();

        foreach (DataColumn column in row.Table.Columns)
        {
            var property = typeof(TTargetType).GetProperty(column.ColumnName);

            if (property is null) { continue; }

            if (row[column.ColumnName] == DBNull.Value)
            {
                // We need to allow it to set the value to null for a nullable primitive as well.  This only sets the value to the default value for value types
                property.SetValue(output, null);
            }
            else
            {
                property.SetValue(output, row[column.ColumnName]);
            }
        }

        return output;
    }

    /// <summary>
    /// Filters a list of field mappings to get only those have output targets.
    /// </summary>
    /// <param name="fieldMapping">The field mapping to filter.</param>
    /// <returns>A list of field mappings that have output targets.</returns>
    /// <remarks>
    /// The only fields that are included are those that have at least one output 
    /// target with a matching field name. The list of output targets is also 
    /// filtered to remove any that do not have a matching field name.
    /// </remarks>
    private static List<FieldMapping> MappedFieldsOnly(this List<FieldMapping> fieldMapping)
        => fieldMapping
            .Where(x => x.MappingRuleType != MappingRuleType.IgnoreRule)
            .ToList();

    /// <summary>
    /// Checks for missing fields in the data tables used in field mappings.
    /// </summary>
    /// <param name="fieldMappings">The field mappings to use for the check.</param>
    /// <param name="sourceDataTable">The source data table to check.</param>
    /// <param name="destDataTable">The destination data table to check.</param>
    /// <exception cref="AutomatedRealms.DataImportUtility.Core.CustomExceptions.MissingFieldMappingException">
    /// Thrown when there are missing fields used in the Field Mappings that don't exist for the given data tables.
    /// </exception>
    /// <remarks>
    /// This method checks the source data table for <see cref="MappingRuleBase.SourceField"/> and the destination data table for <see cref="FieldMapping.FieldName"/>.
    /// It currently does not deeply inspect individual transformations in <see cref="MappingRuleBase.SourceFieldTransformations"/> for other field dependencies.
    /// </remarks>
    /// <exception cref="AutomatedRealms.DataImportUtility.Core.CustomExceptions.MissingFieldMappingException">
    /// Thrown when there are missing fields in only one of the two provided tables.
    /// </exception>
    /// <exception cref="AggregateException">
    /// Thrown when there are missing fields in both of the provided tables.
    /// </exception>
    private static void CheckForMissingFields(this List<FieldMapping> fieldMappings, DataTable sourceDataTable, DataTable destDataTable)
    {
        var missingSourceFields = fieldMappings
            .Where(fm => fm.MappingRule != null &&
                         !fm.IgnoreMapping &&
                         !string.IsNullOrEmpty(fm.MappingRule.SourceField) && 
                         !sourceDataTable.Columns.Contains(fm.MappingRule.SourceField!)) 
            .Select(fm => fm.MappingRule!.SourceField!) 
            .Distinct()
            .ToList();

        var missingTargetFields = fieldMappings
            .Where(x => x.MappingRule is not null && !x.IgnoreMapping && !string.IsNullOrEmpty(x.FieldName) && !destDataTable.Columns.Contains(x.FieldName))
            .Select(x => x.FieldName)
            .Distinct()
            .ToList();

        var missingFieldExceptions = new List<System.Exception>();        if (missingSourceFields.Any())
        {
            var missingFields = fieldMappings
                .Where(fm => fm.MappingRule != null &&
                            !fm.IgnoreMapping &&
                            !string.IsNullOrEmpty(fm.MappingRule.SourceField) &&
                            !sourceDataTable.Columns.Contains(fm.MappingRule.SourceField!))
                .ToList();
                
            missingFieldExceptions.Add(new AutomatedRealms.DataImportUtility.Core.CustomExceptions.MissingFieldMappingException(
                missingFields, 
                $"The source table does not contain the following mapped source fields: {string.Join(", ", missingSourceFields)}")
            );
        }

        if (missingTargetFields.Any())
        {
            missingFieldExceptions.Add(new AutomatedRealms.DataImportUtility.Core.CustomExceptions.MissingFieldMappingException(System.Array.Empty<AutomatedRealms.DataImportUtility.Abstractions.Models.FieldMapping>(), $"The destination table does not contain the following mapped target fields: {string.Join(", ", missingTargetFields)}"));
        }

        if (missingFieldExceptions.Count == 1)
        {
            throw missingFieldExceptions.First();
        }
        else if (missingFieldExceptions.Count > 1)
        {
            throw new AggregateException("Multiple missing field errors occurred.", missingFieldExceptions);
        }
    }
}
