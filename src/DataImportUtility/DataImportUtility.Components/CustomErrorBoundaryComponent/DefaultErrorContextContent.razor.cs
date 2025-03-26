using DataImportUtility.Components.Abstractions;

using Microsoft.AspNetCore.Components;

namespace DataImportUtility.Components.CustomErrorBoundaryComponent;

/// <summary>
/// The default error context content component.
/// </summary>
public partial class DefaultErrorContextContent : FileImportUtilityComponentBase
{
    /// <summary>
    /// The error context.
    /// </summary>
    [Parameter, EditorRequired] public Exception ErrorContext { get; set; } = default!;
}