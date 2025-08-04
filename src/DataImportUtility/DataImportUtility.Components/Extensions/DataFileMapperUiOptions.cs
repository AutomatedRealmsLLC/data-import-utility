using DataImportUtility.Components.Abstractions;
using DataImportUtility.Components.FieldMappingComponents;
using DataImportUtility.Components.FilePickerComponent;

namespace DataImportUtility.Components.Extensions;

/// <summary>
/// Options for the data file mapper UI.
/// </summary>
public class DataFileMapperUiOptions
{
    /// <summary>
    /// The default field mapper editor component.
    /// </summary>
    /// <remarks>
    /// This is the default component that will be used for editing field mappings.
    /// It must be a concrete implementation of <see cref="FieldMapperEditorBase" />.
    /// The default value is <see cref="FieldMapperEditor" /> if no value is set.
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// Thrown if the type is not a subclass of <see cref="FieldMapperEditorBase" /> 
    /// or if it is not a concrete type.
    /// </exception>
    public Type FieldMapperEditorComponentType
    {
        get => _fieldMapperComponentType;
        set
        {
            // Throw an exception if the type is not a subclass of <see cref="FieldMapperEditorBase" />
            // or if it is not a concrete type.
            if (value is null)
            {
                _fieldMapperComponentType = typeof(FieldMapperEditor);
                return;
            }

            if (!typeof(FieldMapperEditorBase).IsAssignableFrom(value))
            {
                throw new ArgumentException($"The type {value.FullName} is not a subclass of {nameof(FieldMapperEditorBase)}.", nameof(value));
            }
            else if (value.IsAbstract)
            {
                throw new ArgumentException($"The type {value.FullName} is an abstract type.", nameof(value));
            }
            else if (value.IsGenericTypeDefinition)
            {
                throw new ArgumentException($"The type {value.FullName} is a generic type definition.", nameof(value));
            }
            else if (value.IsInterface)
            {
                throw new ArgumentException($"The type {value.FullName} is an interface.", nameof(value));
            }

            _fieldMapperComponentType = value;
        }
    }
    private Type _fieldMapperComponentType = typeof(FieldMapperEditor);

    /// <summary>
    /// The data file picker component to use.  The default is <see cref="DataFilePicker" />.
    /// </summary>
    public Type DataFilePickerComponentType
    {
        get => _dataFilePickerComponentType;
        set
        {
            // Throw an exception if the type is not a subclass of DataFilePickerComponentType
            // or if it is not a concrete type.
            if (value is null)
            {
                _dataFilePickerComponentType = typeof(DataFilePicker);
                return;
            }
            if (!typeof(DataFilePickerComponentBase).IsAssignableFrom(value))
            {
                throw new ArgumentException($"The type {value.FullName} is not a subclass of {nameof(DataFilePickerComponentBase)}.", nameof(value));
            }
            else if (value.IsAbstract)
            {
                throw new ArgumentException($"The type {value.FullName} is an abstract type.", nameof(value));
            }
            else if (value.IsGenericTypeDefinition)
            {
                throw new ArgumentException($"The type {value.FullName} is a generic type definition.", nameof(value));
            }
            else if (value.IsInterface)
            {
                throw new ArgumentException($"The type {value.FullName} is an interface.", nameof(value));
            }
            _dataFilePickerComponentType = value;
        }
    }
    private Type _dataFilePickerComponentType = typeof(DataFilePicker);
}
