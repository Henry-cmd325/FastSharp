using FastSharp.Modules.Configuration;
using FastSharp.Modules.Logging;
using FastSharp.Modules.Registry;
using FluentValidation;
using Microsoft.Extensions.Logging.Abstractions;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace FastSharp.Modules;

/// <summary>Extension methods for registering and mapping FastSharp endpoints in an ASP.NET Core application.</summary>
public static class DependencyInjection
{
    private static readonly ConditionalWeakTable<IEndpointRouteBuilder, Dictionary<Assembly, AssemblyMappingState>> AssemblyMappings = new();
    private static readonly object MappingLock = new();

    private enum AssemblyMappingState
    {
        Started,
        Succeeded,
        Failed
    }
    /// <summary>
    /// Registers FastSharp modules, endpoints, and validators from the given assemblies,
    /// applying global FastSharp configuration such as the default GetList page size limit.
    /// </summary>
    /// <param name="services">The service collection to add registrations to.</param>
    /// <param name="configure">An action that configures the global <see cref="FastSharpOptions"/>.</param>
    /// <param name="assemblies">The assemblies to scan. Defaults to the calling assembly when none are provided.</param>
    public static IServiceCollection AddFastSharpEndpoints(this IServiceCollection services, Action<FastSharpOptions> configure, params Assembly[] assemblies)
    {
        if (assemblies.Length == 0)
        {
            assemblies = [Assembly.GetCallingAssembly()];
        }

        return AddFastSharpEndpoints(services, assemblies, configure);
    }

    /// <summary>
    /// Registers FastSharp modules, endpoints, and validators from the given assemblies
    /// using default <see cref="FastSharpOptions"/>.
    /// </summary>
    /// <param name="services">The service collection to add registrations to.</param>
    /// <param name="assemblies">The assemblies to scan. Defaults to the calling assembly when none are provided.</param>
    public static IServiceCollection AddFastSharpEndpoints(this IServiceCollection services, params Assembly[] assemblies)
    {
        if (assemblies.Length == 0)
        {
            assemblies = [Assembly.GetCallingAssembly()];
        }

        return AddFastSharpEndpoints(services, assemblies, configure: null);
    }

    private static IServiceCollection AddFastSharpEndpoints(
        IServiceCollection services,
        Assembly[] assemblies,
        Action<FastSharpOptions>? configure)
    {
        var requestedAssemblies = assemblies.Distinct().ToArray();
        var existingRegistration = services
            .Where(descriptor => descriptor.ServiceType == typeof(FastSharpAssemblyRegistration))
            .Select(descriptor => descriptor.ImplementationInstance as FastSharpAssemblyRegistration)
            .LastOrDefault(registration => registration is not null);
        var existingAssemblies = existingRegistration?.Assemblies ?? [];
        var newlyRegisteredAssemblies = requestedAssemblies.Except(existingAssemblies).ToArray();
        var registeredAssemblies = existingAssemblies.Concat(newlyRegisteredAssemblies).ToArray();

        var registries = newlyRegisteredAssemblies
            .Select(FastSharpAssemblyRegistryStore.GetRequiredRegistry)
            .ToArray();
        var stagedServices = new ServiceCollection();

        stagedServices.AddValidatorsFromAssemblies(newlyRegisteredAssemblies);
        foreach (var registry in registries)
        {
            registry.RegisterServices(stagedServices);
        }

        // The service collection is changed only after every registry has resolved and staged successfully.
        services.AddOptions();
        if (configure is not null)
        {
            services.Configure(configure);
        }

        foreach (var descriptor in stagedServices)
        {
            services.Add(descriptor);
        }

        foreach (var descriptor in services.Where(descriptor => descriptor.ServiceType == typeof(FastSharpAssemblyRegistration)).ToArray())
        {
            services.Remove(descriptor);
        }

        services.AddSingleton(new FastSharpAssemblyRegistration(registeredAssemblies));

        return services;
    }

    /// <summary>
    /// Maps all FastSharp modules and endpoints from the assemblies registered via <see cref="AddFastSharpEndpoints(IServiceCollection, Assembly[])"/>.
    /// Call this in your application's middleware pipeline after <c>app.Build()</c>. Mapping is one-shot per application and assembly;
    /// repeated successful calls are ignored, while a failed mapping cannot be retried on the same application.
    /// </summary>
    /// <param name="app">The application's endpoint route builder.</param>
    public static void MapFastSharpEndpoints(this IEndpointRouteBuilder app)
    {
        var registration = app.ServiceProvider.GetService<FastSharpAssemblyRegistration>();
        var assemblies = registration?.Assemblies.ToArray() ?? [Assembly.GetCallingAssembly()];
        MapFastSharpEndpoints(app, assemblies);
    }

    /// <summary>
    /// Maps all FastSharp modules and endpoints from the specified assemblies.
    /// Use this overload when you did not call <see cref="AddFastSharpEndpoints(IServiceCollection, Assembly[])"/> with assembly arguments.
    /// Mapping is one-shot per application and assembly; a failed mapping cannot be retried on the same application.
    /// </summary>
    /// <param name="app">The application's endpoint route builder.</param>
    /// <param name="assemblies">The assemblies whose modules and endpoints should be mapped.</param>
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

        lock (MappingLock)
        {
            var assemblyMappings = AssemblyMappings.GetOrCreateValue(app);
            foreach (var assembly in distinctAssemblies)
            {
                if (assemblyMappings.TryGetValue(assembly, out var state))
                {
                    if (state == AssemblyMappingState.Succeeded)
                    {
                        continue;
                    }

                    var diagnostic = state == AssemblyMappingState.Started
                        ? "is already in progress"
                        : "failed previously";
                    throw new InvalidOperationException(
                        $"FastSharp mapping for assembly '{assembly.GetName().Name}' {diagnostic}. " +
                        "Mapping is one-shot per application and cannot be retried after a failure.");
                }

                var assemblyName = assembly.GetName().Name ?? assembly.FullName ?? "unknown";
                FastSharpLogger.LogScanningAssembly(logger, assemblyName);

                assemblyMappings[assembly] = AssemblyMappingState.Started;
                try
                {
                    var registry = FastSharpAssemblyRegistryStore.GetRequiredRegistry(assembly);
                    registry.MapEndpoints(app);
                    assemblyMappings[assembly] = AssemblyMappingState.Succeeded;
                }
                catch
                {
                    assemblyMappings[assembly] = AssemblyMappingState.Failed;
                    throw;
                }
            }
        }

        FastSharpLogger.LogCompletedModuleScan(logger);
    }
}
