using FastSharp.Models;
using FastSharp.Modules.Endpoints;
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
    internal IGenericEndpoint GetPagedEndpoint; 
    internal IGenericEndpoint CreateEndpoint; 
    internal IGenericEndpoint UpdateEndpoint; 
    internal IGenericEndpoint DeleteEndpoint;

    internal string RoutePrefix { get; set; }
    internal Func<TKey, Expression<Func<TEntity, bool>>> PredicateFactory;
    internal Expression<Func<TEntity, TKey>> IdSelector;

    public CRUDEndpoints(string routePrefix = "", Expression<Func<TEntity, TKey>>? idSelector = null)
    {
        RoutePrefix = routePrefix;
        // If no selector is provided, try to use IModel
        if (idSelector == null && typeof(IModel<TKey>).IsAssignableFrom(typeof(TEntity)))
            IdSelector = e => ((IModel<TKey>)e).Id;
        else
            IdSelector = idSelector ?? throw new ArgumentException("Debes proveer un ID selector o implementar IModel");

        // Compile the predicate creation logic once during initialization
        var parameter = IdSelector.Parameters[0];
        var left = IdSelector.Body;

        PredicateFactory = (id) => {
            var right = Expression.Constant(id, typeof(TKey));
            var comparison = Expression.Equal(left, right);
            return Expression.Lambda<Func<TEntity, bool>>(comparison, parameter);
        };

        GetByIdEndpoint = new GetByIdEndpoint<TDbContext, TEntity, TKey>(PredicateFactory);
        GetListEndpoint = new GetListEndpoint<TDbContext, TEntity, TKey>();
        GetPagedEndpoint = new GetPagedEndpoint<TDbContext, TEntity, TKey>(IdSelector);
        CreateEndpoint = new CreateEndpoint<TDbContext, TEntity, TKey>();
        UpdateEndpoint = new UpdateEndpoint<TDbContext, TEntity, TKey>(PredicateFactory);
        DeleteEndpoint = new DeleteEndpoint<TDbContext, TEntity, TKey>(PredicateFactory);
    }

    private EndpointOptions CreateEndpointOptions(Action<RouteHandlerBuilder>? configure = null, bool active = true) =>
        new() { Builder = configure, Active = active };

    private void ConfigureEndpoint(IGenericEndpoint endpoint, Action<RouteHandlerBuilder>? configure = null, bool active = true) =>
        endpoint.Configure(CreateEndpointOptions(configure, active));

    private IEnumerable<IGenericEndpoint> GetAllEndpoints()
    {
        yield return GetByIdEndpoint;
        yield return GetListEndpoint;
        yield return GetPagedEndpoint;
        yield return CreateEndpoint;
        yield return UpdateEndpoint;
        yield return DeleteEndpoint;
    }

    private IGenericEndpoint? GetEndpoint(GenericEndpoint endpointName) => endpointName switch
    {
        GenericEndpoint.GetPaged => GetPagedEndpoint,
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
                ConfigureEndpoint(endpoint, active: false);
            }

            return;
        }

        var endpointToDisable = GetEndpoint(endpointName);
        if (endpointToDisable is null)
            return;

        ConfigureEndpoint(endpointToDisable, active: false);
    }

    public void ConfigureGroup(Action<RouteGroupBuilder>? configure) => ConfigGroup = configure;

    public void ConfigureAll(Action<RouteHandlerBuilder> configure)
    {
        ConfigAll = CreateEndpointOptions(configure);
    }

    public void ConfigureAll<TDto>(Action<RouteHandlerBuilder>? configure = null) where TDto : class
    {
        ConfigAll = CreateEndpointOptions(configure);

        GetByIdEndpoint = new GetByIdEndpoint<TDbContext, TEntity, TKey, TDto>(PredicateFactory);
        ConfigureEndpoint(GetByIdEndpoint, configure);

        GetListEndpoint = new GetListEndpoint<TDbContext, TEntity, TKey, TDto>();
        ConfigureEndpoint(GetListEndpoint, configure);

        GetPagedEndpoint = new GetPagedEndpoint<TDbContext, TEntity, TKey, TDto>(IdSelector);
        ConfigureEndpoint(GetPagedEndpoint, configure);

        CreateEndpoint = new CreateEndpoint<TDbContext, TEntity, TKey, TDto>();
        ConfigureEndpoint(CreateEndpoint, configure);

        UpdateEndpoint = new UpdateEndpoint<TDbContext, TEntity, TKey, TDto>(PredicateFactory);
        ConfigureEndpoint(UpdateEndpoint, configure);
    }

    public void ConfigureAll<TRequest, TResponse>(Action<RouteHandlerBuilder>? configure = null) where TRequest : class where TResponse : class
    {
        ConfigAll = CreateEndpointOptions(configure);

        GetByIdEndpoint = new GetByIdEndpoint<TDbContext, TEntity, TKey, TResponse>(PredicateFactory);
        ConfigureEndpoint(GetByIdEndpoint, configure);

        GetListEndpoint = new GetListEndpoint<TDbContext, TEntity, TKey, TResponse>();
        ConfigureEndpoint(GetListEndpoint, configure);

        GetPagedEndpoint = new GetPagedEndpoint<TDbContext, TEntity, TKey, TResponse>(IdSelector);
        ConfigureEndpoint(GetPagedEndpoint, configure);

        CreateEndpoint = new CreateEndpoint<TDbContext, TEntity, TKey, TRequest, TResponse>();
        ConfigureEndpoint(CreateEndpoint, configure);

        UpdateEndpoint = new UpdateEndpoint<TDbContext, TEntity, TKey, TRequest>(PredicateFactory);
        ConfigureEndpoint(UpdateEndpoint, configure);
    }

    public void Get(Action<RouteHandlerBuilder>? configure = null) =>
        ConfigureEndpoint(GetByIdEndpoint, configure);

    public void Get<TDto>(Action<RouteHandlerBuilder>? configure = null) where TDto : class
    {
        GetByIdEndpoint = new GetByIdEndpoint<TDbContext, TEntity, TKey, TDto>(PredicateFactory);
        ConfigureEndpoint(GetByIdEndpoint, configure);
    }

    public void GetList(Action<RouteHandlerBuilder>? configure = null) =>
        ConfigureEndpoint(GetListEndpoint, configure);

    public void GetList<TDto>(Action<RouteHandlerBuilder>? configure = null) where TDto : class
    {
        GetListEndpoint = new GetListEndpoint<TDbContext, TEntity, TKey, TDto>();
        ConfigureEndpoint(GetListEndpoint, configure);
    }

    public void GetPaged(Action<RouteHandlerBuilder>? configure = null) =>
        ConfigureEndpoint(GetPagedEndpoint, configure);

    public void GetPaged<TDto>(Action<RouteHandlerBuilder>? configure = null) where TDto : class
    {
        GetPagedEndpoint = new GetPagedEndpoint<TDbContext, TEntity, TKey, TDto>(IdSelector);
        ConfigureEndpoint(GetPagedEndpoint, configure);
    }

    public void Create(Action<RouteHandlerBuilder>? configure = null) =>
        ConfigureEndpoint(CreateEndpoint, configure);

    public void Create<TDto>(Action<RouteHandlerBuilder>? configure = null) where TDto : class
    { 
        CreateEndpoint = new CreateEndpoint<TDbContext, TEntity, TKey, TDto>();
        ConfigureEndpoint(CreateEndpoint, configure);
    }

    public void Create<TRequest, TResponse>(Action<RouteHandlerBuilder>? configure = null) where TRequest : class where TResponse : class
    {
        CreateEndpoint = new CreateEndpoint<TDbContext, TEntity, TKey, TRequest, TResponse>();
        ConfigureEndpoint(CreateEndpoint, configure);
    }

    public void Update(Action<RouteHandlerBuilder>? configure = null) =>
        ConfigureEndpoint(UpdateEndpoint, configure);

    public void Update<TDto>(Action<RouteHandlerBuilder>? configure = null) where TDto : class
    {
        UpdateEndpoint = new UpdateEndpoint<TDbContext, TEntity, TKey, TDto>(PredicateFactory);
        ConfigureEndpoint(UpdateEndpoint, configure);
    }

    public void Delete(Action<RouteHandlerBuilder>? configure = null) =>
        ConfigureEndpoint(DeleteEndpoint, configure);

    public void Map(RouteGroupBuilder group)
    {
        group = group.MapGroup(RoutePrefix);
        ConfigGroup?.Invoke(group);

        foreach (var endpoint in GetAllEndpoints())
            endpoint.Map(group, ConfigAll);
    }
}