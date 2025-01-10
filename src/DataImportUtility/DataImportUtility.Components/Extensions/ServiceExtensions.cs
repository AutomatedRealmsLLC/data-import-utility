using Microsoft.Extensions.DependencyInjection;

using DataImportUtility.Abstractions;
using DataImportUtility.Components.Services;

namespace DataImportUtility.Components.Extensions;

/// <summary>
/// Extension methods for adding services to the DI container.
/// </summary>
public static class ServiceExtensions
{
    /// <summary>
    /// Adds the data reader services to the DI container.
    /// </summary>
    /// <param name="services">
    /// The <see cref="IServiceCollection" /> to add the services to.
    /// </param>
    /// <returns>
    /// The <see cref="IServiceCollection" /> with the services added.
    /// </returns>
    public static IServiceCollection AddDataReaderServices(this IServiceCollection services)
        => services.AddSingleton<IDataReaderService, DataReaderService>();
}
