using AutomatedRealms.DataImportUtility.DataReader.Services;

using Microsoft.Extensions.DependencyInjection;

namespace AutomatedRealms.DataImportUtility.Components.Extensions;

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

    /// <summary>
    /// Adds the data file mapper options to the DI container.
    /// </summary>
    /// <param name="services">
    /// The <see cref="IServiceCollection" /> to add the services to.
    /// </param>
    /// <param name="configureOptions">
    /// The action to configure the <see cref="DataFileMapperUiOptions" />.
    /// </param>
    /// <returns>
    /// The <see cref="IServiceCollection" /> with the options added.
    /// </returns>
    public static IServiceCollection ConfigureDataFileMapperUiOptions(this IServiceCollection services, Action<DataFileMapperUiOptions> configureOptions)
    {
        var options = new DataFileMapperUiOptions();
        configureOptions(options);
        services.AddSingleton(options);
        return services;
    }
}
