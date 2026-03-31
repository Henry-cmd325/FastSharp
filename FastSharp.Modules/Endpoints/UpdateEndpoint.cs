using FastSharp.Models;
using FastSharp.Modules.Configuration;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FastSharp.Modules.Endpoints;

public class UpdateEndpoint<TDbContext, TEntity, TKey>(Func<TKey, Expression<Func<TEntity, bool>>> predicateFactory, Func<TEntity, TKey> getId) : IGenericEndpoint
    where TEntity : class
    where TDbContext : DbContext
{
    protected EndpointOptions _options = new();

    protected readonly Func<TEntity, TKey> _getId = getId;
    protected readonly Func<TKey, Expression<Func<TEntity, bool>>> _predicateFactory = predicateFactory;

    public void Configure(EndpointOptions options)
    {
        _options = options;
    }

    public virtual void Map(RouteGroupBuilder app, EndpointOptions allOptions)
    {
        if (_options.Active)
        {
            var builder = app.MapPut(
                "/{id}",
                async Task<Results<NoContent, NotFound, BadRequest<string>>> (
                    [FromRoute] TKey id,
                    [FromBody] TEntity updatedEntity,
                    [FromServices] TDbContext context) =>
                {
                    if (!EqualityComparer<TKey>.Default.Equals(id, _getId(updatedEntity)))
                    {
                        return TypedResults.BadRequest("Route id must match body id.");
                    }

                    var entity = await context.Set<TEntity>().Where(_predicateFactory(id)).FirstOrDefaultAsync();
                    if (entity is null)
                        return TypedResults.NotFound();

                    context.Entry(entity).CurrentValues.SetValues(updatedEntity);
                    await context.SaveChangesAsync();
                    return TypedResults.NoContent();
                });

            allOptions.Builder?.Invoke(builder);
            _options.Builder?.Invoke(builder);
        }
    }
}

public class UpdateEndpoint<TDbContext, TEntity, TKey, TDto>(Func<TKey, Expression<Func<TEntity, bool>>> predicateFactory, Func<TEntity, TKey> getId) : UpdateEndpoint<TDbContext, TEntity, TKey>(predicateFactory, getId)
    where TEntity : class
    where TDbContext : DbContext
    where TDto : class
{
    public override void Map(RouteGroupBuilder app, EndpointOptions allOptions)
    {
        if (_options.Active)
        {
            var builder = app.MapPut(
                "/{id}",
                async Task<Results<NoContent, NotFound, BadRequest<string>>> (
                    [FromRoute] TKey id,
                    [FromBody] TDto updatedDto,
                    [FromServices] TDbContext context) =>
                {
                    var entity = await context.Set<TEntity>().FindAsync(id);
                    if (entity is null)
                        return TypedResults.NotFound();

                    updatedDto.Adapt(entity);
                    await context.SaveChangesAsync();
                    return TypedResults.NoContent();
                });

            allOptions.Builder?.Invoke(builder);
            _options.Builder?.Invoke(builder);
        }
    }
}

