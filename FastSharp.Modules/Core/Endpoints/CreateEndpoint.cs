using FastSharp.Modules.Configuration;
using FastSharp.Modules.Logging;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastSharp.Modules.Core.Endpoints;

internal class CreateEndpoint<TDbContext, TEntity, TKey>(string crudPrefix = "")
    : GenericEndpointBase<TDbContext, TEntity>
    where TEntity : class
    where TDbContext : DbContext
{
    protected readonly string _crudPrefix = crudPrefix;

    protected TKey GetPrimaryKeyValue(TDbContext context, TEntity entity)
    {
        // We get the primary key property name from the EF model metadata.
        var keyName = (context.Model.FindEntityType(typeof(TEntity))
            ?.FindPrimaryKey()
            ?.Properties
            .Select(x => x.Name)
            .FirstOrDefault()) ?? throw new Exception("PK not found");

        // We access the value through the EF entry (Entry)
        // This is very fast and does not use .Compile()
        return (TKey)context.Entry(entity).Property(keyName).CurrentValue!;
    }

    protected async Task<string> PersistEntityAsync(TDbContext context, TEntity entity, ILogger logger, CancellationToken ct)
    {
        FastSharpLogger.LogCreatingEntity(logger, EntityName);
        try
        {
            context.Set<TEntity>().Add(entity);
            await context.SaveChangesAsync(ct);

            var entityId = LoggingScope.FormatId(GetPrimaryKeyValue(context, entity));
            FastSharpLogger.LogCreatedEntity(logger, EntityName, entityId);
            return entityId;
        }
        catch (DbUpdateException ex)
        {
            FastSharpLogger.LogPersistenceError(logger, ex, EntityName, "Create");
            throw;
        }
    }

    public override void Map(RouteGroupBuilder app, EndpointOptions allOptions)
    {
        if (_options.Active)
        {
            var builder = app.MapPost("/", async Task<Created<TEntity>> (
                [FromBody] TEntity entity,
                [FromServices] TDbContext context,
                [FromServices] ILogger<FastSharpEngine> logger,
                CancellationToken ct) =>
            {
                using var scope = LoggingScope.BeginEntityScope(logger, EntityName);
                var entityId = await PersistEntityAsync(context, entity, logger, ct);
                return TypedResults.Created($"{_crudPrefix}/{entityId}", entity);
            });

            InvokeBuilders(builder, allOptions, _options);
        }
    }
}

// With DTO.
internal class CreateEndpoint<TDbContext, TEntity, TKey, TDto>(string crudPrefix = "")
    : CreateEndpoint<TDbContext, TEntity, TKey>(crudPrefix)
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
                [FromServices] ILogger<FastSharpEngine> logger,
                CancellationToken ct) =>
            {
                using var scope = LoggingScope.BeginEntityScope(logger, EntityName);
                var entity = dto.Adapt<TEntity>();
                var entityId = await PersistEntityAsync(context, entity, logger, ct);
                dto = entity.Adapt<TDto>();
                return TypedResults.Created($"{_crudPrefix}/{entityId}", dto);
            });

            InvokeBuilders(builder, allOptions, _options);
        }
    }
}

// With request and response DTOs.
internal class CreateEndpoint<TDbContext, TEntity, TKey, TRequest, TResponse>(string crudPrefix = "")
    : CreateEndpoint<TDbContext, TEntity, TKey>(crudPrefix)
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
                [FromServices] ILogger<FastSharpEngine> logger,
                CancellationToken ct) =>
            {
                using var scope = LoggingScope.BeginEntityScope(logger, EntityName);
                var entity = request.Adapt<TEntity>();
                var entityId = await PersistEntityAsync(context, entity, logger, ct);
                var response = entity.Adapt<TResponse>();
                return TypedResults.Created($"{_crudPrefix}/{entityId}", response);
            });

            InvokeBuilders(builder, allOptions, _options);
        }
    }
}
