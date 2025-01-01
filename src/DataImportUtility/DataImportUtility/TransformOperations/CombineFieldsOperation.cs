using System.Text.Json;
using System.Text.Json.Serialization;

using DataImportUtility.Abstractions;
using DataImportUtility.Helpers;
using DataImportUtility.Models;

namespace DataImportUtility.TransformOperations;

/// <summary>
/// Combines the transformed values of multiple fields into a single value.
/// </summary>
public partial class CombineFieldsOperation : ValueTransformationBase
{
    /// <inheritdoc />
    public override string EnumMemberName { get; } = nameof(CombineFieldsOperation);

    /// <summary>
    /// The error _message when the number of values to combine does not match the number of field mappings.
    /// </summary>
    public const string ValueTransformationCountMismatchmessage = "The number of values to combine does not match the number of field mappings.";

    /// <inheritdoc />
    [JsonIgnore]
    public override string DisplayName { get; } = "Combine Fields";

    /// <inheritdoc />
    [JsonIgnore]
    public override string ShortName => "Combine";

    /// <inheritdoc />
    [JsonIgnore]
    public override string Description { get; } = "Combines the transformed values of multiple fields into a single value.";

    /// <summary>
    /// The field transformations to use to combine the values.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public List<FieldTransformation> SourceFieldTransforms { get; set; } = [];

    /// <inheritdoc />
    public override Task<TransformationResult> Apply(TransformationResult result)
    {
        try
        {
            return Task.FromResult(result.CombineFields(OperationDetail));
        }
        catch (Exception ex)
        {
            return Task.FromResult(result with { ErrorMessage = ex.Message });
        }
    }
}

/// <summary>
/// The extension methods for the <see cref="CombineFieldsOperation" /> to be used with the scripting engine.
/// </summary>
public static class CombineFieldsOperationExtensions
{
    /// <summary>
    /// Combines the values of multiple fields for the entire data set
    /// </summary>
    /// <param name="fieldTransforms">The field mappings to use to combine the values.</param>
    /// <param name="operationDetail">The additional information for the operation.</param>
    /// <returns>
    /// The result of the transformation for each value in the <see cref="ImportedRecordFieldDescriptor.ForTable"/>.
    /// </returns>
    /// <remarks>
    /// The operationDetail parameter is a string that contains the pattern to use to combine the values of the fields.<br />
    /// <br />
    /// The pattern should use the syntax `${0}`, `${1}`, etc. to indicate the values to interpolate.
    /// </remarks>
    public static async Task<IEnumerable<TransformationResult>> CombineFields(this IEnumerable<FieldTransformation> fieldTransforms, string? operationDetail)
    {
        var hasPlaceholders = operationDetail.TryGetPlaceholderMatches(out var matches);
        var fieldTransformsToUse = fieldTransforms.Where(x => x.Field?.ForTable is not null).ToList();

        var dataTable = fieldTransformsToUse.FirstOrDefault()?.Field?.ForTable;

        if (fieldTransformsToUse.Count == 0 || dataTable is null || dataTable.Rows.Count == 0)
        {
            return [];
        }

        if (!hasPlaceholders)
        {
            // If there are no placeholders, just return the operation detail as the transformed value for each row
            return Enumerable
                .Range(0, dataTable.Rows.Count)
                .Select(i => new TransformationResult()
                {
                    OriginalValue = JsonSerializer.Serialize(fieldTransforms.Select(x => x.Field!.ValueSet[i].ToString())),
                    Value = operationDetail
                });

        }

        // Otherwise, apply each of the field transformations to the each row in the data table, then combine the results
        var results = new TransformationResult[dataTable.Rows.Count];

        for (var i = 0; i < dataTable.Rows.Count; i++)
        {
            var curRow = dataTable.Rows[i];

            // Get the results for each transformation for the current row
            var rowResults = await Task.WhenAll(fieldTransformsToUse.Select(x => x.Apply(curRow)));

            // Combine the original values and the transformed values for the current row into JSON arrays
            results[i] = new TransformationResult()
            {
                OriginalValue = JsonSerializer.Serialize(rowResults.Select(x => x.OriginalValue)),
                Value = JsonSerializer.Serialize(rowResults.Select(x => x.Value))
            };

            // Apply the combine operation to the transformed values
            results[i] = results[i].CombineFields(operationDetail);
        }

        return results;
    }

    /// <summary>
    /// Combines the values of multiple fields.
    /// </summary>
    /// <param name="result">The result of the transformations applied in order to each of the fields involved in the mapping.</param>
    /// <param name="operationDetail">The additional information for the operation.</param>
    /// <returns>
    /// The result of the transformation.
    /// </returns>
    /// <remarks>
    /// The operationDetail parameter is a string that contains the pattern to use to combine the values of the fields.<br />
    /// <br />
    /// The pattern should use the syntax `${0}`, `${1}`, etc. to indicate the values to interpolate.
    /// </remarks>
    public static TransformationResult CombineFields(this TransformationResult result, string? operationDetail)
    {
        // Apply the interpolation to the values in the result
        var resultValue = operationDetail;

        if (string.IsNullOrWhiteSpace(resultValue) || result.IsNullOrNullValue())
        {
            // No transformation to apply, so just return the input interpolation pattern
            return result with { Value = resultValue };
        }

        var valuesToUse = result.IsJsonArray()
            ? JsonSerializer.Deserialize<string?[]>(result.Value!) ?? []
            : [result.Value];

        // Apply the interpolation
        // Since we aren't using the normal format string syntax (e.g. {0}), we need to do this manually
        // We are using the syntax ${0} to indicate the value to interpolate
        for (var i = 0; i < valuesToUse.Length; i++)
        {
            resultValue = resultValue.Replace($"${{{i}}}", valuesToUse[i] ?? string.Empty);
        }
        return result with { Value = resultValue };
    }
}