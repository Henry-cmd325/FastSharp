using FastSharp.Modules.Configuration;
using FastSharp.Modules.Logging;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FastSharp.Modules.Core.Endpoints;

internal class DeleteEndpoint<TDbContext, TEntity, TKey>(Func<TKey, Expression<Func<TEntity, bool>>> predicateFactory)
    : GenericEndpointBase<TDbContext, TEntity>
    where TEntity : class
    where TDbContext : DbContext
{
    private async Task<Results<NoContent, NotFound>> DeleteEntityAsync(TKey id, TDbContext context, ILogger logger, CancellationToken ct)
    {
        FastSharpLogger.LogDeletingEntity(logger, EntityName, LoggingScope.FormatId(id));
        try
        {
            var entity = await context.Set<TEntity>().Where(predicateFactory(id)).FirstOrDefaultAsync(ct);
            if (entity is null)
            {
                FastSharpLogger.LogEntityNotFound(logger, EntityName, LoggingScope.FormatId(id));
                return TypedResults.NotFound();
            }

            context.Set<TEntity>().Remove(entity);
            await context.SaveChangesAsync(ct);

            FastSharpLogger.LogDeletedEntity(logger, EntityName, LoggingScope.FormatId(id));
            return TypedResults.NoContent();
        }
        catch (DbUpdateException ex)
        {
            FastSharpLogger.LogPersistenceError(logger, ex, EntityName, "Delete");
            throw;
        }
    }

    public override void Map(RouteGroupBuilder app, EndpointOptions allOptions)
    {
        if (_options.Active)
        {
            var builder = app.MapDelete("/{id}", async Task<Results<NoContent, NotFound>> (
                [FromRoute] TKey id,
                [FromServices] TDbContext context,
                [FromServices] ILogger<FastSharpEngine> logger,
                CancellationToken ct) =>
            {
                using var scope = LoggingScope.BeginEntityScope(logger, EntityName, id!);
                return await DeleteEntityAsync(id, context, logger, ct);
            });

            InvokeBuilders(builder, allOptions, _options);
        }
    }
}
