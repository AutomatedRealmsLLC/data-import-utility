using Microsoft.AspNetCore.Components;

using DataImportUtility.Abstractions;
using DataImportUtility.Components.Abstractions;
using DataImportUtility.Models;
using DataImportUtility.TransformOperations;

namespace DataImportUtility.Components.FieldMappingComponents.FieldTransformationComponents;

/// <summary>
/// Displays the detail input for a <see cref="ValueTransformationBase" /> rule.
/// </summary>
public partial class ValueTransformationRuleDetails : FileImportUtilityComponentBase
{
    /// <summary>
    /// The output target to display the configuration for.
    /// </summary>
    [Parameter, EditorRequired, AllowNull] public ValueTransformationBase ValueTransformation { get; set; }
    /// <summary>
    /// The callback for when the output target is changed.
    /// </summary>
    [Parameter] public EventCallback<ValueTransformationBase> ValueTransformationChanged { get; set; }
    /// <summary>
    /// The tranformation result coming in from previous transformation operations.
    /// </summary>
    [Parameter, EditorRequired, AllowNull] public TransformationResult CurrentTransformationResult { get; set; }
    /// <summary>
    /// The callback for when a new transformation result is available.
    /// </summary>
    [Parameter] public EventCallback<TransformationResult> NewTransformationResultAvailable { get; set; }

    [AllowNull]
    private TransformationResult _myTransformationResult;

    private readonly System.Timers.Timer _debounceTimer = new()
    {
        Interval = 200,
        AutoReset = false
    };

    private readonly string _id = $"{Guid.NewGuid()}"[^6..];

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        _myTransformationResult = await ValueTransformation.Apply(CurrentTransformationResult);
        _debounceTimer.Elapsed += HandleDebounceTimedOut;
    }

    private async void HandleDebounceTimedOut(object? sender, System.Timers.ElapsedEventArgs e)
    {
        _debounceTimer.Stop();
        await InvokeAsync(PerformValueUpdate);
    }

    /// <inheritdoc />
    protected override Task OnParametersSetAsync()
    {
        if (_myTransformationResult.OriginalValue?.ToString() != CurrentTransformationResult.OriginalValue?.ToString())
        {
            return PerformValueUpdate();
        }

        return Task.CompletedTask;
    }

    private void HandleOperationDetailChanged()
    {
        _debounceTimer.Stop();
        _debounceTimer.Start();
    }

    private async Task PerformValueUpdate()
    {
        _myTransformationResult = await ValueTransformation.Apply(CurrentTransformationResult);

        await Task.WhenAny(
            ValueTransformationChanged.InvokeAsync(ValueTransformation),
            NewTransformationResultAvailable.InvokeAsync(_myTransformationResult));
    }

    #region For Map Value Transformation Type 
    private string? _inputKey;
    private string? _inputValue;

    private Task AddValueMapping()
    {
        if (ValueTransformation is MapOperation mapOperation)
        {
            if (string.IsNullOrWhiteSpace(_inputKey) || string.IsNullOrWhiteSpace(_inputValue))
            {
                // TODO: Indicate an error that both fields are required.
                return Task.CompletedTask;
            }

            if (mapOperation.ValueMappings.Any(vm => vm.ImportedFieldName == mapOperation.FieldName && vm.FromValue == _inputKey))
            {
                // TODO: Indicate an error that the key already exists.
                return Task.CompletedTask;
            }

            mapOperation.ValueMappings.Add(new() { ImportedFieldName = mapOperation.FieldName!, FromValue = _inputKey, ToValue = _inputValue });
            _inputKey = null;
            _inputValue = null;
        }

        return ValueTransformationChanged.InvokeAsync(ValueTransformation);
    }

    private Task RemoveValueMapping(ValueMap valueMap)
    {
        if (ValueTransformation is MapOperation mapOperation && mapOperation.ValueMappings.Contains(valueMap))
        {
            mapOperation.ValueMappings.Remove(valueMap);
        }
        return ValueTransformationChanged.InvokeAsync(ValueTransformation);
    }
    #endregion For Map Value Transformation Type
}
