using System.Data;

using Microsoft.AspNetCore.Components;

using DataImportUtility.Components.Abstractions;
using DataImportUtility.Components.State;

namespace DataImportUtility.Components.DataSetComponents;

/// <summary>
/// The Component to display a <see cref="DataTable" />.
/// </summary>
public partial class DataTableDisplay : FileImportUtilityComponentBase
{
    /// <summary>
    /// The data table to display.
    /// </summary>
    /// <remarks>
    /// If this is not provided, the <see cref="DataFileMapperState.ActiveDataTable" /> will be used if the <see cref="DataFileMapperState" /> was provided.
    /// </remarks>
    [Parameter] public DataTable? DataTable { get; set; }
    /// <summary>
    /// Overrides the default header row.
    /// </summary>
    [Parameter] public RenderFragment<DataTable>? OverrideHeaderRow { get; set; }
    /// <summary>
    /// Overrides the default data row.
    /// </summary>
    [Parameter] public RenderFragment<DataRow>? OverrideDataRow { get; set; }

    private DataTable? MyDataTable => DataTable ?? DataFileMapperState?.ActiveDataTable;
}
