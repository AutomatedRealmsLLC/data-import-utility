using AutomatedRealms.DataImportUtility.Abstractions.Services;
using AutomatedRealms.DataImportUtility.Core.ComparisonOperations;
using AutomatedRealms.DataImportUtility.Core.Rules;
using AutomatedRealms.DataImportUtility.Core.Services;
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
            registry.RegisterType<CombineFieldsRule>(CombineFieldsRule.TypeIdString);
            registry.RegisterType<ConstantValueRule>(ConstantValueRule.TypeIdString);
            registry.RegisterType<CopyRule>(CopyRule.TypeIdString);
            registry.RegisterType<CustomFieldlessRule>(CustomFieldlessRule.TypeIdString);
            registry.RegisterType<FieldAccessRule>(FieldAccessRule.TypeIdString);
            registry.RegisterType<IgnoreRule>(IgnoreRule.TypeIdString);
            registry.RegisterType<StaticValueRule>(StaticValueRule.TypeIdString);

            // Register Comparison Operations
            registry.RegisterType<BetweenOperation>(BetweenOperation.TypeIdString);
            registry.RegisterType<ContainsOperation>(ContainsOperation.TypeIdString);
            registry.RegisterType<EndsWithOperation>(EndsWithOperation.TypeIdString);
            registry.RegisterType<EqualsOperation>(EqualsOperation.TypeIdString);
            registry.RegisterType<GreaterThanOperation>(GreaterThanOperation.TypeIdString);
            registry.RegisterType<GreaterThanOrEqualOperation>(GreaterThanOrEqualOperation.TypeIdString);
            registry.RegisterType<InOperation>(InOperation.TypeIdString);
            registry.RegisterType<IsFalseOperation>(IsFalseOperation.TypeIdString);
            registry.RegisterType<IsNotNullOperation>(IsNotNullOperation.TypeIdString);
            registry.RegisterType<IsNotNullOrEmptyOperation>(IsNotNullOrEmptyOperation.TypeIdString);
            registry.RegisterType<IsNotNullOrWhiteSpaceOperation>(IsNotNullOrWhiteSpaceOperation.TypeIdString);
            registry.RegisterType<IsNullOperation>(IsNullOperation.TypeIdString);
            registry.RegisterType<IsNullOrEmptyOperation>(IsNullOrEmptyOperation.TypeIdString);
            registry.RegisterType<IsNullOrWhiteSpaceOperation>(IsNullOrWhiteSpaceOperation.TypeIdString);
            registry.RegisterType<IsTrueOperation>(IsTrueOperation.TypeIdString);
            registry.RegisterType<LessThanOperation>(LessThanOperation.TypeIdString);
            registry.RegisterType<LessThanOrEqualOperation>(LessThanOrEqualOperation.TypeIdString);
            registry.RegisterType<NotBetweenOperation>(NotBetweenOperation.TypeIdString);
            registry.RegisterType<NotContainsOperation>(NotContainsOperation.TypeIdString);
            registry.RegisterType<NotEqualOperation>(NotEqualOperation.TypeIdString);
            registry.RegisterType<NotInOperation>(NotInOperation.TypeIdString);
            registry.RegisterType<RegexMatchOperation>(RegexMatchOperation.TypeIdString);
            registry.RegisterType<StartsWithOperation>(StartsWithOperation.TypeIdString);

            // Register Value Transformations
            registry.RegisterType<CalculateTransformation>(CalculateTransformation.TypeIdString);
            registry.RegisterType<CombineFieldsTransformation>(CombineFieldsTransformation.TypeIdString);
            registry.RegisterType<ConditionalTransformation>(ConditionalTransformation.TypeIdString);
            registry.RegisterType<InterpolateTransformation>(InterpolateTransformation.TypeIdString);
            registry.RegisterType<MapTransformation>(MapTransformation.TypeIdString);
            registry.RegisterType<RegexMatchTransformation>(RegexMatchTransformation.TypeIdString);
            registry.RegisterType<SubstringTransformation>(SubstringTransformation.TypeIdString);

            return registry;
        });

        return services;
    }
}
