using DataImportUtility.Models;
using DataImportUtility.ValueTransformations;

namespace DataImportUtility.Components.FieldMappingComponents.ValueTransformationComponents;

/// <summary>
/// The input for a map value transformation.
/// </summary>
public partial class MapTransformationInput : ValueTransformationInputBase<MapTransformation>
{
    private string? _inputKey;
    private string? _inputValue;
    private string? _errorMessage;

    private Task AddValueMapping()
    {
        _errorMessage = null;
        if (ValueTransformation is MapTransformation mapOperation)
        {
            if (mapOperation.ValueMappings.Any(vm => vm.ImportedFieldName == mapOperation.FieldName && vm.FromValue == (_inputKey ?? string.Empty)))
            {
                _errorMessage = $"A mapping for '{_inputKey}' already exists.";
                return Task.CompletedTask;
            }

            mapOperation.ValueMappings.Add(new() { ImportedFieldName = mapOperation.FieldName!, FromValue = _inputKey ?? string.Empty, ToValue = _inputValue ?? string.Empty });
            _inputKey = null;
            _inputValue = null;
        }

        return OnAfterInput.InvokeAsync();
    }

    private Task RemoveValueMapping(ValueMap valueMap)
    {
        if (ValueTransformation is MapTransformation mapOperation && mapOperation.ValueMappings.Contains(valueMap))
        {
            mapOperation.ValueMappings.Remove(valueMap);
        }
        return OnAfterInput.InvokeAsync();
    }
}
