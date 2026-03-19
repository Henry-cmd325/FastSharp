using FastSharp.Models;
using FastSharp.Modules.Configuration;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastSharp.Modules.Endpoints;

public class GetPagedEndpoint<TDbContext, TEntity, TKey>
    where TEntity : class, IModel<TKey>
    where TDbContext : DbContext
{
    protected EndpointOptions _options = new();

    public void Configure(EndpointOptions options)
    {
        _options = options;
    }

    public virtual void Handle(RouteGroupBuilder app, EndpointOptions allOptions)
    {
        if (_options.Active)
        {
            var builder = app.MapGet("/paged", async Task<Results<Ok<PagedResult<TEntity>>, BadRequest<string>>> (
                [FromServices] TDbContext context,
                [FromQuery] int page = 1,
                [FromQuery] int pageSize = 10) =>
            {
                const int maxPageSize = 100;
                if (page < 1)
                {
                    return TypedResults.BadRequest("Page must be greater than or equal to 1.");
                }

                if (pageSize < 1 || pageSize > maxPageSize)
                {
                    return TypedResults.BadRequest($"PageSize must be between 1 and {maxPageSize}.");
                }

                var query = context.Set<TEntity>().AsNoTracking();
                var list = await query
                    .OrderBy(entity => entity.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
                var result = new PagedResult<TEntity>(list, await query.CountAsync(), page, pageSize);
                return TypedResults.Ok(result);
            });

            allOptions.Builder?.Invoke(builder);
            _options.Builder?.Invoke(builder);
        }
    }
}

public class GetPagedEndpoint<TDbContext, TEntity, TKey, TDto> : GetPagedEndpoint<TDbContext, TEntity, TKey>
    where TEntity : class, IModel<TKey>
    where TDbContext : DbContext
    where TDto : class
{
    public override void Handle(RouteGroupBuilder app, EndpointOptions allOptions)
    {
        if (_options.Active)
        {
            var builder = app.MapGet("/paged", async Task<Results<Ok<PagedResult<TDto>>, BadRequest<string>>> (
                [FromServices] TDbContext context,
                [FromQuery] int page = 1,
                [FromQuery] int pageSize = 10) =>
            {
                const int maxPageSize = 100;
                if (page < 1)
                {
                    return TypedResults.BadRequest("Page must be greater than or equal to 1.");
                }

                if (pageSize < 1 || pageSize > maxPageSize)
                {
                    return TypedResults.BadRequest($"PageSize must be between 1 and {maxPageSize}.");
                }

                var query = context.Set<TEntity>().AsNoTracking();
                var list = await query
                    .OrderBy(entity => entity.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ProjectToType<TDto>()
                    .ToListAsync();
                var result = new PagedResult<TDto>(list, await query.CountAsync(), page, pageSize);

                return TypedResults.Ok(result);
            });

            allOptions.Builder?.Invoke(builder);
            _options.Builder?.Invoke(builder);
        }
    }
}
