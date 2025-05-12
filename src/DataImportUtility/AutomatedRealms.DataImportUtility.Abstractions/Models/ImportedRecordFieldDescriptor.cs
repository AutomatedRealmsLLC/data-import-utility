using System.Data;
using System.Text.Json.Serialization;

namespace AutomatedRealms.DataImportUtility.Abstractions.Models;

/// <summary>
/// Describes a field from an imported data source.
/// </summary>
public class ImportedRecordFieldDescriptor
{
    /// <summary>
    /// The name of the table that the field is in, if applicable.
    /// </summary>
    public string? ForTableName { get; set; }

    /// <summary>
    /// The imported field name.
    /// </summary>
    public string FieldName { get; set; /*init;*/ } = string.Empty; // Removed 'required', added initializer

    private Type _fieldType = typeof(object);
    /// <summary>
    /// The value type for the source field.
    /// </summary>
    [JsonIgnore]
    public Type FieldType
    {
        get => _fieldType;
        set
        {
            if (value == _fieldType) { return; }
            _fieldType = value;
            _fieldTypeString = value.AssemblyQualifiedName ?? value.FullName ?? value.ToString();
        }
    }

    private string _fieldTypeString = typeof(object).AssemblyQualifiedName!;
    /// <summary>
    /// The value type for the source field as an assembly qualified string.
    /// </summary>
    public string FieldTypeString
    {
        get => _fieldTypeString;
        set
        {
            if (value == _fieldTypeString) { return; }
            _fieldTypeString = value;
            _fieldType = Type.GetType(value) ?? typeof(object);
        }
    }

    private object[]? _valueSet;
    /// <summary>
    /// A sample or complete set of values for the field.
    /// This property is externally populated in the Abstractions layer.
    /// </summary>
    public virtual object[]? ValueSet // Made virtual
    {
        get => _valueSet;
        set => _valueSet = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImportedRecordFieldDescriptor"/> class.
    /// </summary>
    public ImportedRecordFieldDescriptor() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImportedRecordFieldDescriptor"/> class.
    /// </summary>
    /// <param name="fieldName">The name of the field.</param>
    public ImportedRecordFieldDescriptor(string fieldName)
    {
        FieldName = fieldName;
    }

    /// <summary>
    /// Clones the <see cref="ImportedRecordFieldDescriptor" />.
    /// </summary>
    /// <returns>The cloned <see cref="ImportedRecordFieldDescriptor" />.</returns>
    public virtual ImportedRecordFieldDescriptor Clone()
    {
        var clone = (ImportedRecordFieldDescriptor)MemberwiseClone();
        if (_valueSet is not null)
        {
            // Ensure a shallow copy of the array itself. If objects within ValueSet need deep cloning,
            // that would require more specific logic, but typically ValueSet holds simple types or strings.
            clone._valueSet = (object[]?)_valueSet.Clone();
        }
        return clone;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is ImportedRecordFieldDescriptor other)
        {
            return FieldName == other.FieldName &&
                   ForTableName == other.ForTableName &&
                   FieldTypeString == other.FieldTypeString &&
                   ((_valueSet == null && other._valueSet == null) ||
                    (_valueSet != null && other._valueSet != null && _valueSet.SequenceEqual(other._valueSet)));
        }
        return false;
    }

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    public override int GetHashCode()
    {
        int hash = 17;
        hash = hash * 23 + (FieldName?.GetHashCode() ?? 0);
        hash = hash * 23 + (ForTableName?.GetHashCode() ?? 0);
        hash = hash * 23 + (FieldTypeString?.GetHashCode() ?? 0);
        // Consider if ValueSet should be part of the hash code. It can be large.
        // For now, omitting ValueSet from hash code for performance and simplicity.
        return hash;
    }

    /// <summary>
    /// The data file that the field is imported from.
    /// </summary>
    [JsonIgnore]
    public ImportedDataFile? ImportedDataFile { get; set; } // ImportedDataFile is in Core.Models

    /// <summary>
    /// The table that the field is in.
    /// </summary>
    [JsonIgnore]
    public DataTable? ForTable => ImportedDataFile?.DataSet?.Tables[ForTableName!];
}
