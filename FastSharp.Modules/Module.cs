using FastSharp.Modules.Configuration;
using FastSharp.Models;
using Microsoft.EntityFrameworkCore;

namespace FastSharp.Modules
{
    public abstract class Module<TDbContext> : IFastModule where TDbContext : DbContext
    {
        private readonly List<Type> _moduleEndpoints = [];
        private readonly List<ICrudEndpoints<TDbContext>> _crudOptionsList = [];
        private Action<RouteGroupBuilder>? _groupConfiguration;
        private string urlPrefix = "/api";
        public void Map(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup(urlPrefix);

            _groupConfiguration?.Invoke(group);

            // Primero mapeamos los endpoints CRUD para que tengan prioridad sobre los endpoints personalizados
            foreach (var crudOptions in _crudOptionsList)
            {
                crudOptions.Map(group);
            }

            using var scope = app.ServiceProvider.CreateScope();
            var provider = scope.ServiceProvider;

            foreach (var endpointType in _moduleEndpoints)
            {
                var endpoint = (IEndpoint)provider.GetRequiredService(endpointType);
                endpoint.Map(group);
            }
        }

        protected void ConfigureGroup(string prefix, Action<RouteGroupBuilder> configure)
        {
            urlPrefix = prefix;
            _groupConfiguration = configure;
        } 

        protected void AddCRUD<TEntity, TKey>(string routePrefix, Action<ICrudEndpoints<TDbContext>>? configure = null) where TEntity : class, IModel<TKey>
        {
            var options = new CRUDEndpoints<TDbContext, TEntity, TKey>(routePrefix);
            configure?.Invoke(options);
            _crudOptionsList.Add(options);
        }

        protected void IncludeNamespace<T>() where T : IEndpoint
        {
            var ns = typeof(T).Namespace;
            // Escaneamos el ensamblado buscando clases que:
            // 1. Estén en el namespace indicado.
            // 2. Implementen IFastEndpoint.
            var targetAssembly = typeof(T).Assembly;
            var types = targetAssembly
                .GetTypes()
                .Where(p => typeof(IEndpoint).IsAssignableFrom(p)
                            && p.IsClass
                            && p.Namespace?.StartsWith(ns ?? string.Empty) == true
                            && !p.IsAbstract);

            // Guardamos el tipo para que el motor de FastSharp lo ejecute
            // al momento de construir las Minimal APIs
            _moduleEndpoints.AddRange(types);
        }

        protected void Include<TEndpoint>()
        {
            _moduleEndpoints.Add(typeof(TEndpoint));
        }
    }
}
