using FastSharp.Models;
using FastSharp.Modules.Endpoints;
using Microsoft.EntityFrameworkCore;

namespace FastSharp.Modules.Configuration;

public class CRUDEndpoints<TDbContext, TEntity, TKey>(string routePrefix = "") : ICrudEndpoints<TDbContext>
    where TEntity : class, IModel<TKey>
    where TDbContext : DbContext
{
    internal Action<RouteGroupBuilder>? ConfigGroup;

    internal EndpointOptions ConfigAll = new();

    internal IGenericEndpoint GetByIdEndpoint = new GetByIdEndpoint<TDbContext, TEntity, TKey>();
    internal IGenericEndpoint GetListEndpoint = new GetListEndpoint<TDbContext, TEntity, TKey>();
    internal IGenericEndpoint GetPagedEndpoint = new GetPagedEndpoint<TDbContext, TEntity, TKey>();
    internal IGenericEndpoint CreateEndpoint = new CreateEndpoint<TDbContext, TEntity, TKey>();
    internal IGenericEndpoint UpdateEndpoint = new UpdateEndpoint<TDbContext, TEntity, TKey>();
    internal IGenericEndpoint DeleteEndpoint = new DeleteEndpoint<TDbContext, TEntity, TKey>();

    internal string RoutePrefix { get; set; } = routePrefix;

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

        GetByIdEndpoint = new GetByIdEndpoint<TDbContext, TEntity, TKey, TDto>();
        ConfigureEndpoint(GetByIdEndpoint, configure);

        GetListEndpoint = new GetListEndpoint<TDbContext, TEntity, TKey, TDto>();
        ConfigureEndpoint(GetListEndpoint, configure);

        GetPagedEndpoint = new GetPagedEndpoint<TDbContext, TEntity, TKey, TDto>();
        ConfigureEndpoint(GetPagedEndpoint, configure);

        CreateEndpoint = new CreateEndpoint<TDbContext, TEntity, TKey, TDto>();
        ConfigureEndpoint(CreateEndpoint, configure);

        UpdateEndpoint = new UpdateEndpoint<TDbContext, TEntity, TKey, TDto>();
        ConfigureEndpoint(UpdateEndpoint, configure);
    }

    public void ConfigureAll<TRequest, TResponse>(Action<RouteHandlerBuilder>? configure = null) where TRequest : class where TResponse : class
    {
        ConfigAll = CreateEndpointOptions(configure);

        GetByIdEndpoint = new GetByIdEndpoint<TDbContext, TEntity, TKey, TResponse>();
        ConfigureEndpoint(GetByIdEndpoint, configure);

        GetListEndpoint = new GetListEndpoint<TDbContext, TEntity, TKey, TResponse>();
        ConfigureEndpoint(GetListEndpoint, configure);

        GetPagedEndpoint = new GetPagedEndpoint<TDbContext, TEntity, TKey, TResponse>();
        ConfigureEndpoint(GetPagedEndpoint, configure);

        CreateEndpoint = new CreateEndpoint<TDbContext, TEntity, TKey, TRequest, TResponse>();
        ConfigureEndpoint(CreateEndpoint, configure);

        UpdateEndpoint = new UpdateEndpoint<TDbContext, TEntity, TKey, TRequest>();
        ConfigureEndpoint(UpdateEndpoint, configure);
    }

    public void Get(Action<RouteHandlerBuilder>? configure = null) =>
        ConfigureEndpoint(GetByIdEndpoint, configure);

    public void Get<TDto>(Action<RouteHandlerBuilder>? configure = null) where TDto : class
    {
        GetByIdEndpoint = new GetByIdEndpoint<TDbContext, TEntity, TKey, TDto>();
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
        GetPagedEndpoint = new GetPagedEndpoint<TDbContext, TEntity, TKey, TDto>();
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
        UpdateEndpoint = new UpdateEndpoint<TDbContext, TEntity, TKey, TDto>();
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
