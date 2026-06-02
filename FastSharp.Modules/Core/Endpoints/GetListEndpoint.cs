using FastSharp.Models;
using FastSharp.Modules.Configuration;
using FastSharp.Modules.Logging;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FastSharp.Modules.Core.Endpoints;

internal class GetListEndpoint<TDbContext, TEntity, TKey>(Expression<Func<TEntity, TKey>> idSelector)
    : GenericEndpointBase<TDbContext, TEntity>
    where TEntity : class
    where TDbContext : DbContext
{
    protected readonly Expression<Func<TEntity, TKey>> _idSelector = idSelector;

    protected static (IResult? error, int page, int pageSize) ValidatePagination(int? page, int? pageSize)
    {
        int p = page ?? 1;
        int ps = pageSize ?? 10;
        const int maxPageSize = 100;

        if (p < 1)
            return (TypedResults.BadRequest("Page must be greater than or equal to 1."), 0, 0);

        if (ps < 1 || ps > maxPageSize)
            return (TypedResults.BadRequest($"PageSize must be between 1 and {maxPageSize}."), 0, 0);

        return (null, p, ps);
    }

    protected async Task<IResult> FetchListAsync<TResult>(
        TDbContext context, ILogger logger,
        int? page, int? pageSize,
        Func<IQueryable<TEntity>, IQueryable<TResult>> project)
    {
        if (page.HasValue || pageSize.HasValue)
        {
            var (error, p, ps) = ValidatePagination(page, pageSize);
            if (error is not null) return error;

            FastSharpLogger.LogGetListPaged(logger, EntityName, p, ps);

            var query = context.Set<TEntity>().AsNoTracking();
            var totalItems = await query.CountAsync();
            var list = await project(query.OrderBy(_idSelector).Skip((p - 1) * ps).Take(ps)).ToListAsync();

            return TypedResults.Ok(new PagedResult<TResult>(list, totalItems, p, ps));
        }

        FastSharpLogger.LogGetListAll(logger, EntityName);
        var allItems = await project(context.Set<TEntity>().AsNoTracking()).ToListAsync();
        return TypedResults.Ok(allItems);
    }

    public override void Map(RouteGroupBuilder app, EndpointOptions allOptions)
    {
        if (_options.Active)
        {
            var builder = app.MapGet("/", async Task<IResult> (
                [FromServices] TDbContext context,
                [FromServices] ILogger<FastSharpEngine> logger,
                [FromQuery] int? page,
                [FromQuery] int? pageSize) =>
            {
                using var scope = LoggingScope.BeginEntityScope(logger, EntityName);
                return await FetchListAsync(context, logger, page, pageSize, q => q);
            });

            InvokeBuilders(builder, allOptions, _options);
        }
    }
}

// With DTO
internal class GetListEndpoint<TDbContext, TEntity, TKey, TDto>(Expression<Func<TEntity, TKey>> idSelector)
    : GetListEndpoint<TDbContext, TEntity, TKey>(idSelector)
    where TEntity : class
    where TDbContext : DbContext
    where TDto : class
{
    public override void Map(RouteGroupBuilder app, EndpointOptions allOptions)
    {
        if (_options.Active)
        {
            var builder = app.MapGet("/", async Task<IResult> (
                [FromServices] TDbContext context,
                [FromServices] ILogger<FastSharpEngine> logger,
                [FromQuery] int? page,
                [FromQuery] int? pageSize) =>
            {
                using var scope = LoggingScope.BeginEntityScope(logger, EntityName);
                return await FetchListAsync(context, logger, page, pageSize, q => q.ProjectToType<TDto>());
            });

            InvokeBuilders(builder, allOptions, _options);
        }
    }
}
