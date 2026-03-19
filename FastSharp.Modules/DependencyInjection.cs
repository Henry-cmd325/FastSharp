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

        foreach (var assembly in assemblies)
        {
            var typesToRegister = assembly.GetTypes()
                .Where(t => !t.IsAbstract && !t.IsInterface &&
                            (typeof(IEndpoint).IsAssignableFrom(t) ||
                             typeof(IFastModule).IsAssignableFrom(t)));

            foreach (var type in typesToRegister)
                services.AddTransient(type);
        }

        return services;
    }

    public static void MapFastSharpEndpoints(this IEndpointRouteBuilder app, params Assembly[] assemblies)
    {
        if (assemblies.Length == 0)
        {
            assemblies = [Assembly.GetCallingAssembly()];
        }

        foreach (var assembly in assemblies)
        {
            // Look for the main module types to start the mapping process.
            var moduleTypes = assembly.GetTypes()
                .Where(t => !t.IsAbstract &&
                            !t.IsInterface &&
                            typeof(IFastModule).IsAssignableFrom(t));

            foreach (var type in moduleTypes)
            {
                var module = (IFastModule)app.ServiceProvider.GetRequiredService(type);

                module.Map(app);
            }
        }
    }
}
