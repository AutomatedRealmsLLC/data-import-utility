using System.Data;

using Microsoft.AspNetCore.Components;

using DataImportUtility.Components.Abstractions;
using DataImportUtility.Components.State;
using DataImportUtility.Models;

namespace DataImportUtility.Components.DataSetComponents;

/// <summary>
/// The Component to display a <see cref="System.Data.DataSet" />.
/// </summary>
public partial class DataSetDisplay : FileImportUtilityComponentBase
{
    /// <summary>
    /// The data set to display.
    /// </summary>
    /// <remarks>
    /// If this is not provided, the <see cref="DataFileMapperState.DataFile" />'s <see cref="ImportedDataFile.DataSet"/> will be used.
    /// </remarks>
    [Parameter] public DataSet? DataSet { get; set; }
    /// <summary>
    /// Overrides the default header row.
    /// </summary>
    [Parameter] public RenderFragment<DataTable>? OverrideHeaderRow { get; set; }
    /// <summary>
    /// Overrides the default data row.
    /// </summary>
    [Parameter] public RenderFragment<DataRow>? OverrideDataRow { get; set; }
    /// <summary>
    /// The callback for when the selected data table changes.
    /// </summary>
    [Parameter] public EventCallback<DataTable?> OnSelectedDataTableChanged { get; set; }
    /// <summary>
    /// Whether to register this component to the <see cref="DataFileMapperState" /> (if one was provided).
    /// </summary>
    [Parameter] public bool RegisterSelfToState { get; set; } = true;

    /// <summary>
    /// The callback for when the selected data table changes.
    /// </summary>
    public EventCallback<DataTable?> OnSelectedDataTableChangedInternal { get; set; }
    /// <summary>
    /// The callback for when the show field mapper button is clicked.
    /// </summary>
    public EventCallback<DataTable> OnShowFieldMapperClickedInternal { get; set; }

    private DataSet? MyDataSet => DataSet ?? DataFileMapperState?.DataFile?.DataSet;
    private DataTable? _selectedDataTable;

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        if (RegisterSelfToState && DataFileMapperState is not null) { DataFileMapperState.RegisterDataSetDisplay(this); }
    }

    /// <inheritdoc />
    protected override Task OnParametersSetAsync()
    {
        if (MyDataSet is not null && _selectedDataTable is null)
        {
            _selectedDataTable = MyDataSet is null || MyDataSet.Tables.Count != 1
                ? null
                : MyDataSet.Tables[0];

            return _selectedDataTable is not null
                ? HandleSelectedDataTableChanged(new ChangeEventArgs() { Value = 0 })
                : Task.CompletedTask;
        }

        return base.OnParametersSetAsync();
    }

    private Task HandleSelectedDataTableChanged(ChangeEventArgs e)
    {
        if (e.Value is null)
        {
            _selectedDataTable = null;
            return NotifySelectedDataTableChanged();
        }

        if (MyDataSet is null)
        {
            return Task.CompletedTask;
        }

        var tableNameOrIndex = e.Value.ToString();
        if (int.TryParse(tableNameOrIndex, out var index))
        {
            if (index < MyDataSet.Tables.Count) { _selectedDataTable = MyDataSet.Tables[index]; }
        }
        else
        {
            _selectedDataTable = MyDataSet.Tables[tableNameOrIndex];
        }

        return NotifySelectedDataTableChanged();
    }

    private Task NotifySelectedDataTableChanged()
        => Task.WhenAll(
            OnSelectedDataTableChangedInternal.InvokeAsync(_selectedDataTable),
            OnSelectedDataTableChanged.InvokeAsync(_selectedDataTable));
}
