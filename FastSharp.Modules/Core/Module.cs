using FastSharp.Models;
using FastSharp.Modules.Configuration;
using FastSharp.Modules.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using System.Linq.Expressions;

namespace FastSharp.Modules.Core;

/// <summary>
/// Base class for a FastSharp module. Defines a route group and composes custom <see cref="IEndpoint"/> implementations.
/// Inherit from this class when your module does not need Entity Framework CRUD generation.
/// For CRUD support, inherit from <see cref="Module{TDbContext}"/> instead.
/// </summary>
public abstract class Module : IFastModule
{
    /// <summary>The endpoint types registered via <see cref="Include{TEndpoint}"/>.</summary>
    protected readonly List<Type> _moduleEndpoints = [];
    /// <summary>Optional configuration action applied to the module's <see cref="RouteGroupBuilder"/> at map time.</summary>
    protected Action<RouteGroupBuilder>? _groupConfiguration;
    /// <summary>The URL prefix for this module's route group. Defaults to <c>"/api"</c>.</summary>
    protected string urlPrefix = "/api";

    // This is implemented explicitly so the method cannot be called directly by consumers,
    // and can only be invoked by the FastSharp engine when building the Minimal APIs.
    void IFastModule.Map(IEndpointRouteBuilder app)
    {
        MapEndpoints(app);
    }

    internal virtual void MapEndpoints(IEndpointRouteBuilder app)
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
    /// Configures the routing module with a specified URL prefix and shared endpoint conventions for the route group.
    /// </summary>
    /// <param name="prefix">The URL prefix to apply to the route group (use a leading slash, e.g. <c>"/api"</c>). This prefix is the base path for all routes defined within the module.</param>
    /// <param name="configure">An action that configures shared endpoint conventions for the route group, such as metadata,
    /// authorization policies, filters, and OpenAPI settings. Define custom routes in <see cref="IEndpoint.Map(RouteGroupBuilder)"/>.</param>
    protected void ConfigureModule(string prefix, Action<IEndpointConventionBuilder> configure)
    {
        urlPrefix = prefix;
        _groupConfiguration = group => configure(group);
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

/// <summary>
/// Base class for a FastSharp module with built-in CRUD endpoint generation backed by Entity Framework Core.
/// Inherit from this class to use <c>AddCRUD</c> and compose custom <see cref="IEndpoint"/> implementations
/// within the same route group.
/// </summary>
/// <typeparam name="TDbContext">The Entity Framework <see cref="DbContext"/> used to back generated CRUD endpoints.</typeparam>
public abstract class Module<TDbContext> : Module where TDbContext : DbContext
{
    private readonly List<ICrudEndpoints<TDbContext>> _crudOptionsList = [];

    internal override void MapEndpoints(IEndpointRouteBuilder app)
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
        var crudPrefix = urlPrefix.TrimEnd('/') + "/" + routePrefix.TrimStart('/');
        var options = new CRUDEndpoints<TDbContext, TEntity, TKey>(routePrefix, crudPrefix: crudPrefix);
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
        var crudPrefix = urlPrefix.TrimEnd('/') + "/" + routePrefix.TrimStart('/');
        var options = new CRUDEndpoints<TDbContext, TEntity, TKey>(routePrefix, idSelector, crudPrefix);
        configure?.Invoke(options);
        _crudOptionsList.Add(options);
    }
}
