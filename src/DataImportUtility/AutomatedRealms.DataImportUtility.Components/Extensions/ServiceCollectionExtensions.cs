namespace AutomatedRealms.DataImportUtility.Components.Extensions;

using AutomatedRealms.DataImportUtility.Components.Models;
using AutomatedRealms.DataImportUtility.Components.Models.Validation;
using AutomatedRealms.DataImportUtility.Components.Services;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/> to register component library services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the data import component services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection, for method chaining.</returns>
    public static IServiceCollection AddDataImportComponents(this IServiceCollection services)
    {
        // Register core services
        services.AddSingleton<IComponentFactory, ComponentFactory>();

        return services;
    }

    /// <summary>
    /// Registers a custom file type.
    /// </summary>
    /// <typeparam name="T">The type of the file type to register.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection, for method chaining.</returns>
    public static IServiceCollection AddFileType<T>(this IServiceCollection services)
        where T : FileType, new()
    {
        FileType.Register(new T());
        return services;
    }

    /// <summary>
    /// Registers a custom import workflow.
    /// </summary>
    /// <typeparam name="T">The type of the workflow to register.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection, for method chaining.</returns>
    public static IServiceCollection AddImportWorkflow<T>(this IServiceCollection services)
        where T : ImportWorkflow, new()
    {
        ImportWorkflow.Register(new T());
        return services;
    }

    /// <summary>
    /// Registers a custom mapping strategy.
    /// </summary>
    /// <typeparam name="T">The type of the mapping strategy to register.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection, for method chaining.</returns>
    public static IServiceCollection AddMappingStrategy<T>(this IServiceCollection services)
        where T : MappingStrategy, new()
    {
        MappingStrategy.Register(new T());
        return services;
    }

    /// <summary>
    /// Registers a built-in validation rule.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection, for method chaining.</returns>
    public static IServiceCollection AddBuiltInValidationRules(this IServiceCollection services)
    {
        // Register built-in validation rules
        ValidationRule.Register(new RequiredValidationRule());
        ValidationRule.Register(new RangeValidationRule(0, 100)); // Example range

        return services;
    }

    /// <summary>
    /// Registers a custom validation rule.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="rule">The validation rule to register.</param>
    /// <returns>The service collection, for method chaining.</returns>
    public static IServiceCollection AddValidationRule(this IServiceCollection services, ValidationRule rule)
    {
        ValidationRule.Register(rule);
        return services;
    }
}