using Microsoft.AspNetCore.Components;

using DataImportUtility.Components.Abstractions;

namespace DataImportUtility.Components.CustomErrorBoundaryComponent;

/// <summary>
/// The default error context content component.
/// </summary>
public partial class DefaultErrorContextContent : FileImportUtilityComponentBase
{
    /// <summary>
    /// The error context.
    /// </summary>
    [Parameter, EditorRequired, AllowNull] public Exception ErrorContext { get; set; }
}