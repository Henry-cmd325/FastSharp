using FastSharp.Models;
using FastSharp.Modules.Configuration;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastSharp.Modules.Endpoints;

public class CreateEndpoint<TDbContext, TEntity, TKey>
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
            var builder = app.MapPost("", async Task<Created<TEntity>> ([FromBody] TEntity entity, [FromServices] TDbContext context) =>
            {
                context.Set<TEntity>().Add(entity);
                await context.SaveChangesAsync();
                return TypedResults.Created($"/{entity.Id?.ToString()}", entity);
            });

            allOptions.Builder?.Invoke(builder);
            _options.Builder?.Invoke(builder);
        }
    }
}

// With DTO.
public class CreateEndpoint<TDbContext, TEntity, TKey, TDto> : CreateEndpoint<TDbContext, TEntity, TKey>
    where TEntity : class, IModel<TKey>
    where TDbContext : DbContext
    where TDto : class
{
    public override void Handle(RouteGroupBuilder app, EndpointOptions allOptions)
    {
        if (_options.Active)
        {
            var builder = app.MapPost("", async Task<Created<TDto>> ([FromBody] TDto dto, [FromServices] TDbContext context) =>
            {
                var entity = dto.Adapt<TEntity>();
                context.Set<TEntity>().Add(entity);
                await context.SaveChangesAsync();
                return TypedResults.Created($"/{entity.Id?.ToString()}", dto);
            });

            allOptions.Builder?.Invoke(builder);
            _options.Builder?.Invoke(builder);
        }
    }
}

// With request and response DTOs.
public class CreateEndpoint<TDbContext, TEntity, TKey, TRequest, TResponse> : CreateEndpoint<TDbContext, TEntity, TKey>
    where TEntity : class, IModel<TKey>
    where TDbContext : DbContext
    where TRequest : class
    where TResponse : class
{
    public override void Handle(RouteGroupBuilder app, EndpointOptions allOptions)
    {
        if (_options.Active)
        {
            var builder = app.MapPost("", async Task<Created<TResponse>> ([FromBody] TRequest request, [FromServices] TDbContext context) =>
            {
                var entity = request.Adapt<TEntity>();
                context.Set<TEntity>().Add(entity);
                await context.SaveChangesAsync();
                var response = entity.Adapt<TResponse>();
                return TypedResults.Created($"/{entity.Id?.ToString()}", response);
            });

            allOptions.Builder?.Invoke(builder);
            _options.Builder?.Invoke(builder);
        }
    }
}
