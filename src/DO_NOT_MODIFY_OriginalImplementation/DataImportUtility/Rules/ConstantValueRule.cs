using System.Data;
using System.Text.Json.Serialization;

using DataImportUtility.Abstractions;
using DataImportUtility.Models;

namespace DataImportUtility.Rules;

/// <summary>
/// A rule indicating the value should be constant.
/// </summary>
public partial class ConstantValueRule : MappingRuleBase
{
    /// <inheritdoc />
    public override int EnumMemberOrder { get; } = 2;

    /// <inheritdoc />
    public override string EnumMemberName { get; } = nameof(ConstantValueRule);

    /// <inheritdoc />
    [JsonIgnore]
    public override string DisplayName { get; } = "Constant Value";

    /// <inheritdoc />
    [JsonIgnore]
    public override string ShortName => "Constant";

    /// <inheritdoc />
    [JsonIgnore]
    public override string Description { get; } = "Outputs a constant value for each record.";

    /// <inheritdoc />
    protected override ushort MaxSourceFields { get; } = 1;

    /// <inheritdoc />
    [JsonIgnore]
    public override bool IsEmpty { get; }

    /// <inheritdoc />
    public override Task<IEnumerable<TransformationResult>> Apply()
        => Task.FromResult(new[] { new TransformationResult() { Value = RuleDetail } }.AsEnumerable());

    /// <inheritdoc />
    public override Task<TransformationResult> Apply(TransformationResult result)
        => Task.FromResult(result with { Value = RuleDetail });

    /// <inheritdoc />
    public override async Task<TransformationResult?> Apply(DataRow dataRow)
    {
        var initialResult = (await base.Apply(dataRow));
        if (initialResult?.Value is null || initialResult.WasFailure) { return initialResult; }
        initialResult = initialResult with { Value = initialResult.OriginalValue };

        return initialResult with { Value = RuleDetail };
    }
}
