using AutomatedRealms.DataImportUtility.Abstractions.Models;
using System.Collections;
using System.Text.Json;

namespace AutomatedRealms.DataImportUtility.Abstractions.Helpers;

/// <summary>
/// The helper methods for the <see cref="TransformationResult" />.
/// </summary>
public static class TransformationResultHelpers
{
    /// <summary>
    /// Checks if the current value for the <see cref="TransformationResult" /> is null.
    /// </summary>
    public static bool IsNullOrNullValue(this TransformationResult? result)
    {
        return result is null || result.CurrentValue is null;
    }

    /// <summary>
    /// Checks if the current value for the <see cref="TransformationResult" /> is a JsonArray.
    /// </summary>
    public static bool IsJsonArray(this TransformationResult? result)
    {
        if (result.IsNullOrNullValue() || result!.CurrentValue is null || !result.CurrentValue.ToString().Trim().StartsWith("[")) { return false; }
        try
        {
            using var jsonDoc = JsonDocument.Parse(result.CurrentValue.ToString());
            return jsonDoc.RootElement.ValueKind == JsonValueKind.Array;
        }
        catch (JsonException)
        {
            Console.WriteLine("The value is not a valid JSON array.");
            return false;
        }
    }

    /// <summary>
    /// Generates a display for the <see cref="TransformationResult" />'s <see cref="TransformationResult.OriginalValue" />.
    /// </summary>
    /// <param name="result">
    /// The <see cref="TransformationResult" /> to generate the display for.
    /// </param>
    /// <returns>
    /// The display for the <see cref="TransformationResult" />'s <see cref="TransformationResult.OriginalValue" />.
    /// </returns>
    public static string GetOriginalValueDisplay(this TransformationResult? result)
    {
        return result?.OriginalValue is null
            ? "<null>"
            : string.IsNullOrWhiteSpace(result.OriginalValue.ToString())
                ? "<blank>"
                : result.OriginalValue.ToString()!;
    }    
    
    /// <summary>
    /// Generates a display for the <see cref="TransformationResult" />'s <see cref="TransformationResult.CurrentValue" />.
    /// </summary>
    /// <param name="result">
    /// The <see cref="TransformationResult" /> to generate the display for.
    /// </param>
    /// <returns>
    /// The display for the <see cref="TransformationResult" />'s <see cref="TransformationResult.CurrentValue" />.
    /// </returns>
    public static string GetCurrentValueDisplay(this TransformationResult? result)
    {
        return result?.WasFailure ?? false
            ? "#ERROR"
            : result?.CurrentValue is null
                ? "<null>"
                : string.IsNullOrWhiteSpace(result.CurrentValue.ToString())
                    ? "<blank>"
                    : result.CurrentValue.ToString()!;
    }

    /// <summary>
    /// Checks if the value for the incoming result is a collection and returns a 
    /// new result with an error message if it is.
    /// </summary>
    /// <param name="result">The result to check.</param>
    /// <param name="errorMessage">The error message to return if the value is a collection.</param>
    /// <returns>
    /// The result with an error message if the value is a collection.
    /// </returns>
    public static TransformationResult ErrorIfCollection(this TransformationResult result, string errorMessage)
    {
        if (result.CurrentValue is null)
        {
            return result;
        }

        // Check if it's a direct collection type (except string which is a char collection but treated as scalar)
        if (result.CurrentValue is IEnumerable && !(result.CurrentValue is string))
        {
            return TransformationResult.Failure(
                originalValue: result.OriginalValue,
                targetType: result.TargetFieldType ?? typeof(object),
                errorMessage: errorMessage,
                originalValueType: result.OriginalValueType,
                currentValueType: result.CurrentValueType,
                appliedTransformations: result.AppliedTransformations,
                record: result.Record,
                tableDefinition: result.TableDefinition,
                sourceRecordContext: result.SourceRecordContext,
                explicitTargetFieldType: result.TargetFieldType
            );
        }

        // Check if it's a JSON array
        var valueStr = result.CurrentValue.ToString();
        if (!string.IsNullOrEmpty(valueStr) && valueStr.Trim().StartsWith("["))
        {
            try
            {
                using var jsonDoc = JsonDocument.Parse(valueStr);
                if (jsonDoc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    return TransformationResult.Failure(
                        originalValue: result.OriginalValue,
                        targetType: result.TargetFieldType ?? typeof(object),
                        errorMessage: errorMessage,
                        originalValueType: result.OriginalValueType,
                        currentValueType: result.CurrentValueType,
                        appliedTransformations: result.AppliedTransformations,
                        record: result.Record,
                        tableDefinition: result.TableDefinition,
                        sourceRecordContext: result.SourceRecordContext,
                        explicitTargetFieldType: result.TargetFieldType
                    );
                }
            }
            catch (JsonException)
            {
#if DEBUG
                Console.WriteLine("The value is not a valid JSON array.");
#endif
            }
        }

        return result;
    }

    /// <summary>
    /// Gets the result value as an array.
    /// </summary>
    /// <param name="result">The result to get the value from.</param>
    /// <returns>The <see cref="TransformationResult.CurrentValue"/> as an array of strings.</returns>
    public static string?[]? ResultValueAsArray(this TransformationResult result)
    {
        return result.IsJsonArray()
            ? JsonSerializer.Deserialize<string?[]>(result.CurrentValue!.ToString())
            : [result.CurrentValue?.ToString()];
    }
}
