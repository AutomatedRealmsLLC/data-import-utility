using System.Text.Json.Serialization;

namespace DataImportUtility.Models;

/// <summary>
/// The result of a transformation operation.
/// </summary>
public partial record TransformationResult
{
    /// <summary>
    /// The name of the transformation.
    /// </summary>
    public Type OriginalValueType { get; set; } = typeof(object);

    /// <summary>
    /// The original value (as a string) before transformations are applied.
    /// </summary>
    public string? OriginalValue { get; set; }

    /// <summary>
    /// The type of the current value.
    /// </summary>
    public Type CurrentValueType { get; set; } = typeof(object);

    /// <summary>
    /// The value of the transformation.
    /// </summary>
    /// <remarks>
    /// Note that all of the transformed values will be strings.
    /// </remarks>
    public string? Value { get; set; }

    /// <summary>
    /// The error message if the transformation failed.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Whether the transformation was successful.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool WasFailure => !string.IsNullOrWhiteSpace(ErrorMessage);
}
