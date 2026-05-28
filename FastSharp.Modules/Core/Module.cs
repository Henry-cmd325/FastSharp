using FastSharp.Models;
using FastSharp.Modules.Configuration;
using FastSharp.Modules.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using System.Linq.Expressions;

namespace FastSharp.Modules.Core;

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
        var logger = app.ServiceProvider.GetService<ILoggerFactory>()?.CreateLogger(FastSharpLogger.CategoryName)
            ?? NullLogger.Instance;

        FastSharpLogger.LogMappingModule(logger, GetType().Name, urlPrefix);

        var group = app.MapGroup(urlPrefix);
        _groupConfiguration?.Invoke(group);
        using var scope = app.ServiceProvider.CreateScope();
        var provider = scope.ServiceProvider;
        foreach (var endpointType in _moduleEndpoints)
        {
            FastSharpLogger.LogMappingCustomEndpoint(logger, endpointType.Name, urlPrefix);
            var endpoint = (IEndpoint)provider.GetRequiredService(endpointType);
            endpoint.Map(group);
        }
    }

    /// <summary>
    /// Configures the routing module with a specified URL prefix and a configuration action for the route group.
    /// </summary>
    /// <param name="prefix">The URL prefix to apply to the route group (use a leading slash, e.g. <c>"/api"</c>). This prefix is the base path for all routes defined within the module.</param>
    /// <param name="configure">An action that configures the route group. Use this action to define routes and additional settings within
    /// the specified prefix.</param>
    protected void ConfigureModule(string prefix, Action<RouteGroupBuilder> configure)
    {
        urlPrefix = prefix;
        _groupConfiguration = configure;
    }

    /// <summary>
    /// Adds the specified endpoint to the module's collection of recognized endpoints.
    /// </summary>
    /// <remarks>
    /// Call this method to register an additional endpoint with the module.
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
        var logger = app.ServiceProvider.GetService<ILoggerFactory>()?.CreateLogger(FastSharpLogger.CategoryName)
            ?? NullLogger.Instance;

        FastSharpLogger.LogMappingModule(logger, GetType().Name, urlPrefix);

        var group = app.MapGroup(urlPrefix);

        _groupConfiguration?.Invoke(group);

        foreach (var crudOptions in _crudOptionsList)
        {
            crudOptions.Map(group, logger, urlPrefix);
        }

        using var scope = app.ServiceProvider.CreateScope();
        var provider = scope.ServiceProvider;

        foreach (var endpointType in _moduleEndpoints)
        {
            FastSharpLogger.LogMappingCustomEndpoint(logger, endpointType.Name, urlPrefix);
            var endpoint = (IEndpoint)provider.GetRequiredService(endpointType);
            endpoint.Map(group);
        }
    }

    /// <summary>
    /// Adds a set of CRUD endpoints for a specific entity.
    /// Each endpoint (Get, GetList, Create, Update, Delete) can be configured individually,
    /// or a shared configuration can be applied to the entire CRUD group.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TKey">The entity primary key type.</typeparam>
    /// <param name="routePrefix">The route prefix for the CRUD endpoints (use a leading slash, e.g. <c>"/products"</c> or <c>"/products/v2"</c>).</param>
    /// <param name="configure">An action that configures the CRUD endpoints.</param>
    protected void AddCRUD<TEntity, TKey>(string routePrefix, Action<ICrudEndpoints<TDbContext>>? configure = null) where TEntity : class, IModel<TKey>
    {
        var options = new CRUDEndpoints<TDbContext, TEntity, TKey>(routePrefix);
        configure?.Invoke(options);
        _crudOptionsList.Add(options);
    }

    /// <summary>
    /// Adds a set of CRUD endpoints for a specific entity, with a custom primary key selector.
    /// Each endpoint (Get, GetList, Create, Update, Delete) can be configured individually,
    /// or a shared configuration can be applied to the entire CRUD group.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="routePrefix">The route prefix for the CRUD endpoints (use a leading slash, e.g. <c>"/products"</c>).</param>
    /// <param name="idSelector">Expression that selects the entity's primary key.</param>
    /// <param name="configure">An action that configures the CRUD endpoints.</param>
    protected void AddCRUD<TEntity, TKey>(
    string routePrefix,
    Expression<Func<TEntity, TKey>> idSelector,
    Action<ICrudEndpoints<TDbContext>>? configure = null) where TEntity : class
    {
        var options = new CRUDEndpoints<TDbContext, TEntity, TKey>(routePrefix, idSelector);
        configure?.Invoke(options);
        _crudOptionsList.Add(options);
    }
}
