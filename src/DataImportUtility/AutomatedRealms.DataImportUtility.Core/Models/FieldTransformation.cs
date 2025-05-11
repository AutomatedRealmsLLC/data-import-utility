using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using AutomatedRealms.DataImportUtility.Abstractions; // For ValueTransformationBase, IDefinitionChanges
using AbstractionsModels = AutomatedRealms.DataImportUtility.Abstractions.Models;
using CoreModels = AutomatedRealms.DataImportUtility.Core.Models; // For Core.ImportedRecordFieldDescriptor
using AutomatedRealms.DataImportUtility.Core.Helpers; // For DataTableExtensions.GetColumnValues

namespace AutomatedRealms.DataImportUtility.Core.Models;

/// <summary>
/// Represents a field to obtain values from and the transformations to apply to the values.
/// Inherits from Abstractions.FieldTransformation and adds Core-specific execution logic.
/// </summary>
public class FieldTransformation : AbstractionsModels.FieldTransformation // IDisposable is inherited
{
    private CancellationTokenSource? _applyTransformationsTokenSource;

    /// <summary>
    /// Initializes a new instance of the <see cref="FieldTransformation"/> class.
    /// </summary>
    public FieldTransformation() : base()
    {
        InitializeCore();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FieldTransformation" /> class.
    /// </summary>
    /// <param name="field">The Core field descriptor to transform the value of.</param>
    public FieldTransformation(CoreModels.ImportedRecordFieldDescriptor? field)
        : base(field) 
    {
        InitializeCore();
    }

    private void InitializeCore()
    {
        // Subscribe to the base class event
        this.OnDefinitionChanged += OnBaseDefinitionChangedAsync;
        _ = UpdateTransformationResultsAsync();
    }

    // Changed to async Task to match Func<Task>? event type
    private async Task OnBaseDefinitionChangedAsync()
    {
        await UpdateTransformationResultsAsync();
    }

    /// <summary>
    /// Gets or sets the field to transform the value of, using the Core-specific descriptor.
    /// </summary>
    public new CoreModels.ImportedRecordFieldDescriptor? Field
    {
        get => base.Field as CoreModels.ImportedRecordFieldDescriptor;
        set => base.Field = value; 
    }

    /// <summary>
    /// Gets the results of the transformations. Populated by ApplyAsync methods.
    /// </summary>
    [JsonIgnore]
    public ImmutableList<AbstractionsModels.TransformationResult> TransformationResults { get; private set; } = ImmutableList<AbstractionsModels.TransformationResult>.Empty;

    #region Apply Methods
    /// <summary>
    /// Applies the value transformations (if any) to the field using the value set of the associated <see cref="Field"/>.
    /// </summary>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The result of the transformation.</returns>
    public async Task<IEnumerable<AbstractionsModels.TransformationResult>> ApplyAsync(CancellationToken? ct = null)
    {
        if (Field?.ValueSet is null) 
        {
            TransformationResults = ImmutableList<AbstractionsModels.TransformationResult>.Empty;
            return TransformationResults;
        }
        var results = await ApplyAsync(Field.ValueSet, InitCancellationTokenIfNull(ct));
        TransformationResults = ImmutableList.CreateRange(results);
        return TransformationResults;
    }

    /// <summary>
    /// Applies the value transformations (if any) to the field using the imported records.
    /// </summary>
    /// <param name="importedRecords">The imported records to apply the transformations to.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A list of transformation results.</returns>
    public async Task<IEnumerable<AbstractionsModels.TransformationResult>> ApplyAsync(DataTable importedRecords, CancellationToken? ct = null)
    {
        var cancellationToken = InitCancellationTokenIfNull(ct);

        if (Field is null)
        {
            TransformationResults = ImmutableList.CreateRange(new[] { new AbstractionsModels.TransformationResult { ErrorMessage = "The field is null." } });
            return TransformationResults;
        }

        if (string.IsNullOrEmpty(Field.FieldName) || !importedRecords.Columns.Contains(Field.FieldName))
        {
            TransformationResults = ImmutableList.CreateRange(new[] { new AbstractionsModels.TransformationResult { ErrorMessage = $"The field '{Field.FieldName}' does not exist in the data table." } });
            return TransformationResults;
        }

        var results = await ApplyAsync(DataTableExtensions.GetColumnValues(importedRecords, Field.FieldName), cancellationToken);
        TransformationResults = ImmutableList.CreateRange(results);
        return TransformationResults;
    }

    /// <summary>
    /// Applies the value transformations (if any) to the field.
    /// </summary>
    /// <param name="values">The values to apply the transformations to.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A list of transformation results.</returns>
    public async Task<IEnumerable<AbstractionsModels.TransformationResult>> ApplyAsync(object[]? values, CancellationToken? ct = null)
    {
        var cancellationToken = InitCancellationTokenIfNull(ct);
        ImmutableList<AbstractionsModels.TransformationResult> initialResults;

        if (values is null)
        {
            initialResults = ImmutableList<AbstractionsModels.TransformationResult>.Empty;
        }
        else
        {
            initialResults = ImmutableList.CreateRange(
                values.Select(x => new AbstractionsModels.TransformationResult()
                {
                    OriginalValue = x, 
                    OriginalValueType = x?.GetType(),
                    CurrentValue = x, // Changed from Value to CurrentValue
                    CurrentValueType = x?.GetType()
                }));
        }
        
        var finalResults = await ApplyAsync(initialResults, cancellationToken);
        return finalResults;
    }

    /// <summary>
    /// Applies the value transformations (if any) to provided values.
    /// </summary>
    /// <param name="results">The results of the previous transformations.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A list of transformation results.</returns>
    public async Task<IEnumerable<AbstractionsModels.TransformationResult>> ApplyAsync(IEnumerable<AbstractionsModels.TransformationResult> results, CancellationToken? ct = null)
    {
        var cancellationToken = InitCancellationTokenIfNull(ct);
        var processedResults = new List<AbstractionsModels.TransformationResult>();

        try
        {
            foreach (var result in results)
            {
                cancellationToken.ThrowIfCancellationRequested();
                processedResults.Add(await ApplyToSingleResultAsync(result, cancellationToken));
            }
            return processedResults;
        }
        catch (OperationCanceledException)
        {
            return processedResults.Any() ? processedResults : Enumerable.Empty<AbstractionsModels.TransformationResult>();
        }
    }

    /// <summary>
    /// Applies the value transformations (if any) to the field using the data row.
    /// </summary>
    /// <param name="dataRow">The data row to apply the transformations to.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The result of the transformation.</returns>
    public async Task<AbstractionsModels.TransformationResult> ApplyAsync(DataRow dataRow, CancellationToken? ct = null)
    {
        var cancellationToken = InitCancellationTokenIfNull(ct);

        if (Field is null) { return new AbstractionsModels.TransformationResult { ErrorMessage = "The field is null." }; }

        if (string.IsNullOrEmpty(Field.FieldName) || !dataRow.Table.Columns.Contains(Field.FieldName))
        {
            return new AbstractionsModels.TransformationResult { ErrorMessage = $"The field '{Field.FieldName}' does not exist in the data row." };
        }
        
        object? originalValue = dataRow[Field.FieldName];
        Type? originalType = originalValue?.GetType() ?? dataRow.Table.Columns[Field.FieldName]?.DataType ?? typeof(object);

        var initialResult = new AbstractionsModels.TransformationResult
        {
            OriginalValue = originalValue,
            OriginalValueType = originalType,
            CurrentValue = originalValue, // Changed from Value to CurrentValue
            CurrentValueType = originalType
        };
        
        return await ApplyToSingleResultAsync(initialResult, cancellationToken);
    }

    /// <summary>
    /// Applies the value transformations (if any) to a single provided transformation result.
    /// </summary>
    /// <param name="result">The result of the previous transformation.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The result of the transformation.</returns>
    public async Task<AbstractionsModels.TransformationResult> ApplyToSingleResultAsync(AbstractionsModels.TransformationResult result, CancellationToken? ct = null)
    {
        var cancellationToken = InitCancellationTokenIfNull(ct);
        // Use 'with' expression for cloning records
        var currentResult = result with { }; 

        try
        {
            foreach (var transformation in base.ValueTransformations)
            {
                cancellationToken.ThrowIfCancellationRequested();
                // Use CurrentValue
                currentResult = await transformation.Transform(currentResult.CurrentValue, currentResult.CurrentValueType ?? typeof(object));
                if (currentResult.WasFailure)
                {
                    break; 
                }
            }
            return currentResult;
        }
        catch (OperationCanceledException)
        {
            // Use CurrentValue
            return new AbstractionsModels.TransformationResult { ErrorMessage = "Operation cancelled.", OriginalValue = result.OriginalValue, OriginalValueType = result.OriginalValueType, CurrentValue = result.CurrentValue, CurrentValueType = result.CurrentValueType, WasFailure = true };
        }
        catch (Exception ex)
        {
            // Use CurrentValue
            return new AbstractionsModels.TransformationResult { ErrorMessage = $"An unexpected error occurred: {ex.Message}", OriginalValue = result.OriginalValue, OriginalValueType = result.OriginalValueType, CurrentValue = result.CurrentValue, CurrentValueType = result.CurrentValueType, WasFailure = true };
        }
    }
    #endregion Apply Methods

    #region Helpers
    private CancellationToken InitCancellationTokenIfNull(CancellationToken? ct)
    {
        if (ct.HasValue) { return ct.Value; }

        _applyTransformationsTokenSource?.Cancel();
        _applyTransformationsTokenSource?.Dispose();
        _applyTransformationsTokenSource = new CancellationTokenSource();
        return _applyTransformationsTokenSource.Token;
    }

    private async Task UpdateTransformationResultsAsync()
    {
        if (Field?.ValueSet is null)
        {
            TransformationResults = ImmutableList<AbstractionsModels.TransformationResult>.Empty;
            return;
        }
        var results = await ApplyAsync(Field.ValueSet, _applyTransformationsTokenSource?.Token ?? CancellationToken.None);
        TransformationResults = ImmutableList.CreateRange(results);
    }
    #endregion

    /// <summary>
    /// Creates a new object that is a copy of the current instance.
    /// </summary>
    /// <returns>A new object that is a copy of this instance.</returns>
    public override AbstractionsModels.FieldTransformation Clone()
    {
        var clone = (FieldTransformation)base.Clone(); 

        // Use 'with' expression for cloning records
        clone.TransformationResults = this.TransformationResults.Select(tr => tr with { }).ToImmutableList();
        
        clone._applyTransformationsTokenSource?.Dispose(); 
        clone._applyTransformationsTokenSource = null;

        return clone;
    }

    #region IDisposable
    private bool _disposedValue; // Keep this field as it's specific to this class's dispose pattern

    /// <inheritdoc />
    protected override void Dispose(bool disposing) // Add override
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                // Unsubscribe from the base class event
                this.OnDefinitionChanged -= OnBaseDefinitionChangedAsync;

                _applyTransformationsTokenSource?.Cancel();
                _applyTransformationsTokenSource?.Dispose();
                _applyTransformationsTokenSource = null;
            }
            _disposedValue = true;
        }
        base.Dispose(disposing); // Call base class dispose
    }

    // public override void Dispose() // Base class already has public Dispose(), so this override is not strictly needed unless adding more logic.
    // {
    //     Dispose(disposing: true);
    //     GC.SuppressFinalize(this);
    //     // base.Dispose(); // If base.Dispose() also calls GC.SuppressFinalize, this might be redundant or ensure it's called.
    // }
    #endregion
}
