using FastSharp.Models;
using FastSharp.Modules.Core.Endpoints;
using FastSharp.Modules.Logging;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FastSharp.Modules.Configuration;

/// <summary>
/// Internal implementation of <see cref="ICrudEndpoints{TDbContext}"/> created by FastSharp when you call <c>AddCRUD</c>.
/// Configure this via the delegate passed to <c>AddCRUD</c> — do not instantiate directly.
/// </summary>
public class CRUDEndpoints<TDbContext, TEntity, TKey> : ICrudEndpoints<TDbContext>
    where TEntity : class
    where TDbContext : DbContext
{
    internal Action<RouteGroupBuilder>? ConfigGroup;

    internal EndpointOptions ConfigAll = new();

    // Per-endpoint GetList max page size override. Null means use the global FastSharpOptions value.
    internal int? ListMaxPageSize;

    internal IGenericEndpoint GetByIdEndpoint;
    internal IGenericEndpoint GetListEndpoint;
    internal IGenericEndpoint CreateEndpoint;
    internal IGenericEndpoint UpdateEndpoint;
    internal IGenericEndpoint DeleteEndpoint;

    internal string RoutePrefix { get; set; }
    internal string CrudPrefix { get; set; }
    internal Func<TKey, Expression<Func<TEntity, bool>>> PredicateFactory;
    internal Expression<Func<TEntity, TKey>> IdSelector;

    /// <summary>
    /// Initializes a new <see cref="CRUDEndpoints{TDbContext, TEntity, TKey}"/> instance.
    /// If <paramref name="idSelector"/> is omitted, <typeparamref name="TEntity"/> must implement <see cref="IModel{TKey}"/>.
    /// </summary>
    /// <param name="routePrefix">The route prefix appended to the module's base path.</param>
    /// <param name="idSelector">Expression that selects the entity's primary key. Required when the entity does not implement <see cref="IModel{TKey}"/>.</param>
    /// <param name="crudPrefix">The fully combined route prefix used for location headers. Defaults to <paramref name="routePrefix"/>.</param>
    public CRUDEndpoints(string routePrefix = "", Expression<Func<TEntity, TKey>>? idSelector = null, string? crudPrefix = null)
    {
        RoutePrefix = routePrefix;
        CrudPrefix = crudPrefix ?? routePrefix;

        // If no selector is provided, check if the entity implements IModel
        if (idSelector == null && typeof(IModel<TKey>).IsAssignableFrom(typeof(TEntity)))
            IdSelector = e => ((IModel<TKey>)e).Id;
        else
            IdSelector = idSelector ?? throw new ArgumentException("You must provide an ID selector or implement IModel");

        // Compile the predicate creation logic once at startup
        var parameter = IdSelector.Parameters[0];
        var left = IdSelector.Body;

        PredicateFactory = (id) =>
        {
            var right = Expression.Constant(id, typeof(TKey));
            var comparison = Expression.Equal(left, right);
            return Expression.Lambda<Func<TEntity, bool>>(comparison, parameter);
        };

        GetByIdEndpoint = new GetByIdEndpoint<TDbContext, TEntity, TKey>(PredicateFactory);
        GetListEndpoint = new GetListEndpoint<TDbContext, TEntity, TKey>(IdSelector);
        CreateEndpoint = new CreateEndpoint<TDbContext, TEntity, TKey>(CrudPrefix);
        UpdateEndpoint = new UpdateEndpoint<TDbContext, TEntity, TKey>(PredicateFactory, IdSelector);
        DeleteEndpoint = new DeleteEndpoint<TDbContext, TEntity, TKey>(PredicateFactory);
    }

    private static EndpointOptions CreateEndpointOptions(Action<RouteHandlerBuilder>? configure = null, bool active = true) =>
        new() { Builder = configure, Active = active };

    private static void ConfigureEndpoint(IGenericEndpoint endpoint, Action<RouteHandlerBuilder>? configure = null, bool active = true) =>
        endpoint.Configure(CRUDEndpoints<TDbContext, TEntity, TKey>.CreateEndpointOptions(configure, active));

    private IEnumerable<IGenericEndpoint> GetAllEndpoints()
    {
        yield return GetByIdEndpoint;
        yield return GetListEndpoint;
        yield return CreateEndpoint;
        yield return UpdateEndpoint;
        yield return DeleteEndpoint;
    }

    private IGenericEndpoint? GetEndpoint(GenericEndpoint endpointName) => endpointName switch
    {
        GenericEndpoint.GetList => GetListEndpoint,
        GenericEndpoint.GetById => GetByIdEndpoint,
        GenericEndpoint.Create => CreateEndpoint,
        GenericEndpoint.Update => UpdateEndpoint,
        GenericEndpoint.Delete => DeleteEndpoint,
        _ => null
    };

    /// <inheritdoc/>
    public void DisableEndpoint(GenericEndpoint endpointName)
    {
        if (endpointName == GenericEndpoint.All)
        {
            foreach (var endpoint in GetAllEndpoints())
            {
                CRUDEndpoints<TDbContext, TEntity, TKey>.ConfigureEndpoint(endpoint, active: false);
            }

            return;
        }

        var endpointToDisable = GetEndpoint(endpointName);
        if (endpointToDisable is null)
            return;

        CRUDEndpoints<TDbContext, TEntity, TKey>.ConfigureEndpoint(endpointToDisable, active: false);
    }

    /// <inheritdoc/>
    public void ConfigureGroup(Action<RouteGroupBuilder>? configure) => ConfigGroup = configure;

    /// <inheritdoc/>
    public void ConfigureAll(Action<RouteHandlerBuilder> configure)
    {
        ConfigAll = CRUDEndpoints<TDbContext, TEntity, TKey>.CreateEndpointOptions(configure);
    }

    /// <inheritdoc/>
    public void ConfigureAll<TDto>(Action<RouteHandlerBuilder>? configure = null) where TDto : class
    {
        ConfigAll = CRUDEndpoints<TDbContext, TEntity, TKey>.CreateEndpointOptions(configure);

        GetByIdEndpoint = new GetByIdEndpoint<TDbContext, TEntity, TKey, TDto>(PredicateFactory);
        CRUDEndpoints<TDbContext, TEntity, TKey>.ConfigureEndpoint(GetByIdEndpoint, configure);

        GetListEndpoint = new GetListEndpoint<TDbContext, TEntity, TKey, TDto>(IdSelector);
        ApplyListMaxPageSize();
        CRUDEndpoints<TDbContext, TEntity, TKey>.ConfigureEndpoint(GetListEndpoint, configure);

        CreateEndpoint = new CreateEndpoint<TDbContext, TEntity, TKey, TDto>(CrudPrefix);
        CRUDEndpoints<TDbContext, TEntity, TKey>.ConfigureEndpoint(CreateEndpoint, configure);

        UpdateEndpoint = new UpdateEndpoint<TDbContext, TEntity, TKey, TDto>(PredicateFactory, IdSelector);
        CRUDEndpoints<TDbContext, TEntity, TKey>.ConfigureEndpoint(UpdateEndpoint, configure);
    }

    /// <inheritdoc/>
    public void ConfigureAll<TRequest, TResponse>(Action<RouteHandlerBuilder>? configure = null) where TRequest : class where TResponse : class
    {
        ConfigAll = CRUDEndpoints<TDbContext, TEntity, TKey>.CreateEndpointOptions(configure);

        GetByIdEndpoint = new GetByIdEndpoint<TDbContext, TEntity, TKey, TResponse>(PredicateFactory);
        CRUDEndpoints<TDbContext, TEntity, TKey>.ConfigureEndpoint(GetByIdEndpoint, configure);

        GetListEndpoint = new GetListEndpoint<TDbContext, TEntity, TKey, TResponse>(IdSelector);
        ApplyListMaxPageSize();
        CRUDEndpoints<TDbContext, TEntity, TKey>.ConfigureEndpoint(GetListEndpoint, configure);

        CreateEndpoint = new CreateEndpoint<TDbContext, TEntity, TKey, TRequest, TResponse>(CrudPrefix);
        CRUDEndpoints<TDbContext, TEntity, TKey>.ConfigureEndpoint(CreateEndpoint, configure);

        UpdateEndpoint = new UpdateEndpoint<TDbContext, TEntity, TKey, TRequest>(PredicateFactory, IdSelector);
        CRUDEndpoints<TDbContext, TEntity, TKey>.ConfigureEndpoint(UpdateEndpoint, configure);
    }

    /// <inheritdoc/>
    public void Get(Action<RouteHandlerBuilder>? configure = null) =>
        CRUDEndpoints<TDbContext, TEntity, TKey>.ConfigureEndpoint(GetByIdEndpoint, configure);

    /// <inheritdoc/>
    public void Get<TDto>(Action<RouteHandlerBuilder>? configure = null) where TDto : class
    {
        GetByIdEndpoint = new GetByIdEndpoint<TDbContext, TEntity, TKey, TDto>(PredicateFactory);
        CRUDEndpoints<TDbContext, TEntity, TKey>.ConfigureEndpoint(GetByIdEndpoint, configure);
    }

    // Applies the stored per-endpoint max page size override to the current GetList endpoint
    // (the DTO variant derives from the base, so a single cast covers both).
    private void ApplyListMaxPageSize()
    {
        if (GetListEndpoint is GetListEndpoint<TDbContext, TEntity, TKey> listEndpoint)
            listEndpoint.MaxPageSizeOverride = ListMaxPageSize;
    }

    /// <inheritdoc/>
    public void GetList(Action<RouteHandlerBuilder>? configure = null) =>
        CRUDEndpoints<TDbContext, TEntity, TKey>.ConfigureEndpoint(GetListEndpoint, configure);

    /// <inheritdoc/>
    public void GetList(int maxPageSize, Action<RouteHandlerBuilder>? configure = null)
    {
        ListMaxPageSize = maxPageSize;
        ApplyListMaxPageSize();
        CRUDEndpoints<TDbContext, TEntity, TKey>.ConfigureEndpoint(GetListEndpoint, configure);
    }

    /// <inheritdoc/>
    public void GetList<TDto>(Action<RouteHandlerBuilder>? configure = null) where TDto : class
    {
        GetListEndpoint = new GetListEndpoint<TDbContext, TEntity, TKey, TDto>(IdSelector);
        ApplyListMaxPageSize();
        CRUDEndpoints<TDbContext, TEntity, TKey>.ConfigureEndpoint(GetListEndpoint, configure);
    }

    /// <inheritdoc/>
    public void GetList<TDto>(int maxPageSize, Action<RouteHandlerBuilder>? configure = null) where TDto : class
    {
        ListMaxPageSize = maxPageSize;
        GetListEndpoint = new GetListEndpoint<TDbContext, TEntity, TKey, TDto>(IdSelector);
        ApplyListMaxPageSize();
        CRUDEndpoints<TDbContext, TEntity, TKey>.ConfigureEndpoint(GetListEndpoint, configure);
    }

    /// <inheritdoc/>
    public void Create(Action<RouteHandlerBuilder>? configure = null) =>
        CRUDEndpoints<TDbContext, TEntity, TKey>.ConfigureEndpoint(CreateEndpoint, configure);

    /// <inheritdoc/>
    public void Create<TDto>(Action<RouteHandlerBuilder>? configure = null) where TDto : class
    {
        CreateEndpoint = new CreateEndpoint<TDbContext, TEntity, TKey, TDto>(CrudPrefix);
        CRUDEndpoints<TDbContext, TEntity, TKey>.ConfigureEndpoint(CreateEndpoint, configure);
    }

    /// <inheritdoc/>
    public void Create<TRequest, TResponse>(Action<RouteHandlerBuilder>? configure = null) where TRequest : class where TResponse : class
    {
        CreateEndpoint = new CreateEndpoint<TDbContext, TEntity, TKey, TRequest, TResponse>(CrudPrefix);
        CRUDEndpoints<TDbContext, TEntity, TKey>.ConfigureEndpoint(CreateEndpoint, configure);
    }

    /// <inheritdoc/>
    public void Update(Action<RouteHandlerBuilder>? configure = null) =>
        CRUDEndpoints<TDbContext, TEntity, TKey>.ConfigureEndpoint(UpdateEndpoint, configure);

    /// <inheritdoc/>
    public void Update<TDto>(Action<RouteHandlerBuilder>? configure = null) where TDto : class
    {
        UpdateEndpoint = new UpdateEndpoint<TDbContext, TEntity, TKey, TDto>(PredicateFactory, IdSelector);
        CRUDEndpoints<TDbContext, TEntity, TKey>.ConfigureEndpoint(UpdateEndpoint, configure);
    }

    /// <inheritdoc/>
    public void Delete(Action<RouteHandlerBuilder>? configure = null) =>
        CRUDEndpoints<TDbContext, TEntity, TKey>.ConfigureEndpoint(DeleteEndpoint, configure);

    /// <inheritdoc cref="ICrudEndpoints{TDbContext}.Map"/>
    public void Map(RouteGroupBuilder group, ILogger logger, string moduleRoutePrefix)
    {
        var entityName = typeof(TEntity).Name;
        var route = CombineRoute(moduleRoutePrefix, RoutePrefix);
        var enabledVerbs = GetEnabledVerbs();

        FastSharpLogger.LogMappingCrudEndpoints(logger, entityName, route, enabledVerbs);

        group = group.MapGroup(RoutePrefix);
        ConfigGroup?.Invoke(group);

        foreach (var endpoint in GetAllEndpoints())
            endpoint.Map(group, ConfigAll);
    }

    private static string CombineRoute(string moduleRoutePrefix, string routePrefix)
    {
        var combined = $"{moduleRoutePrefix.TrimEnd('/')}{routePrefix}";
        return combined.StartsWith('/') ? combined : $"/{combined}";
    }

    private string GetEnabledVerbs()
    {
        var verbs = new List<string>(5);

        if (GetListEndpoint.IsActive)
            verbs.Add("GET (list)");

        if (GetByIdEndpoint.IsActive)
            verbs.Add("GET (by id)");

        if (CreateEndpoint.IsActive)
            verbs.Add("POST");

        if (UpdateEndpoint.IsActive)
            verbs.Add("PUT");

        if (DeleteEndpoint.IsActive)
            verbs.Add("DELETE");

        return verbs.Count == 0 ? "none" : string.Join(", ", verbs);
    }
}
