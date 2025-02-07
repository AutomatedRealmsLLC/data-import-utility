using System.Data;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using DataImportUtility.Components.Abstractions;
using DataImportUtility.Components.JsInterop;
using DataImportUtility.Components.State;

namespace DataImportUtility.Components.DataSetComponents;

/// <summary>
/// The Component to display a <see cref="DataTable" />.
/// </summary>
public partial class DataTableDisplay : FileImportUtilityComponentBase
{
    [Inject, AllowNull] private IJSRuntime JsRuntime { get; set; }

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
    /// <summary>
    /// Additional CSS classes to apply to the table element.
    /// </summary>
    [Parameter] public string? AdditionalCssClasses { get; set; }
    /// <summary>
    /// The row index to mark as hovered.
    /// </summary>
    [Parameter] public int? HoveredRow { get; set; }
    /// <summary>
    /// Whether to show the row select column.
    /// </summary>
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
    /// The callback for when the hovered row changes.
    /// </summary>
    [Parameter] public EventCallback<int?> OnHoveredRowChanged { get; set; }

    /// <summary>
    /// The Id of the data table.
    /// </summary>
    public string Id { get; } = $"dataTable{Guid.NewGuid().ToString()[^5..]}";
    /// <summary>
    /// Whether the component has been rendered in the DOM.
    /// </summary>
    public bool TableIsRendered { get; private set; }

    private int? _prevIndex;
    private bool _prevIsRendered;
    private bool _busy;

    private DataTable? MyDataTable => DataTable ?? DataFileMapperState?.ActiveDataTable;
    private string UseTableCssClasses => $"data-table-display{(!string.IsNullOrWhiteSpace(AdditionalCssClasses) ? $" {AdditionalCssClasses}" : null)}{(_busy ? " busy" : null)}{DefaultCssClass}";
    private int RowCount => MyDataTable?.Rows.Count ?? 0;

    private FileMapperJsModule? _fileMapperModule;
    private FileMapperJsModule FileMapperModule => _fileMapperModule ??= new(JsRuntime);

    /// <inheritdoc />
    protected override bool ShouldRender()
    {
        return base.ShouldRender();
    }

    /// <inheritdoc />
    protected override async void OnAfterRender(bool firstRender)
    {
        var curIsRendered = MyDataTable is not null && (await FileMapperModule.ElementExists(Id));
        if (_prevIsRendered != curIsRendered)
        {
            TableIsRendered = curIsRendered;
            _prevIsRendered = curIsRendered;
        }
    }

    private Task HandleMouseHover(int? rowIndex)
    {
        if (_busy) { return Task.CompletedTask; }
        if (rowIndex == _prevIndex) { return Task.CompletedTask; }
        _prevIndex = rowIndex;
        return DoMouseOver(_prevIndex);
    }

    private Task DoMouseOver(int? rowIndex) => OnHoveredRowChanged.InvokeAsync(rowIndex);

    private Task HandleToggleRowSelected(int? rowIndex = null)
    {
        if (_busy) { return Task.CompletedTask; }
        _busy = true;
        StateHasChanged();
        try
        {
            if (MyDataTable is null)
            {
                SelectedRows.Clear();
                return SelectedRowsChanged.InvokeAsync(SelectedRows);
            }

            if (!rowIndex.HasValue)
            {
                // Toggle select all
                if (SelectedRows.Count == RowCount)
                {
                    SelectedRows.Clear();
                }
                else
                {
                    SelectedRows.AddRange(MyDataTable.Rows.OfType<DataRow>().Select((_, i) => i).Except(SelectedRows));
                }
                return SelectedRowsChanged.InvokeAsync(SelectedRows);
            }

            if (SelectedRows.Contains(rowIndex.Value))
            {
                SelectedRows.Remove(rowIndex.Value);
            }
            else
            {
                SelectedRows.Add(rowIndex.Value);
            }

            return SelectedRowsChanged.InvokeAsync(SelectedRows);

        }
        finally
        {
            _busy = false;
        }
    }
}
