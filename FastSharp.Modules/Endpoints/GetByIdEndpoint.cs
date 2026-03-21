using FastSharp.Models;
using FastSharp.Modules.Configuration;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastSharp.Modules.Endpoints;

public class GetByIdEndpoint<TDbContext, TEntity, TKey> : IGenericEndpoint
    where TEntity : class, IModel<TKey>
    where TDbContext : DbContext
{
    protected EndpointOptions _options = new();

    public void Configure(EndpointOptions options)
    {
        _options = options;
    }

    public virtual void Map(RouteGroupBuilder app, EndpointOptions allOptions)
    {
        if (_options.Active)
        {
            var builder = app.MapGet("/{id}", async Task<Results<Ok<TEntity>, NotFound>> ([FromRoute] TKey id, [FromServices] TDbContext context) =>
            {
                var entity = await context.Set<TEntity>().FindAsync(id);
                if (entity is null)
                    return TypedResults.NotFound();

                return TypedResults.Ok(entity);
            });

            allOptions.Builder?.Invoke(builder);
            _options.Builder?.Invoke(builder);
        }
    }
}

// With DTO configured.
public class GetByIdEndpoint<TDbContext, TEntity, TKey, TDto> : GetByIdEndpoint<TDbContext, TEntity, TKey>
    where TEntity : class, IModel<TKey>
    where TDbContext : DbContext
    where TDto : class
{
    public override void Map(RouteGroupBuilder app, EndpointOptions allOptions)
    {
        if (_options.Active)
        {
            var builder = app.MapGet("/{id}", async Task<Results<Ok<TDto>, NotFound>> ([FromRoute] TKey id, [FromServices] TDbContext context) =>
            {
                var entity = await context.Set<TEntity>()
                    .AsNoTracking()
                    .Where(e => e.Id!.Equals(id))
                    .ProjectToType<TDto>()
                    .FirstOrDefaultAsync();

                if (entity is null)
                    return TypedResults.NotFound();

                return TypedResults.Ok(entity);
            });

            allOptions.Builder?.Invoke(builder);
            _options.Builder?.Invoke(builder);
        }
    }
}
