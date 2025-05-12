using AutomatedRealms.DataImportUtility.Abstractions.Models;

namespace DataImportUtility.Tests.SampleData;

internal partial class ImportDataObjects
{
    private readonly static List<ValueMap> _valueMappings =
    [
        new ValueMap() { ImportedFieldName = "Lab Sample ID", FromValue = "1", ToValue = "One" },
        new ValueMap() { ImportedFieldName = "Lab Sample ID", FromValue = "2", ToValue = "Two" },
        new ValueMap() { ImportedFieldName = "Lab Sample ID", FromValue = "3", ToValue = "Three" },
        new ValueMap() { ImportedFieldName = "Matrix", FromValue = "Water", ToValue = "W" },
        new ValueMap() { ImportedFieldName = "Matrix", FromValue = "Soil", ToValue = "S" },
        new ValueMap() { ImportedFieldName = "Matrix", FromValue = "1", ToValue = "A" },
    ];

    /// <summary>
    /// The data to use for testing the <see cref="ValueMapRule"/>.
    /// </summary>
    public static IEnumerable<object?[]> ValidInputData
    {
        get
        {
            yield return ["Lab Sample ID", "1", _valueMappings, "One"];
            yield return ["Lab Sample ID", "4", _valueMappings, "4"];
            yield return ["Matrix", "Water", _valueMappings, "W"];
            yield return ["Matrix", "Air", _valueMappings, "Air"];
            yield return ["Lab Sample ID", "Water", _valueMappings, "Water"];
            yield return ["Matrix", "2", _valueMappings, "2"];
            yield return ["Not In Lookup List Field", "1", _valueMappings, "1"];
            yield return ["", "1", _valueMappings, "One"];
            yield return [null, "1", _valueMappings, "One"];
        }
    }

}