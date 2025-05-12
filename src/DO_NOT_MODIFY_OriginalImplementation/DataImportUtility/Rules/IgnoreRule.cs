using System.Text.Json.Serialization;

using DataImportUtility.Abstractions;
using DataImportUtility.Models;

namespace DataImportUtility.Rules;

/// <summary>
/// A rule indicating the field should be ignored.
/// </summary>
public class IgnoreRule : MappingRuleBase
{
    /// <inheritdoc />
    public override int EnumMemberOrder { get; } = 0;

    /// <inheritdoc />
    public override string EnumMemberName { get; } = nameof(IgnoreRule);

    /// <inheritdoc />
    [JsonIgnore]
    public override string DisplayName { get; } = "Ignore";

    /// <inheritdoc />
    [JsonIgnore]
    public override string ShortName => "Ignore";

    /// <inheritdoc />
    [JsonIgnore]
    public override string Description { get; } = "Do not output this field to the destination.";

    /// <inheritdoc />
    [JsonIgnore]
    public override bool IsEmpty { get; } = false;

    /// <inheritdoc />
    public override Task<IEnumerable<TransformationResult>> Apply()
        => Task.FromResult(Enumerable.Empty<TransformationResult>());

    /// <inheritdoc />
    public override Task<TransformationResult> Apply(TransformationResult result)
        => Task.FromResult(result.IgnoreField());
}

/// <summary>
/// Extension methods for the <see cref="IgnoreRule" /> class.
/// </summary>
public static class IgnoreRuleExtensions
{
    /// <summary>
    /// Ignores the field.
    /// </summary>
    /// <param name="result">The result of the transformations applied in order to each of the fields involved in the mapping.</param>
    /// <returns>
    /// The result of the transformation. The <see cref="TransformationResult.Value" /> will always be null.
    /// </returns>
    public static TransformationResult IgnoreField(this TransformationResult result)
        => result with { Value = null };
}
