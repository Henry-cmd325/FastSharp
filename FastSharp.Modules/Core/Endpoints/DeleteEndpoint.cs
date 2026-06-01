using FastSharp.Modules.Configuration;
using FastSharp.Modules.Logging;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FastSharp.Modules.Core.Endpoints;

public class DeleteEndpoint<TDbContext, TEntity, TKey>(Func<TKey, Expression<Func<TEntity, bool>>> predicateFactory) : IGenericEndpoint
    where TEntity : class
    where TDbContext : DbContext
{
    protected EndpointOptions _options = new();

    protected readonly Func<TKey, Expression<Func<TEntity, bool>>> PredicateFactory = predicateFactory;

    private static readonly string _entityName = typeof(TEntity).Name;

    public bool IsActive => _options.Active;

    public void Configure(EndpointOptions options)
    {
        _options = options;
    }

    public virtual void Map(RouteGroupBuilder app, EndpointOptions allOptions)
    {
        if (_options.Active)
        {
            var builder = app.MapDelete("/{id}", async Task<Results<NoContent, NotFound>> (
                [FromRoute] TKey id,
                [FromServices] TDbContext context,
                [FromServices] ILogger<FastSharpEngine> logger) =>
            {
                using (LoggingScope.BeginEntityScope(logger, _entityName, id!))
                {
                    FastSharpLogger.LogDeletingEntity(logger, _entityName, LoggingScope.FormatId(id));

                    try
                    {
                        var entity = await context.Set<TEntity>().Where(PredicateFactory(id)).FirstOrDefaultAsync();
                        if (entity is null)
                        {
                            FastSharpLogger.LogEntityNotFound(logger, _entityName, LoggingScope.FormatId(id));
                            return TypedResults.NotFound();
                        }

                        context.Set<TEntity>().Remove(entity);
                        await context.SaveChangesAsync();

                        FastSharpLogger.LogDeletedEntity(logger, _entityName, LoggingScope.FormatId(id));
                        return TypedResults.NoContent();
                    }
                    catch (DbUpdateException ex)
                    {
                        FastSharpLogger.LogPersistenceError(logger, ex, _entityName, "Delete");
                        throw;
                    }
                }
            });

            allOptions.Builder?.Invoke(builder);
            _options.Builder?.Invoke(builder);
        }
    }
}
