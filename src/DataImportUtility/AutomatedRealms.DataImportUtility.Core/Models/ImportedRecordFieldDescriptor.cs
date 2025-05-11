using System.Data;
using System.Linq;
using System.Text.Json.Serialization;
using AutomatedRealms.DataImportUtility.Core.Helpers;
using AbstractionsModels = AutomatedRealms.DataImportUtility.Abstractions.Models;

namespace AutomatedRealms.DataImportUtility.Core.Models; // Updated

/// <summary>
/// Describes a field mapping for an imported record, with Core-specific data loading.
/// </summary>
public class ImportedRecordFieldDescriptor : AbstractionsModels.ImportedRecordFieldDescriptor
{
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

    private object[]? _coreValueSet;

    /// <summary>
    /// The sample values for the field, loaded from the ForTable.
    /// </summary>
    [JsonIgnore]
    public override object[]? ValueSet
    {
        get
        {
            // Use _coreValueSet as the backing field for the Core logic
            if (ForTable is { Rows.Count: > 0 } && 
                (_coreValueSet == null || 
                 _coreValueSet.Length != ForTable.Rows.Count || 
                 _coreValueSet.Where((value, index) => value != ForTable.Rows[index][FieldName]).Any()))
            {
                UpdateValueSetInternal(false);
            }
            return _coreValueSet ?? base.ValueSet ?? []; // Fallback to base if core is null, then to empty array
        }
        set
        {
            // When set externally, update both Core's backing and the base property
            _coreValueSet = value;
            base.ValueSet = value;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImportedRecordFieldDescriptor"/> class.
    /// </summary>
    public ImportedRecordFieldDescriptor() : base()
    {
    }

    /// <summary>
    /// Updates the ValueSet property based on the given source table.
    /// </summary>
    private void UpdateValueSetInternal(bool onlyIfEmpty = true)
    {
        if (onlyIfEmpty && _coreValueSet is { Length: > 0 }) { return; }
        if (ForTable is null || string.IsNullOrEmpty(FieldName) || !ForTable.Columns.Contains(FieldName)) // Added checks for FieldName and column existence
        {
            _coreValueSet = null;
            base.ValueSet = null; // Keep base in sync
            return;
        }

        // Preserves the order of the values
        // Explicitly call DataTableExtensions.GetColumnValues to resolve ambiguity
        var values = DataTableExtensions.GetColumnValues(ForTable, FieldName); 
        _coreValueSet = values;
        base.ValueSet = values; // Keep base in sync
    }

    /// <summary>
    /// Clones the <see cref="ImportedRecordFieldDescriptor" />.
    /// </summary>
    /// <returns>The cloned <see cref="ImportedRecordFieldDescriptor" />.</returns>
    public override AbstractionsModels.ImportedRecordFieldDescriptor Clone()
    {
        var clone = (ImportedRecordFieldDescriptor)base.Clone(); // Calls Abstractions.Clone()
        
        // Core-specific cloning:
        // ImportedDataFile is a reference, keep it as is (shallow copy for this reference).
        // clone.ImportedDataFile = this.ImportedDataFile; (already handled by MemberwiseClone if it were part of base, but it's Core specific)
        // _coreValueSet is cloned by the base.Clone() if base.ValueSet was set by this instance.
        // If _coreValueSet was managed independently and base.ValueSet was separate, it would need separate cloning:
        if (_coreValueSet != null)
        {
            clone._coreValueSet = (object[]?)_coreValueSet.Clone();
        }
        else
        {
            clone._coreValueSet = null;
        }
        // Ensure the base ValueSet on the clone is also correctly set if it was different
        // This is tricky because base.Clone() already cloned base.ValueSet. 
        // The current logic has ValueSet setter sync base.ValueSet, so it should be fine.

        return clone;
    }
}
