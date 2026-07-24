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
    private readonly ModuleConfiguration _configuration = new();
    private bool _isConfigured;

    // Set only while AddRoutes composes the route group. Constructor registrations are queued;
    // registrations made through this context map immediately on the active route group.
    internal ModuleRouteComposition? activeRouteComposition;

    // This is implemented explicitly so the method cannot be called directly by consumers,
    // and can only be invoked by the FastSharp engine when building the Minimal APIs.
    void IFastModule.Map(IEndpointRouteBuilder app)
    {
        MapEndpoints(app);
    }

    internal virtual void MapEndpoints(IEndpointRouteBuilder app)
    {
        InitializeConfiguration();

        var logger = app.ServiceProvider.GetService<ILoggerFactory>()?.CreateLogger(FastSharpLogger.CategoryName)
            ?? NullLogger.Instance;

        FastSharpLogger.LogMappingModule(logger, GetType().Name, urlPrefix);

        var group = app.MapGroup(urlPrefix);
        _groupConfiguration?.Invoke(group);
        using var scope = app.ServiceProvider.CreateScope();
        var provider = scope.ServiceProvider;
        MapConstructorRoutes(group, logger, provider);
        ComposeRoutes(group, logger, provider);
    }

    /// <summary>
    /// Configures the module route prefix and conventions before its route group is created.
    /// Override this method for new modules. Constructor-based <see cref="ConfigureModule(string, Action{IEndpointConventionBuilder})"/>
    /// configuration remains supported for backward compatibility.
    /// </summary>
    /// <param name="configuration">The module prefix and shared route group conventions.</param>
    protected virtual void Configure(ModuleConfiguration configuration)
    {
    }

    /// <summary>
    /// Composes the module's routes after shared conventions have been applied to its route group.
    /// Queued constructor registrations map before this method is invoked.
    /// Override this method to map new routes with <c>AddCRUD</c>, <see cref="Include{TEndpoint}"/>,
    /// and native Minimal API mapping methods.
    /// </summary>
    /// <param name="routes">The route group scoped to this module.</param>
    protected virtual void AddRoutes(RouteGroupBuilder routes)
    {
    }

    internal virtual void MapConstructorRoutes(RouteGroupBuilder group, ILogger logger, IServiceProvider provider)
    {
        foreach (var endpointType in _moduleEndpoints)
        {
            FastSharpLogger.LogMappingCustomEndpoint(logger, endpointType.Name, urlPrefix);
            var endpoint = (IEndpoint)provider.GetRequiredService(endpointType);
            endpoint.Map(group);
        }
    }

    private void InitializeConfiguration()
    {
        if (_isConfigured)
        {
            return;
        }

        Configure(_configuration);
        urlPrefix = _configuration.Prefix;
        if (_groupConfiguration is null && _configuration.Conventions is not null)
        {
            _groupConfiguration = group => _configuration.Conventions(group);
        }
        _isConfigured = true;
    }

    private void ComposeRoutes(RouteGroupBuilder group, ILogger logger, IServiceProvider provider)
    {
        activeRouteComposition = new ModuleRouteComposition(group, logger, provider);

        try
        {
            AddRoutes(group);
        }
        finally
        {
            activeRouteComposition = null;
        }
    }

    /// <summary>
    /// Configures the routing module with a specified URL prefix and shared endpoint conventions for the route group.
    /// </summary>
    /// <param name="prefix">The URL prefix to apply to the route group (use a leading slash, e.g. <c>"/api"</c>). This prefix is the base path for all routes defined within the module.</param>
    /// <param name="configure">An action that configures shared endpoint conventions for the route group, such as metadata,
    /// authorization policies, filters, and OpenAPI settings. This legacy constructor-style API remains supported;
    /// prefer overriding <see cref="Configure(ModuleConfiguration)"/> and <see cref="AddRoutes(RouteGroupBuilder)"/> in new modules.</param>
    protected void ConfigureModule(string prefix, Action<IEndpointConventionBuilder> configure)
    {
        urlPrefix = prefix;
        _configuration.Prefix = prefix;
        _groupConfiguration = group => configure(group);
    }

    /// <summary>
    /// Adds the specified endpoint to the module.
    /// </summary>
    /// <remarks>
    /// Calls made from a constructor are queued and mapped after queued CRUD registrations.
    /// Calls made inside <see cref="AddRoutes(RouteGroupBuilder)"/> map immediately on the active module route group.
    /// </remarks>
    /// <typeparam name="TEndpoint">The type of the endpoint to include. Must implement the IEndpoint interface.</typeparam>
    protected void Include<TEndpoint>() where TEndpoint : IEndpoint
    {
        if (TryMapActiveRoute(composition =>
        {
            var endpoint = (IEndpoint)composition.ServiceProvider
                .GetRequiredService(typeof(TEndpoint));
            FastSharpLogger.LogMappingCustomEndpoint(composition.Logger, typeof(TEndpoint).Name, urlPrefix);
            endpoint.Map(composition.Routes);
        }))
        {
            return;
        }

        _moduleEndpoints.Add(typeof(TEndpoint));
    }

    internal bool TryMapActiveRoute(Action<ModuleRouteComposition> map)
    {
        if (activeRouteComposition is null)
        {
            return false;
        }

        map(activeRouteComposition);
        return true;
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

    internal override void MapConstructorRoutes(RouteGroupBuilder group, ILogger logger, IServiceProvider provider)
    {
        foreach (var crudOptions in _crudOptionsList)
        {
            crudOptions.Map(group, logger, urlPrefix);
        }

        base.MapConstructorRoutes(group, logger, provider);
    }

    /// <summary>
    /// Adds a set of CRUD endpoints for a specific entity.
    /// Each endpoint (Get, GetList, Create, Update, Delete) can be configured individually,
    /// or a shared configuration can be applied to the entire CRUD group. Calls made from a constructor are queued;
    /// calls made inside <c>AddRoutes</c> map immediately on the active module route group.
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
        MapOrStoreCrud(options);
    }

    /// <summary>
    /// Adds a set of CRUD endpoints for a specific entity, with a custom primary key selector.
    /// Each endpoint (Get, GetList, Create, Update, Delete) can be configured individually,
    /// or a shared configuration can be applied to the entire CRUD group. Calls made from a constructor are queued;
    /// calls made inside <c>AddRoutes</c> map immediately on the active module route group.
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
        MapOrStoreCrud(options);
    }

    private void MapOrStoreCrud(ICrudEndpoints<TDbContext> options)
    {
        if (TryMapActiveRoute(composition =>
        {
            options.Map(composition.Routes, composition.Logger, urlPrefix);
        }))
        {
            return;
        }

        _crudOptionsList.Add(options);
    }
}

internal sealed class ModuleRouteComposition(RouteGroupBuilder routes, ILogger logger, IServiceProvider serviceProvider)
{
    public RouteGroupBuilder Routes { get; } = routes;

    public ILogger Logger { get; } = logger;

    public IServiceProvider ServiceProvider { get; } = serviceProvider;
}
