using System.Reflection;

namespace FastSharp.Modules
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
                // Buscamos solo los controladores principales para arrancar el mapeo
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
}
