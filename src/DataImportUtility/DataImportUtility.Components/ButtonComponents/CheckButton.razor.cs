using System.Diagnostics.Contracts;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace DataImportUtility.Components.ButtonComponents;

/// <summary>
/// The component for the button components.
/// </summary>
public partial class CheckButton
{
    /// <summary>
    /// Whether the button is checked.
    /// </summary>
    [Parameter] public bool Checked { get; set; }
    /// <summary>
    /// The callback for when the checked state changes.
    /// </summary>
    [Parameter] public EventCallback<bool> CheckedChanged { get; set; }
    /// <summary>
    /// The callback for when the button is clicked.
    /// </summary>
    [Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }
    /// <summary>
    /// Additional attributes to apply to the component.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)] public IDictionary<string, object?>? AdditionalAttributes { get; set; }

    private Task HandleClicked(MouseEventArgs e)
    {
        Checked = !Checked;
        return OnClick.InvokeAsync(e);
    }
}
