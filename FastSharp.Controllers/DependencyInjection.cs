using System.Reflection;

namespace FastSharp.Controllers
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddFastSharpEndpoints(this IServiceCollection services, params Assembly[] assemblies)
        {
            if (assemblies.Length == 0)
            {
                //Si no pasan ensamblados, usamos el que llamó a este método
                assemblies = [Assembly.GetCallingAssembly()];
            }

            foreach (var assembly in assemblies)
            {
                var typesToRegister = assembly.GetTypes()
                    .Where(t => !t.IsAbstract && !t.IsInterface &&
                                (typeof(IFastEndpoint).IsAssignableFrom(t) ||
                                 typeof(IFastController).IsAssignableFrom(t)));

                foreach (var type in typesToRegister)
                    services.AddScoped(type);
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
                // Buscamos solo los controladores principales para arrancar el mapeo
                var controllerTypes = assembly.GetTypes()
                    .Where(t => !t.IsAbstract &&
                                !t.IsInterface &&
                                typeof(IFastController).IsAssignableFrom(t));

                foreach (var type in controllerTypes)
                {
                    var controller = (IFastController)ActivatorUtilities.CreateInstance(app.ServiceProvider, type);

                    controller.Map(app);
                }
            }
        }
    }
}
