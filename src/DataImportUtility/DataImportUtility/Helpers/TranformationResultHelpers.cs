using System.Text.Json;

using DataImportUtility.Models;

namespace DataImportUtility.Helpers;

/// <summary>
/// The helper methods for the <see cref="TransformationResult" />.
/// </summary>
public static class TranformationResultHelpers
{
    /// <summary>
    /// Checks if the current value for the <see cref="TransformationResult" /> is null.
    /// </summary>
    public static bool IsNullOrNullValue(this TransformationResult? result)
    {
        return result is null || result.Value is null;
    }

    /// <summary>
    /// Checks if the current value for the <see cref="TransformationResult" /> is a JsonArray.
    /// </summary>
    public static bool IsJsonArray(this TransformationResult? result)
    {
        if (result.IsNullOrNullValue() || !result!.Value!.Trim().StartsWith("[")) { return false; }
        try
        {
            using var jsonDoc = JsonDocument.Parse(result.Value);
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
    /// Generates a display for the <see cref="TransformationResult" />'s <see cref="TransformationResult.Value" />.
    /// </summary>
    /// <param name="result">
    /// The <see cref="TransformationResult" /> to generate the display for.
    /// </param>
    /// <returns>
    /// The display for the <see cref="TransformationResult" />'s <see cref="TransformationResult.Value" />.
    /// </returns>
    public static string GetCurrentValueDisplay(this TransformationResult? result)
    {
        return (result?.WasFailure ?? false)
            ? "#ERROR"
            : result?.Value is null
                ? "<null>"
                : string.IsNullOrWhiteSpace(result.Value.ToString())
                    ? "<blank>"
                    : result.Value.ToString()!;
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
        if (result.Value is null || !result.Value.Trim().StartsWith("["))
        {
            return result;
        }

        try
        {
            using var jsonDoc = JsonDocument.Parse(result.Value);
            if (jsonDoc.RootElement.ValueKind == JsonValueKind.Array)
            {
                return result with { ErrorMessage = errorMessage };
            }
        }
        catch (JsonException)
        {
#if DEBUG
            Console.WriteLine("The value is not a valid JSON array.");
#endif
        }

        return result;
    }

    /// <summary>
    /// Gets the result value as an array.
    /// </summary>
    /// <param name="result">The result to get the value from.</param>
    /// <returns>The <see cref="TransformationResult.Value"/> as an array of strings.</returns>
    public static string?[]? ResultValueAsArray(this TransformationResult result)
    {
        return result.IsJsonArray()
            ? JsonSerializer.Deserialize<string?[]>(result.Value!)
            : [result.Value];
    }
}
