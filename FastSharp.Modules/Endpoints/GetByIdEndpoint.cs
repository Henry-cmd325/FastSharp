using FastSharp.Modules.Configuration;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FastSharp.Modules.Endpoints;

public class GetByIdEndpoint<TDbContext, TEntity, TKey>(Func<TKey, Expression<Func<TEntity, bool>>> predicateFactory) : IGenericEndpoint
    where TEntity : class
    where TDbContext : DbContext
{
    protected EndpointOptions _options = new();

    protected readonly Func<TKey, Expression<Func<TEntity, bool>>> _predicateFactory = predicateFactory;

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
                var entity = await context.Set<TEntity>().Where(_predicateFactory(id)).FirstOrDefaultAsync();
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
public class GetByIdEndpoint<TDbContext, TEntity, TKey, TDto>(Func<TKey, Expression<Func<TEntity, bool>>> predicateFactory) : GetByIdEndpoint<TDbContext, TEntity, TKey>(predicateFactory)
    where TEntity : class
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
                    .Where(_predicateFactory(id))
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