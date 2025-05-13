// Filepath: d:\git\AutomatedRealms\data-import-utility\src\DataImportUtility\AutomatedRealms.DataImportUtility.Abstractions\Extensions\DataImportUtilityAbstractionsServiceExtensions.cs
using AutomatedRealms.DataImportUtility.Abstractions.Services;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace AutomatedRealms.DataImportUtility.Abstractions.Extensions;

/// <summary>
/// Provides extension methods for IServiceCollection to allow consumers to register
/// their custom implementations of Data Import Utility types.
/// </summary>
public static class DataImportUtilityAbstractionsServiceExtensions
{
    private static void RegisterCustomType<TBase, TImplementation>(
        IServiceCollection services, 
        string typeId, 
        string baseTypeNameForError)
        where TImplementation : TBase
    {
        if (string.IsNullOrWhiteSpace(typeId))
        {
            throw new ArgumentException($"TypeId cannot be null or whitespace when registering a custom {baseTypeNameForError}.", nameof(typeId));
        }

        var serviceProvider = services.BuildServiceProvider();
        var typeRegistry = serviceProvider.GetService<ITypeRegistryService>();

        if (typeRegistry == null)
        {
            throw new InvalidOperationException(
                $"ITypeRegistryService is not registered. Ensure AddCoreImportUtilityTypes() or a similar method that registers ITypeRegistryService has been called.");
        }

        typeRegistry.RegisterType<TImplementation>(typeId);
    }

    /// <summary>
    /// Registers a custom mapping rule implementation with the TypeRegistryService.
    /// </summary>
    /// <typeparam name="TImplementation">The concrete type of the mapping rule to register.</typeparam>
    /// <param name="services">The IServiceCollection.</param>
    /// <param name="typeId">The unique string identifier for this mapping rule type.</param>
    /// <returns>The IServiceCollection so that additional calls can be chained.</returns>
    /// <exception cref="ArgumentException">Thrown if typeId is null or whitespace.</exception>
    /// <exception cref="InvalidOperationException">Thrown if ITypeRegistryService is not registered or if the TypeId is already in use.</exception>
    public static IServiceCollection AddMappingRule<TImplementation>(this IServiceCollection services, string typeId)
        where TImplementation : MappingRuleBase
    {
        RegisterCustomType<MappingRuleBase, TImplementation>(services, typeId, "MappingRule");
        return services;
    }

    /// <summary>
    /// Registers a custom value transformation implementation with the TypeRegistryService.
    /// </summary>
    /// <typeparam name="TImplementation">The concrete type of the value transformation to register.</typeparam>
    /// <param name="services">The IServiceCollection.</param>
    /// <param name="typeId">The unique string identifier for this value transformation type.</param>
    /// <returns>The IServiceCollection so that additional calls can be chained.</returns>
    /// <exception cref="ArgumentException">Thrown if typeId is null or whitespace.</exception>
    /// <exception cref="InvalidOperationException">Thrown if ITypeRegistryService is not registered or if the TypeId is already in use.</exception>
    public static IServiceCollection AddValueTransformation<TImplementation>(this IServiceCollection services, string typeId)
        where TImplementation : ValueTransformationBase
    {
        RegisterCustomType<ValueTransformationBase, TImplementation>(services, typeId, "ValueTransformation");
        return services;
    }

    /// <summary>
    /// Registers a custom comparison operation implementation with the TypeRegistryService.
    /// </summary>
    /// <typeparam name="TImplementation">The concrete type of the comparison operation to register.</typeparam>
    /// <param name="services">The IServiceCollection.</param>
    /// <param name="typeId">The unique string identifier for this comparison operation type.</param>
    /// <returns>The IServiceCollection so that additional calls can be chained.</returns>
    /// <exception cref="ArgumentException">Thrown if typeId is null or whitespace.</exception>
    /// <exception cref="InvalidOperationException">Thrown if ITypeRegistryService is not registered or if the TypeId is already in use.</exception>
    public static IServiceCollection AddComparisonOperation<TImplementation>(this IServiceCollection services, string typeId)
        where TImplementation : ComparisonOperationBase
    {
        RegisterCustomType<ComparisonOperationBase, TImplementation>(services, typeId, "ComparisonOperation");
        return services;
    }
}
