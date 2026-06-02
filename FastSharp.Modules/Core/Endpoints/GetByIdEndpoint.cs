using FastSharp.Modules.Configuration;
using FastSharp.Modules.Logging;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FastSharp.Modules.Core.Endpoints;

internal class GetByIdEndpoint<TDbContext, TEntity, TKey>(Func<TKey, Expression<Func<TEntity, bool>>> predicateFactory)
    : GenericEndpointBase<TDbContext, TEntity>
    where TEntity : class
    where TDbContext : DbContext
{
    protected readonly Func<TKey, Expression<Func<TEntity, bool>>> _predicateFactory = predicateFactory;

    public override void Map(RouteGroupBuilder app, EndpointOptions allOptions)
    {
        if (_options.Active)
        {
            var builder = app.MapGet("/{id}", async Task<Results<Ok<TEntity>, NotFound>> (
                [FromRoute] TKey id,
                [FromServices] TDbContext context,
                [FromServices] ILogger<FastSharpEngine> logger) =>
            {
                using var scope = LoggingScope.BeginEntityScope(logger, EntityName, id!);
                FastSharpLogger.LogGetById(logger, EntityName, LoggingScope.FormatId(id));

                var entity = await context.Set<TEntity>().AsNoTracking().Where(_predicateFactory(id)).FirstOrDefaultAsync();
                if (entity is null)
                {
                    FastSharpLogger.LogEntityNotFound(logger, EntityName, LoggingScope.FormatId(id));
                    return TypedResults.NotFound();
                }

                FastSharpLogger.LogGetByIdSuccess(logger, EntityName, LoggingScope.FormatId(id));
                return TypedResults.Ok(entity);
            });

            InvokeBuilders(builder, allOptions, _options);
        }
    }
}

// With DTO configured.
internal class GetByIdEndpoint<TDbContext, TEntity, TKey, TDto>(Func<TKey, Expression<Func<TEntity, bool>>> predicateFactory)
    : GetByIdEndpoint<TDbContext, TEntity, TKey>(predicateFactory)
    where TEntity : class
    where TDbContext : DbContext
    where TDto : class
{
    public override void Map(RouteGroupBuilder app, EndpointOptions allOptions)
    {
        if (_options.Active)
        {
            var builder = app.MapGet("/{id}", async Task<Results<Ok<TDto>, NotFound>> (
                [FromRoute] TKey id,
                [FromServices] TDbContext context,
                [FromServices] ILogger<FastSharpEngine> logger) =>
            {
                using var scope = LoggingScope.BeginEntityScope(logger, EntityName, id!);
                FastSharpLogger.LogGetById(logger, EntityName, LoggingScope.FormatId(id));

                var entity = await context.Set<TEntity>()
                    .AsNoTracking()
                    .Where(_predicateFactory(id))
                    .ProjectToType<TDto>()
                    .FirstOrDefaultAsync();

                if (entity is null)
                {
                    FastSharpLogger.LogEntityNotFound(logger, EntityName, LoggingScope.FormatId(id));
                    return TypedResults.NotFound();
                }

                FastSharpLogger.LogGetByIdSuccess(logger, EntityName, LoggingScope.FormatId(id));
                return TypedResults.Ok(entity);
            });

            InvokeBuilders(builder, allOptions, _options);
        }
    }
}
