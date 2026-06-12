using FastSharp.Modules.Configuration;
using FastSharp.Modules.Logging;
using FastSharp.Modules.Registry;
using FluentValidation;
using Microsoft.Extensions.Logging.Abstractions;
using System.Reflection;

namespace FastSharp.Modules;

public static class DependencyInjection
{
    /// <summary>
    /// Registers FastSharp modules, endpoints, and validators from the given assemblies,
    /// applying global FastSharp configuration such as the default GetList page size limit.
    /// </summary>
    /// <param name="services">The service collection to add registrations to.</param>
    /// <param name="configure">An action that configures the global <see cref="FastSharpOptions"/>.</param>
    /// <param name="assemblies">The assemblies to scan. Defaults to the calling assembly when none are provided.</param>
    public static IServiceCollection AddFastSharpEndpoints(this IServiceCollection services, Action<FastSharpOptions> configure, params Assembly[] assemblies)
    {
        services.Configure(configure);
        return services.AddFastSharpEndpoints(assemblies);
    }

    public static IServiceCollection AddFastSharpEndpoints(this IServiceCollection services, params Assembly[] assemblies)
    {
        // Ensure IOptions<FastSharpOptions> always resolves (default values) even when the
        // configuring overload above is not used.
        services.AddOptions();

        if (assemblies.Length == 0)
        {
            // If no assemblies are provided, use the calling assembly.
            assemblies = [Assembly.GetCallingAssembly()];
        }

        var registeredAssemblies = assemblies.Distinct().ToArray();
        services.AddSingleton(new FastSharpAssemblyRegistration(registeredAssemblies));

        services.AddValidatorsFromAssemblies(registeredAssemblies);
        foreach (var assembly in registeredAssemblies)
        {
            var registry = FastSharpAssemblyRegistryStore.GetRequiredRegistry(assembly);
            registry.RegisterServices(services);

        }

        return services;
    }

    public static void MapFastSharpEndpoints(this IEndpointRouteBuilder app)
    {
        var registration = app.ServiceProvider.GetService<FastSharpAssemblyRegistration>();
        var assemblies = registration?.Assemblies.ToArray() ?? [Assembly.GetCallingAssembly()];
        MapFastSharpEndpoints(app, assemblies);
    }

    public static void MapFastSharpEndpoints(this IEndpointRouteBuilder app, params Assembly[] assemblies)
    {
        if (assemblies.Length == 0)
        {
            assemblies = [Assembly.GetCallingAssembly()];
        }

        var distinctAssemblies = assemblies.Distinct().ToArray();
        var logger = app.ServiceProvider.GetService<ILoggerFactory>()?.CreateLogger(FastSharpLogger.CategoryName)
            ?? NullLogger.Instance;

        FastSharpLogger.LogStartingModuleScan(logger, distinctAssemblies.Length);

        foreach (var assembly in distinctAssemblies)
        {
            var assemblyName = assembly.GetName().Name ?? assembly.FullName ?? "unknown";
            FastSharpLogger.LogScanningAssembly(logger, assemblyName);

            var registry = FastSharpAssemblyRegistryStore.GetRequiredRegistry(assembly);
            registry.MapEndpoints(app);
        }

        FastSharpLogger.LogCompletedModuleScan(logger);
    }
}
