using FastSharp.Models;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastSharp.Modules.Configuration;

public class CRUDEndpoints<TDbContext, TEntity, TKey>(string routePrefix = "") : ICrudEndpoints<TDbContext>
    where TEntity : class, IModel<TKey>
    where TDbContext : DbContext
{
    internal Action<RouteGroupBuilder>? ConfigGroup;

    internal EndpointOptions ConfigGetPaged = new();
    internal EndpointOptions ConfigGetList = new();
    internal EndpointOptions ConfigGetById = new();
    internal EndpointOptions ConfigCreate = new();
    internal EndpointOptions ConfigUpdate = new();
    internal EndpointOptions ConfigDelete = new();
    internal EndpointOptions ConfigAll = new();

    internal string RoutePrefix { get; set; } = routePrefix;

    public Type EntityType => typeof(TEntity);
    public Type KeyType => typeof(TKey);

    public ICrudEndpoints<TDbContext> DisableEndpoint(GenericEndpoint endpointName)
    {
        switch (endpointName)
        {
            case GenericEndpoint.GetPaged:
                ConfigGetPaged.Active = false;
                break;
            case GenericEndpoint.GetList:
                ConfigGetList.Active = false;
                break;
            case GenericEndpoint.GetById:
                ConfigGetById.Active = false;
                break;
            case GenericEndpoint.Create:
                ConfigCreate.Active = false;
                break;
            case GenericEndpoint.Update:
                ConfigUpdate.Active = false;
                break;
            case GenericEndpoint.Delete:
                ConfigDelete.Active = false;
                break;
            case GenericEndpoint.All:
                ConfigGetPaged.Active = false;
                ConfigGetList.Active = false;
                ConfigGetById.Active = false;
                ConfigCreate.Active = false;
                ConfigUpdate.Active = false;
                ConfigDelete.Active = false;
                break;
        }

        return this;
    }

    public ICrudEndpoints<TDbContext> ConfigureEndpoint(GenericEndpoint endpointName, Action<RouteHandlerBuilder> configure)
    {
        switch (endpointName)
        {
            case GenericEndpoint.GetPaged:
                ConfigGetPaged.Builder = b => configure(b);
                break;
            case GenericEndpoint.GetList:
                ConfigGetList.Builder = b => configure(b);
                break;
            case GenericEndpoint.GetById:
                ConfigGetById.Builder = b => configure(b);
                break;
            case GenericEndpoint.Create:
                ConfigCreate.Builder = b => configure(b);
                break;
            case GenericEndpoint.Update:
                ConfigUpdate.Builder = b => configure(b);
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

    public void ConfigureAll(Action<RouteHandlerBuilder> configure)
    {
        ConfigAll.Active = true;
        ConfigAll.Builder = b => configure(b);
    }

    public void ConfigureAll<TDto>(Action<RouteHandlerBuilder> configure)
    {
        ConfigAll.Active = true;
        ConfigAll.RequestType = typeof(TDto);
        ConfigAll.ResponseType = typeof(TDto);
        ConfigAll.Builder = b => configure(b);
    }

    public void ConfigureAll<TRequest, TResponse>(Action<RouteHandlerBuilder> configure)
    {
        ConfigAll.Active = true;
        ConfigAll.RequestType = typeof(TRequest);
        ConfigAll.ResponseType = typeof(TResponse);
    }

    public void Get(Action<RouteHandlerBuilder> configure)
    {
        ConfigAll.Active = true;
        ConfigGetById.Active = true;
        ConfigGetById.Builder = b => configure(b);
    }

    public void Get<TDto>(Action<RouteHandlerBuilder> configure)
    {
        ConfigGetById.Active = true;
        ConfigGetById.ResponseType = typeof(TDto);
        ConfigGetById.Builder = b => configure(b);
    }

    public void GetList(Action<RouteHandlerBuilder> configure)
    {
        ConfigGetList.Active = true;
        ConfigGetList.Builder = b => configure(b);
    }

    public void GetList<TDto>(Action<RouteHandlerBuilder> configure)
    {
        ConfigGetList.Active = true;
        ConfigGetList.ResponseType = typeof(List<TDto>);
        ConfigGetList.Builder = b => configure(b);
    }

    public void GetPaged(Action<RouteHandlerBuilder> configure)
    {
        ConfigGetPaged.Active = true;
        ConfigGetPaged.Builder = b => configure(b);
    }

    public void GetPaged<TDto>(Action<RouteHandlerBuilder> configure)
    {
        ConfigGetPaged.Active = true;
        ConfigGetPaged.ResponseType = typeof(TDto);
        ConfigGetPaged.Builder = b => configure(b);
    }

    public void Create(Action<RouteHandlerBuilder> configure)
    {
        ConfigCreate.Active = true;
        ConfigCreate.Builder = b => configure(b);
    }

    public void Create<TDto>(Action<RouteHandlerBuilder> configure)
    {
        ConfigCreate.Active = true;
        ConfigCreate.RequestType = typeof(TDto);
        ConfigCreate.ResponseType = typeof(TDto);
        ConfigCreate.Builder = b => configure(b);
    }

    public void Create<TRequest, TResponse>(Action<RouteHandlerBuilder> configure)
    {
        ConfigCreate.Active = true;
        ConfigCreate.RequestType = typeof(TRequest);
        ConfigCreate.ResponseType = typeof(TResponse);
        ConfigCreate.Builder = b => configure(b);
    }

    public void Update(Action<RouteHandlerBuilder> configure)
    {
        ConfigUpdate.Active = true;
        ConfigUpdate.Builder = b => configure(b);
    }

    public void Update<TDto>(Action<RouteHandlerBuilder> configure)
    {
        ConfigUpdate.Active = true;
        ConfigUpdate.RequestType = typeof(TDto);
        ConfigUpdate.Builder = b => configure(b);
    }

    public void Delete(Action<RouteHandlerBuilder> configure)
    {
        ConfigDelete.Active = true;
        ConfigDelete.Builder = b => configure(b);
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

        MapPaged(group);
        MapGetList(group);
        MapGetById(group);
        MapPost(group);
        MapPut(group);
        MapDelete(group);
    }

    private void ExecuteOptions(RouteHandlerBuilder app, EndpointOptions? specific)
    {
        ConfigAll.Builder?.Invoke(app);
        specific?.Builder?.Invoke(app);
    }

    private void MapPaged(IEndpointRouteBuilder app)
    {
        if (ConfigGetPaged.Active)
        {
            var builder = app.MapGet("/paged", async Task<Results<Ok<PagedResult<TEntity>>, BadRequest<string>>> (
                [FromServices] TDbContext context,
                [FromQuery] int page = 1,
                [FromQuery] int pageSize = 10) =>
            {
                const int maxPageSize = 100;
                if (page < 1)
                {
                    return TypedResults.BadRequest("Page must be greater than or equal to 1.");
                }

                if (pageSize < 1 || pageSize > maxPageSize)
                {
                    return TypedResults.BadRequest($"PageSize must be between 1 and {maxPageSize}.");
                }

                if (ConfigGetPaged.ResponseType != null)
                {
                    var query = context.Set<TEntity>().AsNoTracking().ProjectToType(ConfigGetPaged.ResponseType) as IQueryable<dynamic>;
                    var list = await query
                        .Cast<object>()
                        .OrderBy(entity => EF.Property<TKey>(entity, "Id"))
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();

                    var result = new PagedResult<object>(list, await query.CountAsync(), page, pageSize);

                    return TypedResults.Ok(result);
                }
                else
                {
                    var query = context.Set<TEntity>().AsNoTracking();
                    var list = await query
                        .OrderBy(entity => entity.Id)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();
                    var result = new PagedResult<TEntity>(list, await query.CountAsync(), page, pageSize);
                    return TypedResults.Ok(result);
                }
            });

            ExecuteOptions(builder, ConfigGetPaged);
        }
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
        if (ConfigCreate.Active)
        {
            var builder = app.MapPost("", async Task<Created<TEntity>> ([FromBody] TEntity entity, [FromServices] TDbContext context) =>
            {
                context.Set<TEntity>().Add(entity);
                await context.SaveChangesAsync();
                return TypedResults.Created($"/{entity.Id?.ToString()}", entity);
            });

            ExecuteOptions(builder, ConfigCreate);
        }
    }

    private void MapPut(IEndpointRouteBuilder app)
    {
        if (ConfigUpdate.Active)
        {
            var builder = app.MapPut(
                "/{id}",
                async Task<Results<NoContent, NotFound, BadRequest<string>>> (
                    [FromRoute] TKey id,
                    [FromBody] TEntity updatedEntity,
                    [FromServices] TDbContext context) =>
                {
                    if (!EqualityComparer<TKey>.Default.Equals(id, updatedEntity.Id))
                    {
                        return TypedResults.BadRequest("Route id must match body id.");
                    }

                    var entity = await context.Set<TEntity>().FindAsync(id);
                    if (entity is null)
                        return TypedResults.NotFound();
                    context.Entry(entity).CurrentValues.SetValues(updatedEntity);
                    await context.SaveChangesAsync();
                    return TypedResults.NoContent();
                });

            ExecuteOptions(builder, ConfigUpdate);
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
