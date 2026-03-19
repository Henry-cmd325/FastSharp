using FastSharp.Models;
using FastSharp.Modules.Configuration;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastSharp.Modules.Endpoints;

public class GetListEndpoint<TDbContext, TEntity, TKey>
    where TEntity : class, IModel<TKey>
    where TDbContext : DbContext
{
    protected EndpointOptions _options = new();

    public void Configure(EndpointOptions options)
    {
        _options = options;
    }

    public virtual void Handle(RouteGroupBuilder app, EndpointOptions allOptions)
    {
        if (_options.Active)
        {
            var builder = app.MapGet("", async Task<Ok<List<TEntity>>> ([FromServices] TDbContext context) =>
            {
                var list = await context.Set<TEntity>().ToListAsync();
                return TypedResults.Ok(list);
            });

            allOptions.Builder?.Invoke(builder);
            _options.Builder?.Invoke(builder);
        }
    }
}

public class GetListEndpoint<TDbContext, TEntity, TKey, TDto> : GetListEndpoint<TDbContext, TEntity, TKey>
    where TEntity : class, IModel<TKey>
    where TDbContext : DbContext
    where TDto : class
{
    public override void Handle(RouteGroupBuilder app, EndpointOptions allOptions)
    {
        if (_options.Active)
        {
            var builder = app.MapGet("", async Task<Ok<List<TDto>>> ([FromServices] TDbContext context) =>
            {
                var list = await context.Set<TEntity>()
                    .AsNoTracking()
                    .ProjectToType<TDto>()
                    .ToListAsync();

                return TypedResults.Ok(list);
            });

            allOptions.Builder?.Invoke(builder);
            _options.Builder?.Invoke(builder);
        }
    }
}
