using FastSharp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FastSharp.Modules.Configuration
{
    public class CRUDEndpoints<TDbContext, TEntity, TKey>(string routePrefix = "") : ICrudEndpoints<TDbContext>
        where TEntity : class, IModel<TKey>
        where TDbContext : DbContext
    {
        internal Action<RouteGroupBuilder>? ConfigGroup;

        internal EndpointOptions ConfigGetList = new();
        internal EndpointOptions ConfigGetById = new();
        internal EndpointOptions ConfigPost = new();
        internal EndpointOptions ConfigPut = new();
        internal EndpointOptions ConfigDelete = new();
        internal EndpointOptions ConfigAll = new();

        internal string RoutePrefix { get; set; } = routePrefix;

        public Type EntityType => typeof(TEntity);
        public Type KeyType => typeof(TKey);

        public ICrudEndpoints<TDbContext> DisableEndpoint(GenericEndpoint endpointName)
        {
            switch (endpointName)
            {
                case GenericEndpoint.GetList:
                    ConfigGetList.Active = false;
                    break;
                case GenericEndpoint.GetById:
                    ConfigGetById.Active = false;
                    break;
                case GenericEndpoint.Create:
                    ConfigPost.Active = false;
                    break;
                case GenericEndpoint.Update:
                    ConfigPut.Active = false;
                    break;
                case GenericEndpoint.Delete:
                    ConfigDelete.Active = false;
                    break;
                case GenericEndpoint.All:
                    ConfigAll.Active = false;
                    break;
            }

            return this;
        }

        public ICrudEndpoints<TDbContext> ConfigureEndpoint(GenericEndpoint endpointName, Action<RouteHandlerBuilder> configure)
        {
            switch (endpointName)
            {
                case GenericEndpoint.GetList:
                    ConfigGetList.Builder = b => configure(b);
                    break;
                case GenericEndpoint.GetById:
                    ConfigGetById.Builder = b => configure(b);
                    break;
                case GenericEndpoint.Create:
                    ConfigPost.Builder = b => configure(b);
                    break;
                case GenericEndpoint.Update:
                    ConfigPut.Builder = b => configure(b);
                    break;
                case GenericEndpoint.Delete:
                    ConfigDelete.Builder = b => configure(b);
                    break;
                case GenericEndpoint.All:
                    ConfigAll.Builder = b => configure(b);
                    break;
            }

            return this;
        }

        public ICrudEndpoints<TDbContext> ConfigureGroup(Action<RouteGroupBuilder> configure)
        {
            ConfigGroup = configure;
            return this;
        }

        public void Map(RouteGroupBuilder group)
        {
            group = group.MapGroup(RoutePrefix);
            ConfigGroup?.Invoke(group);

            if (ConfigAll.Active)
            {
                MapGetList(group);
                MapGetById(group);
                MapPost(group);
                MapPut(group);
                MapDelete(group);
            }
        }

        private void ExecuteOptions(RouteHandlerBuilder app, EndpointOptions? specific)
        {
            ConfigAll.Builder?.Invoke(app);
            specific?.Builder?.Invoke(app);
        }

        private void MapGetList(IEndpointRouteBuilder app)
        {
            if (ConfigGetList.Active)
            {
                var builder = app.MapGet("", async Task<Ok<List<TEntity>>> ([FromServices] TDbContext context) =>
                {
                    var list = await context.Set<TEntity>().ToListAsync();
                    return TypedResults.Ok(list);
                });

                ExecuteOptions(builder, ConfigGetList);
            }
        }

        private void MapGetById(IEndpointRouteBuilder app)
        {
            if (ConfigGetById.Active)
            {
                var builder = app.MapGet("/{id}", async Task<Results<Ok<TEntity>, NotFound>> ([FromRoute] TKey id, [FromServices] TDbContext context) =>
                {
                    var entity = await context.Set<TEntity>().FindAsync(id);
                    if (entity is null)
                        return TypedResults.NotFound();

                    return TypedResults.Ok(entity);
                });

                ExecuteOptions(builder, ConfigGetById);
            }
        }

        private void MapPost(IEndpointRouteBuilder app)
        {
            if (ConfigPost.Active)
            {
                var builder = app.MapPost("", async Task<Created<TEntity>> ([FromBody] TEntity entity, [FromServices] TDbContext context) =>
                {
                    context.Set<TEntity>().Add(entity);
                    await context.SaveChangesAsync();
                    return TypedResults.Created($"/{entity.Id?.ToString()}", entity);
                });

                ExecuteOptions(builder, ConfigPost);
            }
        }

        private void MapPut(IEndpointRouteBuilder app)
        {
            if (ConfigPut.Active)
            {
                var builder = app.MapPut("/{id}", async Task<Results<NoContent, NotFound>> ([FromRoute] TKey id, [FromBody] TEntity updatedEntity, [FromServices] TDbContext context) =>
                {
                    var entity = await context.Set<TEntity>().FindAsync(id);
                    if (entity is null)
                        return TypedResults.NotFound();
                    context.Entry(entity).CurrentValues.SetValues(updatedEntity);
                    await context.SaveChangesAsync();
                    return TypedResults.NoContent();
                });

                ExecuteOptions(builder, ConfigPut);
            }
        }

        private void MapDelete(IEndpointRouteBuilder app)
        {
            if (ConfigDelete.Active)
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

                ExecuteOptions(builder, ConfigDelete);
            }
        }
    }
}
