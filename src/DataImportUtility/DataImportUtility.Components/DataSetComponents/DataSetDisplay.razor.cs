using System.Data;

using DataImportUtility.Components.Abstractions;
using DataImportUtility.Components.State;
using DataImportUtility.Models;

using Microsoft.AspNetCore.Components;

namespace DataImportUtility.Components.DataSetComponents;

/// <summary>
/// The Component to display a <see cref="System.Data.DataSet" />.
/// </summary>
public partial class DataSetDisplay : DataFilePickerComponentBase
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
    /// The row index to mark as hovered.
    /// </summary>
    [Parameter] public int? HoveredRow { get; set; }
    /// <summary>
    /// The callback for when the hovered row changes.
    /// </summary>
    [Parameter] public EventCallback<int?> HoveredRowChanged { get; set; }
    /// <summary>
    /// Whether to show the row select column.
    /// </summary>
    /// <remarks>
    /// If you override the default data row, you will need to handle the header column
    /// for the row select, as well as the row select logic.
    /// </remarks>
    [Parameter] public bool ShowRowSelect { get; set; }
    /// <summary>
    /// Rows to mark as selected.
    /// </summary>
    [Parameter] public List<int> SelectedRows { get; set; } = [];
    /// <summary>
    /// The callback for when the selected rows change.
    /// </summary>
    [Parameter] public EventCallback<List<int>> SelectedRowsChanged { get; set; }
    /// <summary>
    /// The callback for when the selected data table changes.
    /// </summary>
    [Parameter] public EventCallback<DataTableDisplay?> OnSelectedDataTableChanged { get; set; }
    /// <summary>
    /// Whether to register this component to the <see cref="DataFileMapperState" /> (if one was provided).
    /// </summary>
    [Parameter] public bool RegisterSelfToState { get; set; } = true;
    /// <summary>
    /// Additional CSS classes to apply to the table element.
    /// </summary>
    [Parameter] public string? AdditionalTableCssClasses { get; set; }

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
    private DataTableDisplay? _dataTableRef;
    private string? _prevDataTableId;

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

    /// <inheritdoc />
    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        if (_dataTableRef?.Id != _prevDataTableId)
        {
            _prevDataTableId = _dataTableRef?.Id;
            return NotifySelectedDataTableChanged();
        }

        return base.OnAfterRenderAsync(firstRender);
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
            OnSelectedDataTableChanged.InvokeAsync(_dataTableRef));
}
