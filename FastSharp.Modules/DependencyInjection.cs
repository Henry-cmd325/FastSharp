using FastSharp.Modules.Registry;
using System.Reflection;

namespace FastSharp.Modules;

public static class DependencyInjection
{
    public static IServiceCollection AddFastSharpEndpoints(this IServiceCollection services, params Assembly[] assemblies)
    {
        if (assemblies.Length == 0)
        {
            // If no assemblies are provided, use the calling assembly.
            assemblies = [Assembly.GetCallingAssembly()];
        }

        var registeredAssemblies = assemblies.Distinct().ToArray();
        services.AddSingleton(new FastSharpAssemblyRegistration(registeredAssemblies));

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

        foreach (var assembly in assemblies)
        {
            var registry = FastSharpAssemblyRegistryStore.GetRequiredRegistry(assembly);
            registry.MapEndpoints(app);
        }
    }
}
