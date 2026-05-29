using FastSharp.Models;
using FastSharp.Modules.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FastSharp.Modules;

public abstract class Module : IFastModule
{
    protected readonly List<Type> _moduleEndpoints = [];
    protected Action<RouteGroupBuilder>? _groupConfiguration;
    protected string urlPrefix = "/api";
    
    // This is implemented explicitly so the method cannot be called directly by consumers,
    // and can only be invoked by the FastSharp engine when building the Minimal APIs.
    void IFastModule.Map(IEndpointRouteBuilder app)
    {
        MapEndpoints(app);
    }

    protected virtual void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(urlPrefix);
        _groupConfiguration?.Invoke(group);
        using var scope = app.ServiceProvider.CreateScope();
        var provider = scope.ServiceProvider;
        foreach (var endpointType in _moduleEndpoints)
        {
            var endpoint = (IEndpoint)provider.GetRequiredService(endpointType);
            endpoint.Map(group);
        }
    }

    /// <summary>
    /// Configures the routing module with a specified URL prefix and a configuration action for the route group.
    /// </summary>
    /// <param name="prefix">The URL prefix to apply to the route group. This prefix is used as the base path for all routes defined
    /// within the module.</param>
    /// <param name="configure">An action that configures the route group. Use this action to define routes and additional settings within
    /// the specified prefix.</param>
    protected void ConfigureModule(string prefix, Action<RouteGroupBuilder> configure)
    {
        urlPrefix = prefix;
        _groupConfiguration = configure;
    }

    /// <summary>
    /// Registers all non-abstract classes in the namespace of the specified type that implement the IEndpoint
    /// interface for use in Minimal APIs.
    /// </summary>
    /// <remarks>This method scans the assembly of the specified type T to find all non-abstract
    /// classes within the same namespace that implement IEndpoint. The discovered types are added to the module
    /// endpoints collection, making them available for execution by the FastSharp engine when constructing Minimal
    /// APIs.</remarks>
    /// <typeparam name="T">The type whose namespace will be scanned for IEndpoint implementations. Must implement IEndpoint.</typeparam>
    protected void IncludeNamespace<T>() where T : IEndpoint
    {
        var ns = typeof(T).Namespace;
        // Scan the assembly for classes that:
        // 1. Are in the target namespace.
        // 2. Implement IEndpoint.
        var targetAssembly = typeof(T).Assembly;
        var types = targetAssembly
            .GetTypes()
            .Where(p => typeof(IEndpoint).IsAssignableFrom(p)
                        && p.IsClass
                        && p.Namespace?.StartsWith(ns ?? string.Empty) == true
                        && !p.IsAbstract);

        // Store the type so the FastSharp engine can execute it
        // when building the Minimal APIs.
        _moduleEndpoints.AddRange(types);
    }

    /// <summary>
    /// Adds the specified endpoint to the module's collection of recognized endpoints.
    /// </summary>
    /// <remarks>Call this method to register an additional endpoint with the module.
    /// Try to always use this method instead of IncludeNamespace when possible, as it is more explicit and makes the module's behavior clearer.
    /// </remarks>
    /// <typeparam name="TEndpoint">The type of the endpoint to include. Must implement the IEndpoint interface.</typeparam>
    protected void Include<TEndpoint>() where TEndpoint : IEndpoint
    {
        _moduleEndpoints.Add(typeof(TEndpoint));
    }
}

//Module with built-in support for CRUD endpoints based on Entity Framework Core DbContext.
public abstract class Module<TDbContext> : Module where TDbContext : DbContext
{
    private readonly List<ICrudEndpoints<TDbContext>> _crudOptionsList = [];

    protected override void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(urlPrefix);

        _groupConfiguration?.Invoke(group);

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

    /// <summary>
    /// Adds a set of CRUD endpoints for a specific entity.
    /// Each endpoint (Get, GetList, GetPaged, Create, Update, Delete) can be configured individually,
    /// or a shared configuration can be applied to the entire CRUD group.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TKey">The entity primary key type.</typeparam>
    /// <param name="routePrefix">The route prefix for the CRUD endpoints.</param>
    /// <param name="configure">An action that configures the CRUD endpoints.</param>
    protected void AddCRUD<TEntity, TKey>(string routePrefix, Action<ICrudEndpoints<TDbContext>>? configure = null) where TEntity : class, IModel<TKey>
    {
        var crudPrefix = urlPrefix.TrimEnd('/') + "/" + routePrefix.TrimStart('/');
        var options = new CRUDEndpoints<TDbContext, TEntity, TKey>(routePrefix, crudPrefix: crudPrefix);
        configure?.Invoke(options);
        _crudOptionsList.Add(options);
    }

    /// <summary>
    /// Adds a set of CRUD endpoints for a specific entity, with a custom primary key selector.
    /// Each endpoint (Get, GetList, GetPaged, Create, Update, Delete) can be configured individually,
    /// or a shared configuration can be applied to the entire CRUD group.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="routePrefix"></param>
    /// <param name="idSelector"></param>
    /// <param name="configure"></param>
    protected void AddCRUD<TEntity, TKey>(
    string routePrefix,
    Expression<Func<TEntity, TKey>> idSelector,
    Action<ICrudEndpoints<TDbContext>>? configure = null) where TEntity : class
    {
        var crudPrefix = urlPrefix.TrimEnd('/') + "/" + routePrefix.TrimStart('/');
        var options = new CRUDEndpoints<TDbContext, TEntity, TKey>(routePrefix, idSelector, crudPrefix);
        configure?.Invoke(options);
        _crudOptionsList.Add(options);
    }
}
