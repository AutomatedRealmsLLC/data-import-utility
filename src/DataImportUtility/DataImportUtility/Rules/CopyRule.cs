using System.Text.Json.Serialization;

using DataImportUtility.Abstractions;
using DataImportUtility.Helpers;
using DataImportUtility.Models;

namespace DataImportUtility.Rules;

/// <summary>
/// A rule indicating the value should be copied.
/// </summary>
public class CopyRule : MappingRuleBase
{
    /// <inheritdoc />
    public override int EnumMemberOrder { get; } = 1;

    /// <inheritdoc />
    public override string EnumMemberName { get; } = nameof(CopyRule);

    /// <inheritdoc />
    [JsonIgnore]
    public override string DisplayName { get; } = "Copy Value";

    /// <inheritdoc />
    [JsonIgnore]
    public override string ShortName => "Copy";

    /// <inheritdoc />
    [JsonIgnore]
    public override string Description { get; } = "Copy the value of the source field into the output field.";

    /// <inheritdoc />
    protected override ushort MaxSourceFields { get; } = 1;

    /// <summary>
    /// The field to copy the value from and value transformation(s) to apply to the field.
    /// </summary>
    [JsonIgnore]
    public FieldTransformation? FieldTransformation
    {
        get => SourceFieldTranformations.Count > 0 ? SourceFieldTranformations[0] : null;
        set
        {
            if (value is null)
            {
                ClearFieldTransformations();
            }
            else if (SourceFieldTranformations.Count == 0)
            {
                AddFieldTransformation(value);
            }
            else
            {
                ReplaceFieldTransformation(0, value);
            }
        }
    }

    /// <inheritdoc />
    [JsonIgnore]
    public override bool IsEmpty => string.IsNullOrWhiteSpace(FieldTransformation?.Field?.FieldName);

    /// <inheritdoc />
    public override async Task<IEnumerable<TransformationResult>> Apply()
    {
        var sourceFieldResults = FieldTransformation is not null ? await FieldTransformation.Apply() : [];
        return await Task.WhenAll(sourceFieldResults.Select(result => Apply(result)));
    }

    /// <inheritdoc />
    public override Task<TransformationResult> Apply(TransformationResult result)
        => Task.FromResult(result.CopyField());
}

/// <summary>
/// Extension methods for the <see cref="CopyRule" /> class.
/// </summary>
public static class CopyRuleExtensions
{
    /// <summary>
    /// Copies the field.
    /// </summary>
    /// <param name="result">The result of the transformations applied in order to each of the fields involved in the mapping.</param>
    /// <returns>
    /// The result of the transformation.
    /// </returns>
    public static TransformationResult CopyField(this TransformationResult result)
        => result.ErrorIfCollection(ValueTransformationBase.OperationInvalidForCollectionsMessage);
}