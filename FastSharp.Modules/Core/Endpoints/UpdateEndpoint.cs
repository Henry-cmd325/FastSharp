using FastSharp.Modules.Configuration;
using FastSharp.Modules.Logging;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FastSharp.Modules.Core.Endpoints;

internal class UpdateEndpoint<TDbContext, TEntity, TKey>(Func<TKey, Expression<Func<TEntity, bool>>> predicateFactory)
    : GenericEndpointBase<TDbContext, TEntity>
    where TEntity : class
    where TDbContext : DbContext
{
    protected readonly Func<TKey, Expression<Func<TEntity, bool>>> _predicateFactory = predicateFactory;

    protected async Task<Results<NoContent, NotFound, BadRequest<string>>> UpdateEntityAsync(
        TKey id, TDbContext context, ILogger logger, Action<TEntity> applyChanges)
    {
        FastSharpLogger.LogUpdatingEntity(logger, EntityName, LoggingScope.FormatId(id));
        try
        {
            var entity = await context.Set<TEntity>().Where(_predicateFactory(id)).FirstOrDefaultAsync();
            if (entity is null)
            {
                FastSharpLogger.LogEntityNotFound(logger, EntityName, LoggingScope.FormatId(id));
                return TypedResults.NotFound();
            }

            applyChanges(entity);
            await context.SaveChangesAsync();

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
                    [FromServices] ILogger<FastSharpEngine> logger) =>
                {
                    using var scope = LoggingScope.BeginEntityScope(logger, EntityName, id!);
                    return await UpdateEntityAsync(id, context, logger,
                            e => context.Entry(e).CurrentValues.SetValues(updatedEntity));
                });

            InvokeBuilders(builder, allOptions, _options);
        }
    }
}

internal class UpdateEndpoint<TDbContext, TEntity, TKey, TDto>(Func<TKey, Expression<Func<TEntity, bool>>> predicateFactory)
    : UpdateEndpoint<TDbContext, TEntity, TKey>(predicateFactory)
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
                    [FromServices] ILogger<FastSharpEngine> logger) =>
                {
                    using var scope = LoggingScope.BeginEntityScope(logger, EntityName, id!);
                    return await UpdateEntityAsync(id, context, logger, e => updatedDto.Adapt(e));
                });

            InvokeBuilders(builder, allOptions, _options);
        }
    }
}
