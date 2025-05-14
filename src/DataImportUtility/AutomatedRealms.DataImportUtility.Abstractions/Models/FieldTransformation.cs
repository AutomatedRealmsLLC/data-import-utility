using AutomatedRealms.DataImportUtility.Abstractions.Helpers;

using System.Collections.Immutable;
using System.Data;
using System.Text.Json.Serialization;

namespace AutomatedRealms.DataImportUtility.Abstractions.Models;

/// <summary>
/// Defines a field and the sequence of value transformations to be applied to it.
/// This class is intended for definition and serialization; execution logic resides elsewhere.
/// </summary>
public class FieldTransformation : IDisposable, IEquatable<FieldTransformation>
{
    #region Constructors and Initialization
    /// <summary>
    /// Initializes a new instance of the <see cref="FieldTransformation"/> class.
    /// </summary>
    public FieldTransformation()
    {
        Initialize();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FieldTransformation"/> class with a specific field.
    /// </summary>
    /// <param name="field">The field descriptor.</param>
    public FieldTransformation(ImportedRecordFieldDescriptor? field)
    {
        Field = field;
        Initialize();
    }

    private void Initialize()
    {
        // Subscribe to the base class event
        OnDefinitionChanged += OnBaseDefinitionChangedAsync;
        _ = UpdateTransformationResultsAsync();
    }
    #endregion Constructors and Initialization

    #region Fields and Properites
    private ImmutableList<ValueTransformationBase> _valueTransformations = ImmutableList<ValueTransformationBase>.Empty;
    private CancellationTokenSource? _applyTransformationsTokenSource;

    /// <summary>
    /// The descriptor of the field whose value is to be transformed.
    /// </summary>
    public virtual ImportedRecordFieldDescriptor? Field { get; set; }

    /// <summary>
    /// Gets the list of value transformations to apply to the field's value, in order.
    /// </summary>
    [JsonInclude]
    public ImmutableList<ValueTransformationBase> ValueTransformations
    {
        get => _valueTransformations;
        protected set // Changed to protected set
        {
            if (EqualityComparer<ImmutableList<ValueTransformationBase>>.Default.Equals(_valueTransformations, value)) return;

            // Unsubscribe from old transformations
            foreach (var transformation in _valueTransformations)
            {
                transformation.OnDefinitionChanged -= HandleChildDefinitionChangedAsync;
            }

            _valueTransformations = value ?? ImmutableList<ValueTransformationBase>.Empty;

            // Subscribe to new transformations
            foreach (var transformation in _valueTransformations)
            {
                transformation.OnDefinitionChanged += HandleChildDefinitionChangedAsync;
            }
            _ = InvokeDefinitionChangedAsync(); // Fire and get task
        }
    }

    /// <summary>
    /// Gets the results of the transformations. Populated by ApplyAsync methods.
    /// </summary>
    [JsonIgnore]
    public ImmutableList<TransformationResult> TransformationResults { get; private set; } = ImmutableList<TransformationResult>.Empty;

    /// <summary>
    /// Event that is raised when the definition of this field transformation changes
    /// (either its own properties or a child transformation's definition).
    /// </summary>
    public event Func<Task>? OnDefinitionChanged;
    #endregion Fields and Properites

    #region Callback Methods
    // Changed to async Task to match Func<Task>? event type
    private async Task OnBaseDefinitionChangedAsync()
    {
        await UpdateTransformationResultsAsync();
    }

    private Task InvokeDefinitionChangedAsync()
    {
        return OnDefinitionChanged?.Invoke() ?? Task.CompletedTask;
    }

    private Task HandleChildDefinitionChangedAsync()
    {
        return InvokeDefinitionChangedAsync();
    }
    #endregion Callback Methods

    #region ValueTransformation Management Methods
    /// <summary>
    /// Adds a value transformation to the end of the sequence.
    /// </summary>
    /// <param name="transformation">The transformation to add.</param>
    public virtual void AddTransformation(ValueTransformationBase transformation) // Made virtual
    {
        if (transformation is null) throw new ArgumentNullException(nameof(transformation));
        ValueTransformations = ValueTransformations.Add(transformation);
    }

    /// <summary>
    /// Removes a specific value transformation from the sequence.
    /// </summary>
    /// <param name="transformation">The transformation to remove.</param>
    public virtual void RemoveTransformation(ValueTransformationBase transformation) // Made virtual
    {
        if (transformation is null) throw new ArgumentNullException(nameof(transformation));
        ValueTransformations = ValueTransformations.Remove(transformation);
    }

    /// <summary>
    /// Removes a value transformation at the specified index.
    /// </summary>
    /// <param name="index">The index of the transformation to remove.</param>
    public virtual void RemoveTransformationAt(int index) // Made virtual
    {
        ValueTransformations = ValueTransformations.RemoveAt(index);
    }

    /// <summary>
    /// Replaces an existing value transformation with a new one.
    /// </summary>
    /// <param name="oldTransformation">The transformation to be replaced.</param>
    /// <param name="newTransformation">The new transformation.</param>
    public virtual void ReplaceTransformation(ValueTransformationBase oldTransformation, ValueTransformationBase newTransformation) // Made virtual
    {
        if (oldTransformation is null) throw new ArgumentNullException(nameof(oldTransformation));
        if (newTransformation is null) throw new ArgumentNullException(nameof(newTransformation));
        ValueTransformations = ValueTransformations.Replace(oldTransformation, newTransformation);
    }

    /// <summary>
    /// Replaces the value transformation at the specified index with a new one.
    /// </summary>
    /// <param name="index">The index of the transformation to replace.</param>
    /// <param name="newTransformation">The new transformation.</param>
    public virtual void ReplaceTransformationAt(int index, ValueTransformationBase newTransformation) // Made virtual
    {
        if (newTransformation is null) throw new ArgumentNullException(nameof(newTransformation));
        ValueTransformations = ValueTransformations.SetItem(index, newTransformation);
    }

    /// <summary>
    /// Clears all value transformations.
    /// </summary>
    public virtual void ClearTransformations() // Made virtual
    {
        ValueTransformations = ImmutableList<ValueTransformationBase>.Empty;
    }

    /// <summary>
    /// Sets all value transformations, replacing any existing ones.
    /// </summary>
    /// <param name="transformations">The collection of transformations to set.</param>
    public virtual void SetTransformations(IEnumerable<ValueTransformationBase> transformations) // Made virtual
    {
        ValueTransformations = transformations?.ToImmutableList() ?? ImmutableList<ValueTransformationBase>.Empty;
    }
    #endregion ValueTransformation Management Methods

    #region Clone and Equals Overrides
    /// <summary>
    /// Creates a new object that is a copy of the current instance.
    /// </summary>
    /// <returns>A new object that is a copy of this instance.</returns>
    public virtual FieldTransformation Clone()
    {
        var clone = (FieldTransformation)this.MemberwiseClone();

        clone.OnDefinitionChanged = null; // Clear event subscriptions on the new clone

        clone.Field = this.Field?.Clone(); // Clone the field descriptor

        // Deep clone ValueTransformations and re-subscribe internal child event handlers for the clone
        var clonedValueTransformations = ImmutableList.CreateRange(this.ValueTransformations.Select(vt => vt.Clone()));
        clone._valueTransformations = ImmutableList<ValueTransformationBase>.Empty; // Start with empty to clear old (if any from memberwise)
        clone.ValueTransformations = clonedValueTransformations; // This will call the setter and wire up clone.HandleChildDefinitionChangedAsync

        clone.TransformationResults = TransformationResults.Select(tr => tr with { }).ToImmutableList();

        clone._applyTransformationsTokenSource?.Dispose();
        clone._applyTransformationsTokenSource = null;

        return clone;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return Equals(obj as FieldTransformation);
    }

    /// <inheritdoc />
    public bool Equals(FieldTransformation? other)
    {
        if (other is null) return false;
        return ReferenceEquals(this, other) || EqualityComparer<ImportedRecordFieldDescriptor?>.Default.Equals(Field, other.Field) &&
               ValueTransformations.SequenceEqual(other.ValueTransformations);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        int hash = 17;
        hash = hash * 23 + (Field?.GetHashCode() ?? 0);
        foreach (var transformation in ValueTransformations)
        {
            hash = hash * 23 + (transformation?.GetHashCode() ?? 0);
        }
        return hash;
    }
    #endregion Clone and Equals Overrides

    #region Apply Methods
    /// <summary>
    /// Applies the value transformations (if any) to the field using the value set of the associated <see cref="Field"/>.
    /// </summary>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The result of the transformation.</returns>
    public async Task<IEnumerable<TransformationResult>> ApplyAsync(CancellationToken? ct = null)
    {
        if (Field?.ValueSet is null)
        {
            TransformationResults = ImmutableList<TransformationResult>.Empty;
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
    public async Task<IEnumerable<TransformationResult>> ApplyAsync(DataTable importedRecords, CancellationToken? ct = null)
    {
        var cancellationToken = InitCancellationTokenIfNull(ct);

        if (Field is null)
        {
            TransformationResults = ImmutableList.CreateRange([new TransformationResult { ErrorMessage = "The field is null." }]);
            return TransformationResults;
        }

        if (string.IsNullOrEmpty(Field.FieldName) || !importedRecords.Columns.Contains(Field.FieldName))
        {
            TransformationResults = ImmutableList.CreateRange([new TransformationResult { ErrorMessage = $"The field '{Field.FieldName}' does not exist in the data table." }]);
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
    public async Task<IEnumerable<TransformationResult>> ApplyAsync(object[]? values, CancellationToken? ct = null)
    {
        var cancellationToken = InitCancellationTokenIfNull(ct);
        ImmutableList<TransformationResult> initialResults;

        if (values is null)
        {
            initialResults = ImmutableList<TransformationResult>.Empty;
        }
        else
        {
            initialResults = ImmutableList.CreateRange(
                values.Select(x => new TransformationResult()
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
    public async Task<IEnumerable<TransformationResult>> ApplyAsync(IEnumerable<TransformationResult> results, CancellationToken? ct = null)
    {
        var cancellationToken = InitCancellationTokenIfNull(ct);
        var processedResults = new List<TransformationResult>();

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
            return processedResults.Any() ? processedResults : Enumerable.Empty<TransformationResult>();
        }
    }

    /// <summary>
    /// Applies the value transformations (if any) to the field using the data row.
    /// </summary>
    /// <param name="dataRow">The data row to apply the transformations to.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The result of the transformation.</returns>
    public async Task<TransformationResult> ApplyAsync(DataRow dataRow, CancellationToken? ct = null)
    {
        var cancellationToken = InitCancellationTokenIfNull(ct);

        if (Field is null) { return new TransformationResult { ErrorMessage = "The field is null." }; }

        if (string.IsNullOrEmpty(Field.FieldName) || !dataRow.Table.Columns.Contains(Field.FieldName))
        {
            return new TransformationResult { ErrorMessage = $"The field '{Field.FieldName}' does not exist in the data row." };
        }

        object? originalValue = dataRow[Field.FieldName];
        Type? originalType = originalValue?.GetType() ?? dataRow.Table.Columns[Field.FieldName]?.DataType ?? typeof(object);

        var initialResult = new TransformationResult
        {
            OriginalValue = originalValue,
            OriginalValueType = originalType,
            CurrentValue = originalValue, // Changed from Value to CurrentValue
            CurrentValueType = originalType
        };

        return await ApplyToSingleResultAsync(initialResult, cancellationToken);
    }    /// <summary>
         /// Applies the value transformations (if any) to a single provided transformation result.
         /// </summary>
         /// <param name="result">The result of the previous transformation.</param>
         /// <param name="ct">The cancellation token.</param>
         /// <returns>The result of the transformation.</returns>
    public async Task<TransformationResult> ApplyToSingleResultAsync(TransformationResult result, CancellationToken? ct = null)
    {
        var cancellationToken = InitCancellationTokenIfNull(ct);
        // Use 'with' expression for cloning records
        var currentResult = result with { };

        try
        {
            foreach (var transformation in ValueTransformations)
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
            // Create a new result with an error message instead of directly setting WasFailure
            return TransformationResult.Failure(
                originalValue: result.OriginalValue,
                targetType: result.CurrentValueType ?? typeof(object),
                errorMessage: "Operation cancelled.",
                originalValueType: result.OriginalValueType,
                currentValueType: result.CurrentValueType);
        }
        catch (Exception ex)
        {
            // Create a new result with an error message instead of directly setting WasFailure
            return TransformationResult.Failure(
                originalValue: result.OriginalValue,
                targetType: result.CurrentValueType ?? typeof(object),
                errorMessage: $"An unexpected error occurred: {ex.Message}",
                originalValueType: result.OriginalValueType,
                currentValueType: result.CurrentValueType);
        }
    }
    #endregion Apply Methods

    #region Dispose Pattern
    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Unsubscribe from child transformations
            foreach (var transformation in _valueTransformations)
            {
                transformation.OnDefinitionChanged -= HandleChildDefinitionChangedAsync;
            }

            // Clear the main event's invocation list
            OnDefinitionChanged = null;

            _applyTransformationsTokenSource?.Cancel();
            _applyTransformationsTokenSource?.Dispose();
            _applyTransformationsTokenSource = null;
        }
    }
    #endregion Dispose Pattern

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
            TransformationResults = ImmutableList<TransformationResult>.Empty;
            return;
        }
        var results = await ApplyAsync(Field.ValueSet, _applyTransformationsTokenSource?.Token ?? CancellationToken.None);
        TransformationResults = ImmutableList.CreateRange(results);
    }
    #endregion
}
