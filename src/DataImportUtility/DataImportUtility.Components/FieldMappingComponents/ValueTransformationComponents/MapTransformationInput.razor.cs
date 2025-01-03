using DataImportUtility.Models;
using DataImportUtility.TransformOperations;

namespace DataImportUtility.Components.FieldMappingComponents.ValueTransformationComponents;

/// <summary>
/// The input for a map value transformation.
/// </summary>
public partial class MapTransformationInput : ValueTransformationInputBase<MapOperation>
{
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

        return OnAfterInput.InvokeAsync();
    }

    private Task RemoveValueMapping(ValueMap valueMap)
    {
        if (ValueTransformation is MapOperation mapOperation && mapOperation.ValueMappings.Contains(valueMap))
        {
            mapOperation.ValueMappings.Remove(valueMap);
        }
        return OnAfterInput.InvokeAsync();
    }
}
