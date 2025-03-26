using DataImportUtility.Components.FieldMappingComponents;

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
}
