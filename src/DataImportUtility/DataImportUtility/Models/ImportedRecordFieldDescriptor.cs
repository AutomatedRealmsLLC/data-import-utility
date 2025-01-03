using System.Data;
using System.Text.Json.Serialization;

using DataImportUtility.Helpers;

namespace DataImportUtility.Models;

/// <summary>
/// Describes a field mapping for an imported record.
/// </summary>
public class ImportedRecordFieldDescriptor
{
    /// <summary>
    /// The data file that the field is imported from.
    /// </summary>
    [JsonIgnore]
    public ImportedDataFile? ImportedDataFile { get; set; }

    /// <summary>
    /// The name of the table that the field is in within the data set.
    /// </summary>
    public string? ForTableName { get; set; }

    /// <summary>
    /// The table that the field is in.
    /// </summary>
    [JsonIgnore]
    public DataTable? ForTable => ImportedDataFile?.DataSet?.Tables[ForTableName];

    /// <summary>
    /// The imported field name.
    /// </summary>
    public /*required*/ string FieldName { get; set; } = string.Empty;

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
            FieldTypeString = value.FullName!.ToString();
        }
    }
    private Type _fieldType = typeof(object);

    /// <summary>
    /// The value type for the source field as a string.
    /// </summary>
    public string FieldTypeString { get; set; } = typeof(object).FullName!.ToString();

    /// <summary>
    /// The sample values for the field.
    /// </summary>
    [JsonIgnore]
    public object[] ValueSet
    {
        get
        {
            if (ForTable is { Rows.Count: > 0 } && (_valueSet is null  || _valueSet.Length != ForTable.Rows.Count || _valueSet.Where((value, index) => value != ForTable.Rows[index][FieldName]).Any()))
            {
                UpdateValueSet(false);
            }
            return _valueSet ?? Array.Empty<object>();
        }
        private set
        {
            _valueSet = value;
        }
    }
    private object[]? _valueSet;

    /// <summary>
    /// Updates the ValueSet property based on the given source table.
    /// </summary>
    private void UpdateValueSet(bool onlyIfEmpty = true)
    {
        if (onlyIfEmpty && ValueSet is { Length: >0 }) { return; }
        if (ForTable is null)
        {
            _valueSet = null;
            return;
        }

        // Preserves the order of the values
        ValueSet = ForTable.GetColumnValues(FieldName);
    }

    /// <summary>
    /// Clones the <see cref="ImportedRecordFieldDescriptor"/>.
    /// </summary>
    /// <returns>A new <see cref="ImportedRecordFieldDescriptor"/> that is a deep clone of the original.</returns>
    public ImportedRecordFieldDescriptor Clone()
    {
        var forRet = (ImportedRecordFieldDescriptor)MemberwiseClone();

        if (ValueSet is { Length: >0 })
        {
            ValueSet.CopyTo(forRet.ValueSet = new object[ValueSet.Length], 0);
        }
        return forRet;
    }
}
