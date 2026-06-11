using FastSharp.Modules.Configuration;
using FastSharp.Modules.Logging;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FastSharp.Modules.Core.Endpoints;

internal class UpdateEndpoint<TDbContext, TEntity, TKey>(
    Func<TKey, Expression<Func<TEntity, bool>>> predicateFactory,
    Expression<Func<TEntity, TKey>> idSelector)
    : GenericEndpointBase<TDbContext, TEntity>
    where TEntity : class
    where TDbContext : DbContext
{
    protected readonly Func<TKey, Expression<Func<TEntity, bool>>> _predicateFactory = predicateFactory;

    // Compiled once so the body id can be read and compared against the route id.
    protected readonly Func<TEntity, TKey> _idAccessor = idSelector.Compile();

    protected async Task<Results<NoContent, NotFound, BadRequest<string>>> UpdateEntityAsync(
        TKey id, TDbContext context, ILogger logger, Action<TEntity> applyChanges, CancellationToken ct)
    {
        FastSharpLogger.LogUpdatingEntity(logger, EntityName, LoggingScope.FormatId(id));
        try
        {
            var entity = await context.Set<TEntity>().Where(_predicateFactory(id)).FirstOrDefaultAsync(ct);
            if (entity is null)
            {
                FastSharpLogger.LogEntityNotFound(logger, EntityName, LoggingScope.FormatId(id));
                return TypedResults.NotFound();
            }

            applyChanges(entity);
            await context.SaveChangesAsync(ct);

            FastSharpLogger.LogUpdatedEntity(logger, EntityName, LoggingScope.FormatId(id));
            return TypedResults.NoContent();
        }
        catch (DbUpdateException ex)
        {
            FastSharpLogger.LogPersistenceError(logger, ex, EntityName, "Update");
            throw;
        }
    }

    public override void Map(RouteGroupBuilder app, EndpointOptions allOptions)
    {
        if (_options.Active)
        {
            var builder = app.MapPut(
                "/{id}",
                async Task<Results<NoContent, NotFound, BadRequest<string>>> (
                    [FromRoute] TKey id,
                    [FromBody] TEntity updatedEntity,
                    [FromServices] TDbContext context,
                    [FromServices] ILogger<FastSharpEngine> logger,
                    CancellationToken ct) =>
                {
                    using var scope = LoggingScope.BeginEntityScope(logger, EntityName, id!);

                    var bodyId = _idAccessor(updatedEntity);
                    if (!EqualityComparer<TKey>.Default.Equals(bodyId, id))
                    {
                        return TypedResults.BadRequest(
                            $"The id in the route ('{id}') does not match the id in the request body ('{bodyId}').");
                    }

                    return await UpdateEntityAsync(id, context, logger,
                        e => context.Entry(e).CurrentValues.SetValues(updatedEntity), ct);
                });

            InvokeBuilders(builder, allOptions, _options);
        }
    }
}

// The request DTO is decoupled from the entity key, so route/body id consistency
// is not validated on this overload.
internal class UpdateEndpoint<TDbContext, TEntity, TKey, TDto>(
    Func<TKey, Expression<Func<TEntity, bool>>> predicateFactory,
    Expression<Func<TEntity, TKey>> idSelector)
    : UpdateEndpoint<TDbContext, TEntity, TKey>(predicateFactory, idSelector)
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
                    [FromServices] TDbContext context,
                    [FromServices] ILogger<FastSharpEngine> logger,
                    CancellationToken ct) =>
                {
                    using var scope = LoggingScope.BeginEntityScope(logger, EntityName, id!);
                    return await UpdateEntityAsync(id, context, logger, e => updatedDto.Adapt(e), ct);
                });

            InvokeBuilders(builder, allOptions, _options);
        }
    }
}
