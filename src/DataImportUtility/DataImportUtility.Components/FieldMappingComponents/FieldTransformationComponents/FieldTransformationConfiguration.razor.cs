using DataImportUtility.Abstractions;
using DataImportUtility.Components.Abstractions;
using DataImportUtility.Models;

using Microsoft.AspNetCore.Components;

namespace DataImportUtility.Components.FieldMappingComponents.FieldTransformationComponents;

/// <summary>
/// Displays the configuration for a <see cref="FieldTransformation" />.
/// </summary>
public partial class FieldTransformationConfiguration : FileImportUtilityComponentBase
{
    private const ValueTransformationType _defaultTransformationType = ValueTransformationType.InterpolateTransformation;

    /// <summary>
    /// Whether to show the dialog.
    /// </summary>
    [Parameter] public bool Show { get; set; }
    /// <summary>
    /// The callback for when the show property is changed.
    /// </summary>
    [Parameter] public EventCallback<bool> ShowChanged { get; set; }
    /// <summary>
    /// The field mapping the transformation is for.
    /// </summary>
    [Parameter, EditorRequired] public FieldMapping FieldMapping { get; set; } = default!;
    /// <summary>
    /// The field transformation to display the configuration for.
    /// </summary>
    [Parameter, EditorRequired] public FieldTransformation FieldTransformation { get; set; } = default!;
    /// <summary>
    /// The callback for when the output target is changed.
    /// </summary>
    [Parameter] public EventCallback<FieldTransformation> FieldTransformationChanged { get; set; }
    /// <summary>
    /// The imported data file.
    /// </summary>
    [Parameter] public ImportedDataFile? ImportedDataFile { get; set; }

    private object[] DistinctValues => Field?.ValueSet?.Distinct().ToArray() ?? Array.Empty<object>();
    private bool HasValues => DistinctValues.Length > 0;
    private int _previewIndex = 0;

    private ImportedRecordFieldDescriptor? Field => FieldTransformation.Field;
    private List<ValueTransformationBase> _valueTransformations = [];
    private bool _previewsExpanded;

    /// <inheritdoc />
    protected override Task OnInitializedAsync()
    {
        if (FieldTransformation.ValueTransformations.Count > 0)
        {
            _valueTransformations.AddRange(FieldTransformation.ValueTransformations);
        }
        else
        {
            _valueTransformations.Add(_defaultTransformationType.CreateNewInstance()!);
            FieldTransformation.AddTransformation(_valueTransformations.First());
        }

        return HandleValueTransformationChanged();
    }

    private Task UpdatePreviewIndex(int index)
    {
        _previewIndex = index;
        return HandleValueTransformationChanged();
    }

    private Task AddTransformation()
    {
        _valueTransformations.Add(_defaultTransformationType.CreateNewInstance()!);

        return HandleValueTransformationChanged();
    }

    private bool _removingRec;
    private async Task RemoveTransformation(ValueTransformationBase transformation)
    {
        _removingRec = true;
        await Task.Delay(20);
        _valueTransformations.Remove(transformation);
        _removingRec = false;
        await HandleValueTransformationChanged();
    }

    private async Task HandleValueTransformationChanged()
    {
        FieldTransformation.SetTransformations(_valueTransformations);
        await RepopulateTransformationResults();

        if (FieldTransformationChanged.HasDelegate)
        {
            await FieldTransformationChanged.InvokeAsync(FieldTransformation);
        }
    }

    private Task HandleTransformationRuleUpdated()
    {
        _valueTransformations = [.. FieldTransformation.ValueTransformations];
        return RepopulateTransformationResults();
    }

    private async Task<TransformationResult> ApplyTransformationsThrough(int? throughTransformIndex = null, int? valueIndex = null, bool resetOriginal = true)
    {
        // Apply all transformations if no index is specified
        throughTransformIndex ??= _valueTransformations.Count - 1;
        valueIndex ??= _previewIndex;

        if (throughTransformIndex.Value >= 0 && DistinctValues.Length > valueIndex && _transformationResults.TryGetValue((DistinctValues[valueIndex.Value]?.ToString() ?? "<null>"), out var results) && results.Count > throughTransformIndex.Value)
        {
            var forRet = results[throughTransformIndex.Value];
            forRet = forRet with { OriginalValue = resetOriginal ? forRet.Value : forRet.OriginalValue, Value = forRet.Value };

            return forRet;
        }

        var valueTransform = GetInitialValueTransform(valueIndex);
        for (var i = 0; i <= throughTransformIndex; i++)
        {
            valueTransform = await _valueTransformations[i].Apply(valueTransform);
        }

        return valueTransform with { OriginalValue = resetOriginal ? valueTransform.Value : valueTransform.OriginalValue, Value = valueTransform.Value };
    }

    private TransformationResult GetInitialValueTransform(int? valueIndex = null)
    {
        valueIndex ??= _previewIndex;

        var retVal = HasValues
            ? DistinctValues
                .Skip(Math.Max(0, Math.Min(valueIndex.Value, DistinctValues.Length - 1)))
                .First()
            : default;

        return new()
        {
            OriginalValue = retVal?.ToString(),
            Value = retVal?.ToString()
        };
    }

    private Task CloseDialog()
    {
        Show = false;
        return ShowChanged.HasDelegate
            ? ShowChanged.InvokeAsync(Show)
            : Task.CompletedTask;
    }

    #region Transformation Results (Cache)
    private readonly Dictionary<string, List<TransformationResult>> _transformationResults = [];
    private async Task RepopulateTransformationResults()
    {
        foreach (var value in DistinctValues)
        {
            var useValue = value?.ToString() ?? "<null>";
            if (!_transformationResults.TryGetValue(useValue, out var transformResults))
            {
                transformResults = [];
                _transformationResults.Add(useValue, transformResults);
            }
            transformResults.Clear();

            var curTransform = new TransformationResult()
            {
                OriginalValue = value?.ToString(),
                Value = value?.ToString()
            };

            if (_valueTransformations.Count == 0)
            {
                transformResults.Add(curTransform);
                _transformationResults[useValue] = transformResults;
                continue;
            }

            for (var i = 0; i <= _valueTransformations.Count - 1; i++)
            {
                curTransform = await _valueTransformations[i].Apply(curTransform);
                transformResults.Add(curTransform);
            }
        }

        await FieldMapping.UpdateValidationResults();
    }
    #endregion Transformation Results
}
