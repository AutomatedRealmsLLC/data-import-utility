namespace AutomatedRealms.DataImportUtility.Components.Services;

using AutomatedRealms.DataImportUtility.Components.Models;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

using System;
using System.Collections.Generic;

/// <summary>
/// Factory service for creating Blazor components from model types.
/// </summary>
public class ComponentFactory : IComponentFactory
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="ComponentFactory"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider for dependency injection.</param>
    public ComponentFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Creates a component from a mapping strategy.
    /// </summary>
    /// <typeparam name="TModel">The target model type.</typeparam>
    /// <param name="strategy">The mapping strategy to use.</param>
    /// <param name="parameters">The parameters to pass to the component.</param>
    /// <returns>A render fragment that renders the component.</returns>
    public RenderFragment CreateMappingComponent<TModel>(MappingStrategy strategy, Dictionary<string, object>? parameters = null)
        where TModel : class, new()
    {
        return builder =>
        {
            Type componentType = strategy.ComponentType;

            // Handle generic components
            if (strategy.IsGenericComponent)
            {
                componentType = componentType.MakeGenericType(typeof(TModel));
            }

            // Create the component
            CreateComponent(builder, componentType, parameters);
        };
    }

    /// <summary>
    /// Creates a component from an import workflow.
    /// </summary>
    /// <typeparam name="TModel">The target model type.</typeparam>
    /// <param name="workflow">The workflow to use.</param>
    /// <param name="parameters">The parameters to pass to the component.</param>
    /// <returns>A render fragment that renders the component.</returns>
    public RenderFragment CreateWorkflowComponent<TModel>(ImportWorkflow workflow, Dictionary<string, object>? parameters = null)
        where TModel : class, new()
    {
        return builder =>
        {
            Type componentType = workflow.ComponentType;

            // Handle generic components
            if (workflow.IsGenericComponent)
            {
                componentType = componentType.MakeGenericType(typeof(TModel));
            }

            // Create the component
            CreateComponent(builder, componentType, parameters);
        };
    }

    /// <summary>
    /// Creates a component from a file type.
    /// </summary>
    /// <param name="fileType">The file type.</param>
    /// <param name="parameters">The parameters to pass to the component.</param>
    /// <returns>A render fragment that renders the component.</returns>
    public RenderFragment CreateFileTypeComponent(FileType fileType, Dictionary<string, object>? parameters = null)
    {
        // Assuming the file type specific component is stored somewhere
        var componentType = GetComponentTypeForFileType(fileType);

        return builder => CreateComponent(builder, componentType, parameters);
    }

    private Type GetComponentTypeForFileType(FileType fileType)
    {
        // This method would be implemented to return the component type for the file type
        // For example, using a registry or convention

        // For now, we'll just return a default
        return typeof(FileTypeDisplay);
    }

    private void CreateComponent(RenderTreeBuilder builder, Type componentType, Dictionary<string, object>? parameters)
    {
        int sequence = 0;

        // Open the component
        builder.OpenComponent(sequence++, componentType);

        // Set parameters
        if (parameters != null)
        {
            foreach (var parameter in parameters)
            {
                builder.AddAttribute(sequence++, parameter.Key, parameter.Value);
            }
        }

        // Close the component
        builder.CloseComponent();
    }
}