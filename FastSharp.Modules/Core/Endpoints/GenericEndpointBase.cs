using FastSharp.Modules.Configuration;
using Microsoft.EntityFrameworkCore;

namespace FastSharp.Modules.Core.Endpoints;

internal abstract class GenericEndpointBase<TDbContext, TEntity> : IGenericEndpoint
    where TEntity : class
    where TDbContext : DbContext
{
    protected EndpointOptions _options = new();

    protected static readonly string EntityName = typeof(TEntity).Name;

    public bool IsActive => _options.Active;

    public void Configure(EndpointOptions options) => _options = options;

    public abstract void Map(RouteGroupBuilder app, EndpointOptions allOptions);

    protected static void InvokeBuilders(RouteHandlerBuilder builder, EndpointOptions allOptions, EndpointOptions options)
    {
        allOptions.Builder?.Invoke(builder);
        options.Builder?.Invoke(builder);
    }
}
