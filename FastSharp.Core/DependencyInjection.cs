using System.Reflection;

namespace FastSharp.Controllers
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddFastSharpEndpoints(this IServiceCollection services, Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                var endpointTypes = assembly.GetTypes()
                .Where(t => !t.IsAbstract &&
                            !t.IsInterface &&
                            typeof(IFastEndpoint).IsAssignableFrom(t));

                foreach (var type in endpointTypes)
                {
                    services.AddScoped(type);
                }
            }

            return services;
        }

        public static IServiceCollection AddFastSharpEndpoints(this IServiceCollection services)
        {
            var assembly = Assembly.GetCallingAssembly();
            var endpointTypes = assembly.GetTypes()
                .Where(t => !t.IsAbstract &&
                            !t.IsInterface &&
                            typeof(IFastEndpoint).IsAssignableFrom(t));

            foreach (var type in endpointTypes)
            {
                services.AddScoped(type);
            }

            return services;
        }

        public static void MapFastSharpEndpoints(this IEndpointRouteBuilder app, Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                var endpointTypes = assembly.GetTypes()
                .Where(t => !t.IsAbstract &&
                            !t.IsInterface &&
                            typeof(IFastEndpoint).IsAssignableFrom(t));

                foreach (var type in endpointTypes)
                {
                    var instance = ActivatorUtilities.CreateInstance(app.ServiceProvider, type);
                    var method = type.GetMethod("Map", [typeof(IEndpointRouteBuilder)]);
                    method?.Invoke(instance, [app]);
                }
            }
        }

        public static void MapFastSharpEndpoints(this IEndpointRouteBuilder app)
        {
            var assembly = Assembly.GetCallingAssembly();

            var endpointTypes = assembly.GetTypes()
                .Where(t => !t.IsAbstract &&
                            !t.IsInterface &&
                            typeof(IFastEndpoint).IsAssignableFrom(t));

            foreach (var type in endpointTypes)
            {
                var instance = ActivatorUtilities.CreateInstance(app.ServiceProvider, type);
                var method = type.GetMethod("Map", [typeof(IEndpointRouteBuilder)]);
                method?.Invoke(instance, [app]);
            }
        }
    }
}
