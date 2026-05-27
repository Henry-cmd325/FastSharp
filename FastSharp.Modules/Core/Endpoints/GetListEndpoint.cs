using FastSharp.Models;
using FastSharp.Modules.Configuration;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FastSharp.Modules.Core.Endpoints;

public class GetListEndpoint<TDbContext, TEntity, TKey>(Expression<Func<TEntity, TKey>> idSelector) : IGenericEndpoint
    where TEntity : class
    where TDbContext : DbContext
{
    protected Expression<Func<TEntity, TKey>> IdSelector = idSelector;
    protected EndpointOptions _options = new();

    public void Configure(EndpointOptions options)
    {
        _options = options;
    }

    public virtual void Map(RouteGroupBuilder app, EndpointOptions allOptions)
    {
        if (_options.Active)
        {
            var builder = app.MapGet("/", async Task<IResult> (
                [FromServices] TDbContext context,
                [FromQuery] int? page,
                [FromQuery] int? pageSize) =>
            {
                if (page.HasValue || pageSize.HasValue)
                {
                    const int maxPageSize = 100;
                    int p = page ?? 1;
                    int ps = pageSize ?? 10;

                    if (p < 1)
                        return TypedResults.BadRequest("Page must be greater than or equal to 1.");

                    if (ps < 1 || ps > maxPageSize)
                        return TypedResults.BadRequest($"PageSize must be between 1 and {maxPageSize}.");

                    var query = context.Set<TEntity>().AsNoTracking();
                    var totalItems = await query.CountAsync();
                    var list = await query
                        .OrderBy(IdSelector)
                        .Skip((p - 1) * ps)
                        .Take(ps)
                        .ToListAsync();

                    return TypedResults.Ok(new PagedResult<TEntity>(list, totalItems, p, ps));
                }

                var allItems = await context.Set<TEntity>().AsNoTracking().ToListAsync();
                return TypedResults.Ok(allItems);
            });

            allOptions.Builder?.Invoke(builder);
            _options.Builder?.Invoke(builder);
        }
    }
}

// With DTO
public class GetListEndpoint<TDbContext, TEntity, TKey, TDto>(Expression<Func<TEntity, TKey>> idSelector) : GetListEndpoint<TDbContext, TEntity, TKey>(idSelector)
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
                [FromQuery] int? page,
                [FromQuery] int? pageSize) =>
            {
                if (page.HasValue || pageSize.HasValue)
                {
                    const int maxPageSize = 100;
                    int p = page ?? 1;
                    int ps = pageSize ?? 10;

                    if (p < 1)
                        return TypedResults.BadRequest("Page must be greater than or equal to 1.");

                    if (ps < 1 || ps > maxPageSize)
                        return TypedResults.BadRequest($"PageSize must be between 1 and {maxPageSize}.");

                    var query = context.Set<TEntity>().AsNoTracking();
                    var totalItems = await query.CountAsync();
                    var list = await query
                        .OrderBy(IdSelector)
                        .Skip((p - 1) * ps)
                        .Take(ps)
                        .ProjectToType<TDto>()
                        .ToListAsync();

                    return TypedResults.Ok(new PagedResult<TDto>(list, totalItems, p, ps));
                }

                var allItems = await context.Set<TEntity>()
                    .AsNoTracking()
                    .ProjectToType<TDto>()
                    .ToListAsync();
                return TypedResults.Ok(allItems);
            });

            allOptions.Builder?.Invoke(builder);
            _options.Builder?.Invoke(builder);
        }
    }
}
