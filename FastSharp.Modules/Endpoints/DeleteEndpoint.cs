using FastSharp.Models;
using FastSharp.Modules.Configuration;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastSharp.Modules.Endpoints;

public class DeleteEndpoint<TDbContext, TEntity, TKey> : IGenericEndpoint
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
            var builder = app.MapDelete("/{id}", async Task<Results<NoContent, NotFound>> ([FromRoute] TKey id, [FromServices] TDbContext context) =>
            {
                var entity = await context.Set<TEntity>().FindAsync(id);
                if (entity is null)
                {
                    return TypedResults.NotFound();
                }

                context.Set<TEntity>().Remove(entity);
                await context.SaveChangesAsync();
                return TypedResults.NoContent();
            });

            allOptions.Builder?.Invoke(builder);
            _options.Builder?.Invoke(builder);
        }
    }
}

