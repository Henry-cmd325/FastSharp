using FastSharp.Modules.Configuration;
using FastSharp.Modules.Logging;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mapster;

namespace FastSharp.Modules.Core.Endpoints;

public class CreateEndpoint<TDbContext, TEntity, TKey>() : IGenericEndpoint
    where TEntity : class
    where TDbContext : DbContext
{
    protected EndpointOptions _options = new();

    protected static readonly string EntityName = typeof(TEntity).Name;

    public bool IsActive => _options.Active;

    public void Configure(EndpointOptions options)
    {
        _options = options;
    }

    protected TKey GetPrimaryKeyValue(TDbContext context, TEntity entity)
    {
        // We get the primary key property name from the EF model metadata.
        var keyName = (context.Model.FindEntityType(typeof(TEntity))
            ?.FindPrimaryKey()
            ?.Properties
            .Select(x => x.Name)
            .FirstOrDefault()) ?? throw new Exception("No se encontró PK");

        // We access the value through the EF entry (Entry)
        // This is very fast and does not use .Compile()
        return (TKey)context.Entry(entity).Property(keyName).CurrentValue!;
    }

    public virtual void Map(RouteGroupBuilder app, EndpointOptions allOptions)
    {
        if (_options.Active)
        {
            var builder = app.MapPost("/", async Task<Created<TEntity>> (
                [FromBody] TEntity entity,
                [FromServices] TDbContext context,
                [FromServices] ILogger<FastSharpEngine> logger) =>
            {
                using (LoggingScope.BeginEntityScope(logger, EntityName))
                {
                    FastSharpLogger.LogCreatingEntity(logger, EntityName);

                    try
                    {
                        context.Set<TEntity>().Add(entity);
                        await context.SaveChangesAsync();

                        var entityId = LoggingScope.FormatId(GetPrimaryKeyValue(context, entity));
                        FastSharpLogger.LogCreatedEntity(logger, EntityName, entityId);

                        return TypedResults.Created($"/{entityId}", entity);
                    }
                    catch (DbUpdateException ex)
                    {
                        FastSharpLogger.LogPersistenceError(logger, ex, EntityName, "Create");
                        throw;
                    }
                }
            });

            allOptions.Builder?.Invoke(builder);
            _options.Builder?.Invoke(builder);
        }
    }
}

// With DTO.
public class CreateEndpoint<TDbContext, TEntity, TKey, TDto>() : CreateEndpoint<TDbContext, TEntity, TKey>
    where TEntity : class
    where TDbContext : DbContext
    where TDto : class
{
    public override void Map(RouteGroupBuilder app, EndpointOptions allOptions)
    {
        if (_options.Active)
        {
            var builder = app.MapPost("/", async Task<Created<TDto>> (
                [FromBody] TDto dto,
                [FromServices] TDbContext context,
                [FromServices] ILogger<FastSharpEngine> logger) =>
            {
                using (LoggingScope.BeginEntityScope(logger, EntityName))
                {
                    FastSharpLogger.LogCreatingEntity(logger, EntityName);

                    try
                    {
                        var entity = dto.Adapt<TEntity>();
                        context.Set<TEntity>().Add(entity);
                        await context.SaveChangesAsync();

                        var entityId = LoggingScope.FormatId(GetPrimaryKeyValue(context, entity));
                        FastSharpLogger.LogCreatedEntity(logger, EntityName, entityId);

                        return TypedResults.Created($"/{entityId}", dto);
                    }
                    catch (DbUpdateException ex)
                    {
                        FastSharpLogger.LogPersistenceError(logger, ex, EntityName, "Create");
                        throw;
                    }
                }
            });

            allOptions.Builder?.Invoke(builder);
            _options.Builder?.Invoke(builder);
        }
    }
}

// With request and response DTOs.
public class CreateEndpoint<TDbContext, TEntity, TKey, TRequest, TResponse>() : CreateEndpoint<TDbContext, TEntity, TKey>
    where TEntity : class
    where TDbContext : DbContext
    where TRequest : class
    where TResponse : class
{
    public override void Map(RouteGroupBuilder app, EndpointOptions allOptions)
    {
        if (_options.Active)
        {
            var builder = app.MapPost("/", async Task<Created<TResponse>> (
                [FromBody] TRequest request,
                [FromServices] TDbContext context,
                [FromServices] ILogger<FastSharpEngine> logger) =>
            {
                using (LoggingScope.BeginEntityScope(logger, EntityName))
                {
                    FastSharpLogger.LogCreatingEntity(logger, EntityName);

                    try
                    {
                        var entity = request.Adapt<TEntity>();
                        context.Set<TEntity>().Add(entity);
                        await context.SaveChangesAsync();

                        var response = entity.Adapt<TResponse>();
                        var entityId = LoggingScope.FormatId(GetPrimaryKeyValue(context, entity));
                        FastSharpLogger.LogCreatedEntity(logger, EntityName, entityId);

                        return TypedResults.Created($"/{entityId}", response);
                    }
                    catch (DbUpdateException ex)
                    {
                        FastSharpLogger.LogPersistenceError(logger, ex, EntityName, "Create");
                        throw;
                    }
                }
            });

            allOptions.Builder?.Invoke(builder);
            _options.Builder?.Invoke(builder);
        }
    }
}
