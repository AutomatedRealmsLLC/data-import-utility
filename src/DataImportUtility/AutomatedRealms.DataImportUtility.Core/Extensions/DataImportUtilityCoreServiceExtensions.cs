// Filepath: d:\git\AutomatedRealms\data-import-utility\src\DataImportUtility\AutomatedRealms.DataImportUtility.Core\Extensions\DataImportUtilityCoreServiceExtensions.cs
using AutomatedRealms.DataImportUtility.Abstractions.Services;
using AutomatedRealms.DataImportUtility.Core.Services;
using AutomatedRealms.DataImportUtility.Core.Rules;
using AutomatedRealms.DataImportUtility.Core.ComparisonOperations;
using AutomatedRealms.DataImportUtility.Core.ValueTransformations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AutomatedRealms.DataImportUtility.Core.Extensions;

/// <summary>
/// Provides extension methods for IServiceCollection to register core services and types
/// for the Data Import Utility.
/// </summary>
public static class DataImportUtilityCoreServiceExtensions
{
    /// <summary>
    /// Adds core Data Import Utility types to the service collection and registers them
    /// with the ITypeRegistryService.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <returns>The IServiceCollection so that additional calls can be chained.</returns>
    public static IServiceCollection AddCoreImportUtilityTypes(this IServiceCollection services)
    {
        // Register TypeRegistryService if not already registered, and pre-populate it with core types.
        services.TryAddSingleton<ITypeRegistryService>(sp =>
        {
            var registry = new TypeRegistryService();

            // Register Mapping Rules
            registry.RegisterType<CombineFieldsRule>("Core.Rule.CombineFieldsRule");
            registry.RegisterType<ConstantValueRule>("Core.Rule.ConstantValueRule");
            registry.RegisterType<CopyRule>("Core.Rule.CopyRule");
            registry.RegisterType<CustomFieldlessRule>("Core.Rule.CustomFieldlessRule");
            registry.RegisterType<FieldAccessRule>("Core.Rule.FieldAccessRule");
            registry.RegisterType<IgnoreRule>("Core.Rule.IgnoreRule");
            registry.RegisterType<StaticValueRule>("Core.Rule.StaticValueRule");

            // Register Comparison Operations
            registry.RegisterType<BetweenOperation>("Core.Operation.BetweenOperation");
            registry.RegisterType<ContainsOperation>("Core.Operation.ContainsOperation");
            registry.RegisterType<EndsWithOperation>("Core.Operation.EndsWithOperation");
            registry.RegisterType<EqualsOperation>("Core.Operation.EqualsOperation");
            registry.RegisterType<GreaterThanOperation>("Core.Operation.GreaterThanOperation");
            registry.RegisterType<GreaterThanOrEqualOperation>("Core.Operation.GreaterThanOrEqualOperation");
            registry.RegisterType<InOperation>("Core.Operation.InOperation");
            registry.RegisterType<IsFalseOperation>("Core.Operation.IsFalseOperation");
            registry.RegisterType<IsNotNullOperation>("Core.Operation.IsNotNullOperation");
            registry.RegisterType<IsNotNullOrEmptyOperation>("Core.Operation.IsNotNullOrEmptyOperation");
            registry.RegisterType<IsNotNullOrWhiteSpaceOperation>("Core.Operation.IsNotNullOrWhiteSpaceOperation");
            registry.RegisterType<IsNullOperation>("Core.Operation.IsNullOperation");
            registry.RegisterType<IsNullOrEmptyOperation>("Core.Operation.IsNullOrEmptyOperation");
            registry.RegisterType<IsNullOrWhiteSpaceOperation>("Core.Operation.IsNullOrWhiteSpaceOperation");
            registry.RegisterType<IsTrueOperation>("Core.Operation.IsTrueOperation");
            registry.RegisterType<LessThanOperation>("Core.Operation.LessThanOperation");
            registry.RegisterType<LessThanOrEqualOperation>("Core.Operation.LessThanOrEqualOperation");
            registry.RegisterType<NotBetweenOperation>("Core.Operation.NotBetweenOperation");
            registry.RegisterType<NotContainsOperation>("Core.Operation.NotContainsOperation");
            registry.RegisterType<NotEqualOperation>("Core.Operation.NotEqualOperation");
            registry.RegisterType<NotInOperation>("Core.Operation.NotInOperation");
            registry.RegisterType<RegexMatchOperation>("Core.Operation.RegexMatchOperation");
            registry.RegisterType<StartsWithOperation>("Core.Operation.StartsWithOperation");

            // Register Value Transformations
            registry.RegisterType<CalculateTransformation>("Core.Transform.CalculateTransformation");
            registry.RegisterType<CombineFieldsTransformation>("Core.Transform.CombineFieldsTransformation");
            registry.RegisterType<ConditionalTransformation>("Core.Transform.ConditionalTransformation");
            registry.RegisterType<InterpolateTransformation>("Core.Transform.InterpolateTransformation");
            registry.RegisterType<MapTransformation>("Core.Transform.MapTransformation");
            registry.RegisterType<RegexMatchTransformation>("Core.Transform.RegexMatchTransformation");
            registry.RegisterType<SubstringTransformation>("Core.Transform.SubstringTransformation");
            
            return registry;
        });

        return services;
    }
}
