using AutomatedRealms.DataImportUtility.Abstractions.Services;

using Microsoft.Extensions.DependencyInjection;

namespace AutomatedRealms.DataImportUtility.Abstractions.Extensions;

/// <summary>
/// Provides extension methods for IServiceCollection to allow consumers to register
/// their custom implementations of Data Import Utility types.
/// </summary>
public static class DataImportUtilityAbstractionsServiceExtensions
{
    private static IServiceCollection RegisterCustomType<TBase, TImplementation>(
        this IServiceCollection services,
        string typeId,
        string baseTypeNameForError)
        where TImplementation : TBase
    {
        if (string.IsNullOrWhiteSpace(typeId))
        {
            throw new ArgumentException($"TypeId cannot be null or whitespace when registering a custom {baseTypeNameForError}.", nameof(typeId));
        }

        var serviceProvider = services.BuildServiceProvider();
        var typeRegistry = serviceProvider.GetRequiredService<ITypeRegistryService>();

        typeRegistry.RegisterType<TImplementation>(typeId);

        return services;
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
        return services.RegisterCustomType<MappingRuleBase, TImplementation>(typeId, nameof(MappingRuleBase));
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
        return services.RegisterCustomType<ValueTransformationBase, TImplementation>(typeId, nameof(ValueTransformationBase));
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
        return services.RegisterCustomType<ComparisonOperationBase, TImplementation>(typeId, nameof(ComparisonOperationBase));
    }
}
