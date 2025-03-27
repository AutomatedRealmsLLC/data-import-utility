using DataImportUtility.Abstractions;
using DataImportUtility.Models;

namespace DataImportUtility.Rules;

/// <summary>
/// A custom rule that does not require a field to be selected.
/// </summary>
public partial class CustomFieldlessRule : MappingRuleBase
{
    /// <inheritdoc />
    public override string EnumMemberName => nameof(CustomFieldlessRule);
    /// <inheritdoc />
    public override string DisplayName => "Custom...";
    /// <inheritdoc />
    public override string ShortName => "Custom...";
    /// <inheritdoc />
    public override string Description => "A custom rule that does not require a field to be selected.";

    /// <inheritdoc />
    public override bool IsEmpty => !SourceFieldTransformations.Any(x => x.ValueTransformations.Count > 0);

    /// <inheritdoc />
    public override Task<IEnumerable<TransformationResult>> Apply()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override Task<TransformationResult> Apply(TransformationResult result)
    {
        throw new NotImplementedException();
    }
}