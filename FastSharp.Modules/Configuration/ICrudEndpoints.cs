using FastSharp.Models;
using Microsoft.EntityFrameworkCore;

namespace FastSharp.Modules.Configuration;

public interface ICrudEndpoints<TDbContext> where TDbContext : DbContext
{
    Type EntityType { get; }
    Type KeyType { get; }

    public void DisableEndpoint(GenericEndpoint endpointName);

    /// <summary>
    /// Configures all CRUD endpoints to use a single DTO type.
    /// </summary>
    /// <remarks>
    /// The provided <typeparamref name="TDto"/> is used as the contract for GetById, GetList, GetPaged,
    /// Create, and Update endpoints.
    /// For list and paged endpoints, the framework wraps the DTO automatically as <c>List&lt;TDto&gt;</c>
    /// and <see cref="PagedResult{T}"/>.
    /// </remarks>
    /// <typeparam name="TDto">The DTO type used for all CRUD endpoints.</typeparam>
    /// <param name="configure">Optional endpoint configuration applied to all mapped CRUD handlers.</param>
    public void ConfigureAll<TDto>(Action<RouteHandlerBuilder>? configure = null) where TDto : class;

    /// <summary>
    /// Configures CRUD endpoints with different request and response DTO types.
    /// </summary>
    /// <remarks>
    /// <typeparamref name="TResponse"/> is used for GetById, GetList, and GetPaged responses.
    /// <typeparamref name="TRequest"/> is used for Create and Update request bodies.
    /// Create returns <typeparamref name="TResponse"/>.
    /// For list and paged endpoints, the framework wraps <typeparamref name="TResponse"/> automatically
    /// as <c>List&lt;TResponse&gt;</c> and <see cref="PagedResult{T}"/>.
    /// </remarks>
    /// <typeparam name="TRequest">The DTO type used for write operations.</typeparam>
    /// <typeparam name="TResponse">The DTO type used for read operations.</typeparam>
    /// <param name="configure">Optional endpoint configuration applied to all mapped CRUD handlers.</param>
    public void ConfigureAll<TRequest, TResponse>(Action<RouteHandlerBuilder>? configure = null) where TRequest : class where TResponse : class;
    public void ConfigureGroup(Action<RouteGroupBuilder>? configure = null);
    public void Get(Action<RouteHandlerBuilder>? configure = null);
    public void Get<TDto>(Action<RouteHandlerBuilder>? configure = null) where TDto : class;
    public void GetList(Action<RouteHandlerBuilder>? configure = null);

    /// <summary>
    /// This method already returns a <see cref="List{TEntity}"/>, so the configured DTO represents
    /// the entities inside the list rather than the full list response.
    /// </summary>
    /// <param name="configure">An optional delegate to further configure the route using the provided RouteHandlerBuilder. If null, the route
    /// is added with default configuration.</param>
    public void GetList<TDto>(Action<RouteHandlerBuilder>? configure = null) where TDto : class;

    /// <summary>
    /// This method already returns a <see cref="PagedResult{TEntity}"/>, so the configured DTO represents
    /// the entities inside the paged result rather than the full paged response.
    /// </summary>
    /// <param name="configure">Additional endpoint configuration.</param>
    public void GetPaged(Action<RouteHandlerBuilder>? configure = null);

    /// <summary>
    /// This method already returns a <see cref="PagedResult{TDto}"/>, so the configured DTO represents
    /// the entities inside the paged result rather than the full paged response.
    /// </summary>
    /// <param name="configure">Additional endpoint configuration.</param>
    public void GetPaged<TDto>(Action<RouteHandlerBuilder>? configure = null) where TDto : class;

    public void Create(Action<RouteHandlerBuilder>? configure = null);
    public void Create<TDto>(Action<RouteHandlerBuilder>? configure = null) where TDto : class;
    public void Create<TRequest, TResponse>(Action<RouteHandlerBuilder>? configure = null) where TRequest : class where TResponse : class;
    public void Update(Action<RouteHandlerBuilder>? configure = null);
    public void Update<TDto>(Action<RouteHandlerBuilder>? configure = null) where TDto : class;
    public void Delete(Action<RouteHandlerBuilder>? configure = null);
    internal void Map(RouteGroupBuilder group);
}