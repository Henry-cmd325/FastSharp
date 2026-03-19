using Microsoft.EntityFrameworkCore;
using FastSharp.Models;

namespace FastSharp.Modules.Configuration;

public interface ICrudEndpoints<TDbContext> where TDbContext : DbContext
{
    Type EntityType { get; }
    Type KeyType { get; }

    public ICrudEndpoints<TDbContext> DisableEndpoint(GenericEndpoint endpointName);

    public ICrudEndpoints<TDbContext> ConfigureEndpoint(GenericEndpoint endpointName, Action<RouteHandlerBuilder> configure);

    public ICrudEndpoints<TDbContext> ConfigureGroup(Action<RouteGroupBuilder> configure);

    public void Get(Action<RouteHandlerBuilder> configure);

    public void Get<TDto>(Action<RouteHandlerBuilder> configure);

    public void GetList(Action<RouteHandlerBuilder> configure);

    public void GetList<TDto>(Action<RouteHandlerBuilder> configure);

    /// <summary>
    /// This method already returns a <see cref="PagedResult{TEntity}"/>, so the configured DTO represents
    /// the entity inside the paged result rather than the full paged response.
    /// </summary>
    /// <param name="configure">Additional endpoint configuration.</param>
    public void GetPaged(Action<RouteHandlerBuilder> configure);

    /// <summary>
    /// This method already returns a <see cref="PagedResult{TDto}"/>, so the configured DTO represents
    /// the entity inside the paged result rather than the full paged response.
    /// </summary>
    /// <param name="configure">Additional endpoint configuration.</param>
    public void GetPaged<TDto>(Action<RouteHandlerBuilder> configure);

    public void Create(Action<RouteHandlerBuilder> configure);

    public void Create<TDto>(Action<RouteHandlerBuilder> configure);

    public void Create<TRequest, TResponse>(Action<RouteHandlerBuilder> configure);

    public void Update(Action<RouteHandlerBuilder> configure);

    public void Update<TDto>(Action<RouteHandlerBuilder> configure);

    public void Delete(Action<RouteHandlerBuilder> configure);

    internal void Map(RouteGroupBuilder group);
}