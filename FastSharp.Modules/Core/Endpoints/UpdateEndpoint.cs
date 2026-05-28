using FastSharp.Modules.Configuration;
using FastSharp.Modules.Logging;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FastSharp.Modules.Core.Endpoints;

public class UpdateEndpoint<TDbContext, TEntity, TKey>(Func<TKey, Expression<Func<TEntity, bool>>> predicateFactory) : IGenericEndpoint
    where TEntity : class
    where TDbContext : DbContext
{
    protected EndpointOptions _options = new();

    protected readonly Func<TKey, Expression<Func<TEntity, bool>>> _predicateFactory = predicateFactory;

    protected static readonly string EntityName = typeof(TEntity).Name;

    public bool IsActive => _options.Active;

    public void Configure(EndpointOptions options)
    {
        _options = options;
    }

    public virtual void Map(RouteGroupBuilder app, EndpointOptions allOptions)
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
                    using (LoggingScope.BeginEntityScope(logger, EntityName, id!))
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

                            context.Entry(entity).CurrentValues.SetValues(updatedEntity);
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
                });

            allOptions.Builder?.Invoke(builder);
            _options.Builder?.Invoke(builder);
        }
    }
}

public class UpdateEndpoint<TDbContext, TEntity, TKey, TDto>(Func<TKey, Expression<Func<TEntity, bool>>> predicateFactory) : UpdateEndpoint<TDbContext, TEntity, TKey>(predicateFactory)
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
                    using (LoggingScope.BeginEntityScope(logger, EntityName, id!))
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

                            updatedDto.Adapt(entity);
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
                });

            allOptions.Builder?.Invoke(builder);
            _options.Builder?.Invoke(builder);
        }
    }
}
