using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using DataImportUtility.Abstractions;
using DataImportUtility.Helpers;

namespace DataImportUtility.Models;

/// <summary>
/// Represents a field to obtain values from and the transformations to apply to the values.
/// </summary>
public class FieldTransformation : IDisposable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FieldTransformation" /> class.
    /// </summary>
    /// <param name="field">
    /// The field to transform the value of.
    /// </param>
    public FieldTransformation(ImportedRecordFieldDescriptor? field = null)
    {
        if (field is not null)
        {
            Field = field;
            TransformationResults = (field.ValueSet?
                .Select(value => new TransformationResult()
                {
                    OriginalValue = value?.ToString(),
                    Value = value?.ToString()
                }) ?? Enumerable.Empty<TransformationResult>())
                .ToImmutableList();
        }
    }

    private CancellationTokenSource? _applyTransformationsTokenSource;

    /// <summary>
    /// The field to transform the value of.
    /// </summary>
    public ImportedRecordFieldDescriptor? Field { get; set; }

    /// <summary>
    /// Gets or sets the value transformations to apply to the field.
    /// </summary>
    /// <remarks>
    /// The order of the transformations is important. They will be applied in the order they are listed.
    /// </remarks>
    [JsonInclude]
    public ImmutableList<ValueTransformationBase> ValueTransformations
    {
        get => _valueTransformations;
        private set
        {
            if (_valueTransformations.Count > 0)
            {
                // Unregister from the OnDefinitionChanged event of the previous transformations collection
                _valueTransformations.ToList()
                    .ForEach(transformation => transformation.OnDefinitionChanged -= UpdateTransformationResults);
            }
            _valueTransformations = value;
            // Register to the OnDefinitionChanged event of the new transformations collection
            _valueTransformations.ToList()
                .ForEach(transformation => transformation.OnDefinitionChanged += UpdateTransformationResults);
            UpdateTransformationResults();
        }
    }
    private ImmutableList<ValueTransformationBase> _valueTransformations = ImmutableList.Create<ValueTransformationBase>();

    /// <summary>
    /// Gets or sets the results of the transformations.
    /// </summary>
    [JsonIgnore]
    public ImmutableList<TransformationResult> TransformationResults { get; private set; } = ImmutableList.Create<TransformationResult>();

    #region ValueTransformation Methods
    /// <summary>
    /// Adds a value transformation to the field.
    /// </summary>
    /// <param name="transformation">The transformation to add.</param>
    public void AddTransformation(ValueTransformationBase transformation)
    {
        var transformations = ValueTransformations.ToList();
        transformations.Add(transformation);
        ValueTransformations = transformations.ToImmutableList();
    }

    /// <summary>
    /// Removes a value transformation from the field.
    /// </summary>
    /// <param name="transformation">The transformation to remove.</param>
    public void RemoveTransformation(ValueTransformationBase transformation)
    {
        var transformations = ValueTransformations.ToList();
        transformations.Remove(transformation);
        ValueTransformations = transformations.ToImmutableList();
    }

    /// <summary>
    /// Removes a value transformation from the field.
    /// </summary>
    /// <param name="index">The index of the transformation to remove.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the index is out of range.
    /// </exception>
    public void RemoveTransformation(int index)
    {
        var transformations = ValueTransformations.ToList();
        transformations.RemoveAt(index);
        ValueTransformations = transformations.ToImmutableList();
    }

    /// <summary>
    /// Replaces a value transformation with a new transformation.
    /// </summary>
    /// <param name="oldTransformation">The old transformation to replace.</param>
    /// <param name="newTransformation">The new transformation to replace with.</param>
    public void ReplaceTransformation(ValueTransformationBase oldTransformation, ValueTransformationBase newTransformation)
    {
        var transformations = ValueTransformations.ToList();
        transformations[transformations.IndexOf(oldTransformation)] = newTransformation;
        ValueTransformations = transformations.ToImmutableList();
    }

    /// <summary>
    /// Replaces a value transformation with a new transformation.
    /// </summary>
    /// <param name="index">The index of the transformation to replace.</param>
    /// <param name="newTransformation">The new transformation to replace with.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the index is out of range.
    /// </exception>
    public void ReplaceTransformation(int index, ValueTransformationBase newTransformation)
    {
        var transformations = ValueTransformations.ToList();
        transformations[index] = newTransformation;
        ValueTransformations = transformations.ToImmutableList();
    }

    /// <summary>
    /// Replaces all existing value transformations with the provided transformations.
    /// </summary>
    /// <param name="transformations">
    /// The transformations to replace the existing transformations with.
    /// </param>
    public void SetTransformations(IEnumerable<ValueTransformationBase> transformations)
    {
        ValueTransformations = transformations.ToImmutableList();
    }
    #endregion ValueTransformation Methods

    #region Apply Methods
    /// <summary>
    /// Applies the value transformations (if any) to the field using the value set of the associated <see cref="Field"/>.
    /// </summary>
    /// <returns>
    /// The result of the transformation.
    /// </returns>
    public Task<IEnumerable<TransformationResult>> Apply(CancellationToken? ct = null)
        => Field?.ForTable is null
            ? Task.FromResult(new TransformationResult[] { new() { ErrorMessage = "The field is null." } }.AsEnumerable())
            : Apply(Field.ValueSet, InitCancellationTokenIfNull(ct));

    /// <summary>
    /// Applies the value transformations (if any) to the field using the imported records.
    /// </summary>
    /// <param name="importedRecords">The imported records to apply the transformations to.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns></returns>
    public Task<IEnumerable<TransformationResult>> Apply(DataTable importedRecords, CancellationToken? ct = null)
    {
        ct ??= InitCancellationTokenIfNull(ct);

        if (Field is null)
        {
            return Task.FromResult(new TransformationResult[] { new() { ErrorMessage = "The field is null." } }.AsEnumerable());
        }

        if (importedRecords.Columns[Field.FieldName] is null)
        {
            return Task.FromResult(new TransformationResult[] { new() { ErrorMessage = $"The field {Field.FieldName} does not exist in the data table." } }.AsEnumerable());
        }

        // Since we are being given a random data table, we need to get the values from the data table
        return Apply(importedRecords.GetColumnValues(Field.FieldName), ct);
    }

    /// <summary>
    /// Applies the value transformations (if any) to the field.
    /// </summary>
    /// <param name="values">The values to apply the transformations to.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>
    /// The result of the transformation.
    /// </returns>
    public async Task<IEnumerable<TransformationResult>> Apply(object[]? values, CancellationToken? ct = null)
    {
        ct ??= InitCancellationTokenIfNull(ct);

        try
        {
            // Reinitialize the TransformationResults with the original values
            if (values is null)
            {
                TransformationResults = ImmutableList.Create<TransformationResult>();
            }
            else
            {
                TransformationResults = values
                        .Select(x => new TransformationResult()
                        {
                            OriginalValue = x?.ToString(),
                            Value = x?.ToString()
                        })
                        .ToImmutableList();
            }
            TransformationResults = (await Apply(TransformationResults, ct)).ToImmutableList();

            return TransformationResults;
        }
        catch (OperationCanceledException)
        {
            return ImmutableList.Create<TransformationResult>();
        }
    }

    /// <summary>
    /// Applies the value transformations (if any) to provided values.
    /// </summary>
    /// <param name="results">The results of the previous transformations.</param>
    /// <param name="ct">The cancellation token.</param>
    public async Task<IEnumerable<TransformationResult>> Apply(IEnumerable<TransformationResult> results, CancellationToken? ct = null)
    {
        ct ??= InitCancellationTokenIfNull(ct);

        try
        {
            return await Task.WhenAll(results.Select(result => Apply(result, ct)));
        }
        catch (OperationCanceledException)
        {
            return TransformationResults;
        }
    }

    /// <summary>
    /// Applies the value transformations (if any) to the field using the data row.
    /// </summary>
    /// <param name="dataRow">The data row to apply the transformations to.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>
    /// The result of the transformation.
    /// </returns>
    public async Task<TransformationResult> Apply(DataRow dataRow, CancellationToken? ct = null)
    {
        ct ??= InitCancellationTokenIfNull(ct);

        if (Field is null) { return new() { ErrorMessage = "The field is null." }; }

        if (dataRow.Table.Columns.IndexOf(Field.FieldName) < 0)
        {
            return new() { ErrorMessage = $"The field {Field.FieldName} does not exist in the data row." };
        }

        return (await Apply(new[] { dataRow[Field.FieldName] }, ct)).FirstOrDefault()
            ?? new()
            {
                ErrorMessage = "The transformation produced no result.",
                OriginalValue = dataRow[Field.FieldName]?.ToString(),
                Value = dataRow[Field.FieldName]?.ToString()
            };
    }

    /// <summary>
    /// Applies the value transformations (if any) to provided value.
    /// </summary>
    /// <param name="result">The result of the previous transformation.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>
    /// The result of the transformation.
    /// </returns>
    public async Task<TransformationResult> Apply(TransformationResult result, CancellationToken? ct = null)
    {
        ct ??= InitCancellationTokenIfNull(ct);

        try
        {
            foreach (var transformation in ValueTransformations)
            {
                result = await transformation.Apply(result);
                if (ct.Value.IsCancellationRequested) { break; }
            }
            return result;
        }
        catch (OperationCanceledException)
        {
            return result;
        }
    }
    #endregion Apply Methods

    #region Helpers
    private CancellationToken InitCancellationTokenIfNull(CancellationToken? ct)
    {
        if (ct.HasValue) { return ct.Value; }

        _applyTransformationsTokenSource?.Cancel();
        _applyTransformationsTokenSource = new CancellationTokenSource();
        ct = _applyTransformationsTokenSource.Token;
        ct.Value.ThrowIfCancellationRequested();

        return ct.Value;
    }

    // This is used as the callback for the OnDefinitionChanged event of the ValueTransformations.
    // We cannot use a ApplyTransformations(CancellationToken? ct = null) method directly because
    // the OnDefinitionChanged event does not support optional parameters.
    private Task<IEnumerable<TransformationResult>> UpdateTransformationResults() => Apply(null);

    /// <summary>
    /// Clones the <see cref="FieldTransformation" />.
    /// </summary>
    /// <returns>The cloned <see cref="FieldTransformation" />.</returns>
    public FieldTransformation Clone()
    {
        var forRet = (FieldTransformation)MemberwiseClone();
        forRet.ValueTransformations = ValueTransformations.Select(x => x.Clone()).ToImmutableList();
        return forRet;
    }
    #endregion

    /// <inheritdoc />
    public void Dispose()
    {
        foreach (var transform in ValueTransformations)
        {
            transform.OnDefinitionChanged -= UpdateTransformationResults;
        }
    }
}
