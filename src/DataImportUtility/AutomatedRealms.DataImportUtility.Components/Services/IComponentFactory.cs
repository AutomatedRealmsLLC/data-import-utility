namespace AutomatedRealms.DataImportUtility.Components.Services;

using AutomatedRealms.DataImportUtility.Components.Models;

using Microsoft.AspNetCore.Components;

using System.Collections.Generic;

/// <summary>
/// Interface for creating Blazor components from model types.
/// </summary>
public interface IComponentFactory
{
    /// <summary>
    /// Creates a component from a mapping strategy.
    /// </summary>
    /// <typeparam name="TModel">The target model type.</typeparam>
    /// <param name="strategy">The mapping strategy to use.</param>
    /// <param name="parameters">The parameters to pass to the component.</param>
    /// <returns>A render fragment that renders the component.</returns>
    RenderFragment CreateMappingComponent<TModel>(MappingStrategy strategy, Dictionary<string, object>? parameters = null)
        where TModel : class, new();

    /// <summary>
    /// Creates a component from an import workflow.
    /// </summary>
    /// <typeparam name="TModel">The target model type.</typeparam>
    /// <param name="workflow">The workflow to use.</param>
    /// <param name="parameters">The parameters to pass to the component.</param>
    /// <returns>A render fragment that renders the component.</returns>
    RenderFragment CreateWorkflowComponent<TModel>(ImportWorkflow workflow, Dictionary<string, object>? parameters = null)
        where TModel : class, new();

    /// <summary>
    /// Creates a component from a file type.
    /// </summary>
    /// <param name="fileType">The file type.</param>
    /// <param name="parameters">The parameters to pass to the component.</param>
    /// <returns>A render fragment that renders the component.</returns>
    RenderFragment CreateFileTypeComponent(FileType fileType, Dictionary<string, object>? parameters = null);
}