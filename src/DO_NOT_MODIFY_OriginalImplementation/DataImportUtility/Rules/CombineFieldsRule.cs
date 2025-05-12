using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;

using DataImportUtility.Abstractions;
using DataImportUtility.Models;
using DataImportUtility.ValueTransformations;

namespace DataImportUtility.Rules;

/// <summary>
/// A rule indicating the value should be combined.
/// </summary>
public class CombineFieldsRule : MappingRuleBase
{
    /// <inheritdoc />
    public override int EnumMemberOrder { get; } = 3;

    /// <inheritdoc />
    public override string EnumMemberName { get; } = nameof(CombineFieldsRule);

    /// <inheritdoc />
    public override string DisplayName { get; } = "Combine Fields";

    /// <inheritdoc />
    public override string ShortName => "Combine";

    /// <inheritdoc />
    [JsonIgnore]
    public override string Description { get; } = "Combine the values of the source fields into the output field.";

    /// <inheritdoc />
    [JsonIgnore]
    public override bool IsEmpty => !SourceFieldTransformations.Any(x => !string.IsNullOrWhiteSpace(x.Field?.FieldName));

    /// <inheritdoc />
    public override async Task<IEnumerable<TransformationResult>> Apply()
    {
        var sourceFieldResults = new List<(int SourceIndex, int ResultIndex, TransformationResult Result)>();
        for (var sourceIndex = 0; sourceIndex < SourceFieldTransformations.Count; sourceIndex++)
        {
            var field = SourceFieldTransformations[sourceIndex];
            var currentSourceFieldTransforms = await field.Apply();

            sourceFieldResults.AddRange(currentSourceFieldTransforms
                .Select((result, resultIndex) => (sourceIndex, resultIndex, result)));
        }

        var resultTransforms = new List<TransformationResult>();
        foreach (var result in sourceFieldResults.GroupBy(x => x.ResultIndex))
        {
            var resultValueOutput = JsonSerializer.Serialize(result.OrderBy(x => x.SourceIndex).Select(x => $"{x.Result.Value}").ToArray());
            var currentResult = new TransformationResult()
            {
                OriginalValue = resultValueOutput,
                Value = resultValueOutput
            };
            currentResult = await Apply(currentResult);
            resultTransforms.Add(currentResult);
        }

        return resultTransforms;
    }

    /// <inheritdoc />
    public override Task<TransformationResult> Apply(TransformationResult result)
        => Task.FromResult(result.CombineFields(RuleDetail));

    /// <inheritdoc />
    public override async Task<TransformationResult?> Apply(DataRow dataRow)
    {
        var initialResult = (await base.Apply(dataRow));
        if (initialResult?.Value is null || initialResult.WasFailure) { return initialResult; }
        initialResult = initialResult with { Value = initialResult.OriginalValue };

        return initialResult.CombineFields(RuleDetail);
    }
}
