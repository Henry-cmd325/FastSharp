using FastSharp.Models;
using FastSharp.Modules.Core.Endpoints;
using FastSharp.Modules.Logging;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FastSharp.Modules.Configuration;

public class CRUDEndpoints<TDbContext, TEntity, TKey> : ICrudEndpoints<TDbContext>
    where TEntity : class
    where TDbContext : DbContext
{
    internal Action<RouteGroupBuilder>? ConfigGroup;

    internal EndpointOptions ConfigAll = new();

    internal IGenericEndpoint GetByIdEndpoint; 
    internal IGenericEndpoint GetListEndpoint; 
    internal IGenericEndpoint CreateEndpoint; 
    internal IGenericEndpoint UpdateEndpoint; 
    internal IGenericEndpoint DeleteEndpoint;

    internal string RoutePrefix { get; set; }
    internal string CrudPrefix { get; set; }
    internal Func<TKey, Expression<Func<TEntity, bool>>> PredicateFactory;
    internal Expression<Func<TEntity, TKey>> IdSelector;

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

        PredicateFactory = (id) => {
            var right = Expression.Constant(id, typeof(TKey));
            var comparison = Expression.Equal(left, right);
            return Expression.Lambda<Func<TEntity, bool>>(comparison, parameter);
        };

        GetByIdEndpoint = new GetByIdEndpoint<TDbContext, TEntity, TKey>(PredicateFactory);
        GetListEndpoint = new GetListEndpoint<TDbContext, TEntity, TKey>(IdSelector);
        CreateEndpoint = new CreateEndpoint<TDbContext, TEntity, TKey>(CrudPrefix);
        UpdateEndpoint = new UpdateEndpoint<TDbContext, TEntity, TKey>(PredicateFactory);
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

    public void ConfigureGroup(Action<RouteGroupBuilder>? configure) => ConfigGroup = configure;

    public void ConfigureAll(Action<RouteHandlerBuilder> configure)
    {
        ConfigAll = CRUDEndpoints<TDbContext, TEntity, TKey>.CreateEndpointOptions(configure);
    }

    public void ConfigureAll<TDto>(Action<RouteHandlerBuilder>? configure = null) where TDto : class
    {
        ConfigAll = CRUDEndpoints<TDbContext, TEntity, TKey>.CreateEndpointOptions(configure);

        GetByIdEndpoint = new GetByIdEndpoint<TDbContext, TEntity, TKey, TDto>(PredicateFactory);
        CRUDEndpoints<TDbContext, TEntity, TKey>.ConfigureEndpoint(GetByIdEndpoint, configure);

        GetListEndpoint = new GetListEndpoint<TDbContext, TEntity, TKey, TDto>(IdSelector);
        CRUDEndpoints<TDbContext, TEntity, TKey>.ConfigureEndpoint(GetListEndpoint, configure);

        CreateEndpoint = new CreateEndpoint<TDbContext, TEntity, TKey, TDto>(CrudPrefix);
        CRUDEndpoints<TDbContext, TEntity, TKey>.ConfigureEndpoint(CreateEndpoint, configure);

        UpdateEndpoint = new UpdateEndpoint<TDbContext, TEntity, TKey, TDto>(PredicateFactory);
        CRUDEndpoints<TDbContext, TEntity, TKey>.ConfigureEndpoint(UpdateEndpoint, configure);
    }

    public void ConfigureAll<TRequest, TResponse>(Action<RouteHandlerBuilder>? configure = null) where TRequest : class where TResponse : class
    {
        ConfigAll = CRUDEndpoints<TDbContext, TEntity, TKey>.CreateEndpointOptions(configure);

        GetByIdEndpoint = new GetByIdEndpoint<TDbContext, TEntity, TKey, TResponse>(PredicateFactory);
        CRUDEndpoints<TDbContext, TEntity, TKey>.ConfigureEndpoint(GetByIdEndpoint, configure);

        GetListEndpoint = new GetListEndpoint<TDbContext, TEntity, TKey, TResponse>(IdSelector);
        CRUDEndpoints<TDbContext, TEntity, TKey>.ConfigureEndpoint(GetListEndpoint, configure);

        CreateEndpoint = new CreateEndpoint<TDbContext, TEntity, TKey, TRequest, TResponse>(CrudPrefix);
        CRUDEndpoints<TDbContext, TEntity, TKey>.ConfigureEndpoint(CreateEndpoint, configure);

        UpdateEndpoint = new UpdateEndpoint<TDbContext, TEntity, TKey, TRequest>(PredicateFactory);
        CRUDEndpoints<TDbContext, TEntity, TKey>.ConfigureEndpoint(UpdateEndpoint, configure);
    }

    public void Get(Action<RouteHandlerBuilder>? configure = null) =>
        CRUDEndpoints<TDbContext, TEntity, TKey>.ConfigureEndpoint(GetByIdEndpoint, configure);

    public void Get<TDto>(Action<RouteHandlerBuilder>? configure = null) where TDto : class
    {
        GetByIdEndpoint = new GetByIdEndpoint<TDbContext, TEntity, TKey, TDto>(PredicateFactory);
        CRUDEndpoints<TDbContext, TEntity, TKey>.ConfigureEndpoint(GetByIdEndpoint, configure);
    }

    public void GetList(Action<RouteHandlerBuilder>? configure = null) =>
        CRUDEndpoints<TDbContext, TEntity, TKey>.ConfigureEndpoint(GetListEndpoint, configure);

    public void GetList<TDto>(Action<RouteHandlerBuilder>? configure = null) where TDto : class
    {
        GetListEndpoint = new GetListEndpoint<TDbContext, TEntity, TKey, TDto>(IdSelector);
        CRUDEndpoints<TDbContext, TEntity, TKey>.ConfigureEndpoint(GetListEndpoint, configure);
    }

    public void Create(Action<RouteHandlerBuilder>? configure = null) =>
        CRUDEndpoints<TDbContext, TEntity, TKey>.ConfigureEndpoint(CreateEndpoint, configure);

    public void Create<TDto>(Action<RouteHandlerBuilder>? configure = null) where TDto : class
    { 
        CreateEndpoint = new CreateEndpoint<TDbContext, TEntity, TKey, TDto>(CrudPrefix);
        CRUDEndpoints<TDbContext, TEntity, TKey>.ConfigureEndpoint(CreateEndpoint, configure);
    }

    public void Create<TRequest, TResponse>(Action<RouteHandlerBuilder>? configure = null) where TRequest : class where TResponse : class
    {
        CreateEndpoint = new CreateEndpoint<TDbContext, TEntity, TKey, TRequest, TResponse>(CrudPrefix);
        CRUDEndpoints<TDbContext, TEntity, TKey>.ConfigureEndpoint(CreateEndpoint, configure);
    }

    public void Update(Action<RouteHandlerBuilder>? configure = null) =>
        CRUDEndpoints<TDbContext, TEntity, TKey>.ConfigureEndpoint(UpdateEndpoint, configure);

    public void Update<TDto>(Action<RouteHandlerBuilder>? configure = null) where TDto : class
    {
        UpdateEndpoint = new UpdateEndpoint<TDbContext, TEntity, TKey, TDto>(PredicateFactory);
        CRUDEndpoints<TDbContext, TEntity, TKey>.ConfigureEndpoint(UpdateEndpoint, configure);
    }

    public void Delete(Action<RouteHandlerBuilder>? configure = null) =>
        CRUDEndpoints<TDbContext, TEntity, TKey>.ConfigureEndpoint(DeleteEndpoint, configure);

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
