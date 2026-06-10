using FastSharp.Models;
using FastSharp.Modules.Configuration;
using FastSharp.Modules.Logging;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Linq.Expressions;

namespace FastSharp.Modules.Core.Endpoints;

internal class GetListEndpoint<TDbContext, TEntity, TKey>(Expression<Func<TEntity, TKey>> idSelector)
    : GenericEndpointBase<TDbContext, TEntity>
    where TEntity : class
    where TDbContext : DbContext
{
    protected readonly Expression<Func<TEntity, TKey>> _idSelector = idSelector;

    /// <summary>
    /// Per-endpoint override for the maximum page size. When <c>null</c>, the global
    /// <see cref="FastSharpOptions.MaxPageSize"/> is used instead.
    /// </summary>
    internal int? MaxPageSizeOverride { get; set; }

    /// <summary>
    /// Resolves the effective maximum page size, preferring the per-endpoint override over the
    /// global option. Never below 1 so a misconfigured value cannot disable the endpoint.
    /// </summary>
    protected int ResolveMaxPageSize(FastSharpOptions options)
        => Math.Max(1, MaxPageSizeOverride ?? options.MaxPageSize);

    protected static (IResult? error, int page, int pageSize) ValidatePagination(int? page, int? pageSize, int maxPageSize)
    {
        int p = page ?? 1;
        int ps = pageSize ?? Math.Min(10, maxPageSize);

        if (p < 1)
            return (TypedResults.BadRequest("Page must be greater than or equal to 1."), 0, 0);

        if (ps < 1 || ps > maxPageSize)
            return (TypedResults.BadRequest($"PageSize must be between 1 and {maxPageSize}."), 0, 0);

        return (null, p, ps);
    }

    protected async Task<IResult> FetchListAsync<TResult>(
        TDbContext context, ILogger logger,
        int? page, int? pageSize, int maxPageSize,
        Func<IQueryable<TEntity>, IQueryable<TResult>> project,
        CancellationToken ct)
    {
        if (page.HasValue || pageSize.HasValue)
        {
            var (error, p, ps) = ValidatePagination(page, pageSize, maxPageSize);
            if (error is not null) return error;

            FastSharpLogger.LogGetListPaged(logger, EntityName, p, ps);

            var query = context.Set<TEntity>().AsNoTracking();
            var totalItems = await query.CountAsync(ct);
            var list = await project(query.OrderBy(_idSelector).Skip((p - 1) * ps).Take(ps)).ToListAsync(ct);

            return TypedResults.Ok(new PagedResult<TResult>(list, totalItems, p, ps));
        }

        // No pagination parameters: still bounded by the effective max page size to avoid
        // loading the entire table into memory.
        FastSharpLogger.LogGetListAll(logger, EntityName);
        var allItems = await project(
            context.Set<TEntity>().AsNoTracking().OrderBy(_idSelector).Take(maxPageSize)).ToListAsync(ct);
        return TypedResults.Ok(allItems);
    }

    public override void Map(RouteGroupBuilder app, EndpointOptions allOptions)
    {
        if (_options.Active)
        {
            var builder = app.MapGet("/", async Task<IResult> (
                [FromServices] TDbContext context,
                [FromServices] ILogger<FastSharpEngine> logger,
                [FromServices] IOptions<FastSharpOptions> fastSharpOptions,
                [FromQuery] int? page,
                [FromQuery] int? pageSize,
                CancellationToken ct) =>
            {
                int maxPageSize = ResolveMaxPageSize(fastSharpOptions.Value);
                using var scope = LoggingScope.BeginEntityScope(logger, EntityName);
                return await FetchListAsync(context, logger, page, pageSize, maxPageSize, q => q, ct);
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
                [FromServices] IOptions<FastSharpOptions> fastSharpOptions,
                [FromQuery] int? page,
                [FromQuery] int? pageSize,
                CancellationToken ct) =>
            {
                int maxPageSize = ResolveMaxPageSize(fastSharpOptions.Value);
                using var scope = LoggingScope.BeginEntityScope(logger, EntityName);
                return await FetchListAsync<TDto>(context, logger, page, pageSize, maxPageSize, q => q.ProjectToType<TDto>(), ct);
            });

            InvokeBuilders(builder, allOptions, _options);
        }
    }
}
