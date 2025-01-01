using System.Text.Json.Serialization;

namespace DataImportUtility.Models;

/// <summary>
/// The result of a transformation operation.
/// </summary>
public partial record TransformationResult
{
    /// <summary>
    /// The original value (as a string) before transformations are applied.
    /// </summary>
    public string? OriginalValue { get; set; }
    /// <summary>
    /// The value of the transformation.
    /// </summary>
    /// <remarks>
    /// Note that all of the transformed values will be strings.
    /// </remarks>
    public string? Value { get; set; }
    /// <summary>
    /// The error _message if the transformation failed.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? ErrorMessage { get; set; }
    /// <summary>
    /// Whether the transformation was successful.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool WasFailure => !string.IsNullOrWhiteSpace(ErrorMessage);
}
