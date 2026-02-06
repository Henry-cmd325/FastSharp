using FastSharp.Controllers.Configuration;
using FastSharp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastSharp.Controllers
{
    public abstract class FastController<TDbContext, TModel, TIdentity> : IFastController
        where TModel : class, IModel<TIdentity> 
        where TDbContext : DbContext
    {
        private readonly string ControllerName;
        private readonly List<Type> _controllerEndpoints = [];
        private readonly CRUDOptions _controllerOptions = new();
        private Action<RouteGroupBuilder>? _groupConfiguration;

        protected FastController()
        {
            ControllerName = this.GetType().Name;

            if (ControllerName.EndsWith("Controller", StringComparison.OrdinalIgnoreCase))
            {
                ControllerName = ControllerName[..^"Controller".Length].ToLowerInvariant();
            }
        }

        protected void ConfigureGroup(Action<RouteGroupBuilder> configure) => _groupConfiguration = configure;
        protected void ConfigureCRUD(Action<CRUDOptions> configure) => configure(_controllerOptions);
        
        public void Map(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup($"/api/{ControllerName}").WithTags(ControllerName);
            _groupConfiguration?.Invoke(group);

            if (_controllerOptions.ConfigAll.Active)
            {
                MapGetList(group);
                MapGetById(group);
                MapPost(group);
                MapPut(group);
                MapDelete(group);
            }

            using var scope = app.ServiceProvider.CreateScope();
            var provider = scope.ServiceProvider;

            foreach (var endpointType in _controllerEndpoints)
            {
                var endpoint = (IFastEndpoint)provider.GetRequiredService(endpointType);
                endpoint.Map(group);
            }
        }

        private void ExecuteOptions(RouteHandlerBuilder app, EndpointOptions? specific)
        {
            _controllerOptions.ConfigAll.Builder?.Invoke(app);
            specific?.Builder?.Invoke(app);
        }

        private void MapGetList(IEndpointRouteBuilder app)
        {
            if (_controllerOptions.ConfigGetList.Active)
            {
                var builder = app.MapGet("", async ([FromServices]TDbContext context) =>
                {
                    return await context.Set<TModel>().ToListAsync();
                });

                ExecuteOptions(builder, _controllerOptions.ConfigGetList);
            }
        }

        private void MapGetById(IEndpointRouteBuilder app)
        {
            if (_controllerOptions.ConfigGetById.Active)
            {
                var builder = app.MapGet($"/{{id}}", async ([FromRoute]TIdentity id, [FromServices]TDbContext context) =>
                {
                    var entity = await context.Set<TModel>().FindAsync(id);
                    if (entity is null)
                        return Results.NotFound();

                    return Results.Ok(entity);
                });

                ExecuteOptions(builder, _controllerOptions.ConfigGetById);
            }
        }

        private void MapPost(IEndpointRouteBuilder app)
        {
            if (_controllerOptions.ConfigPost.Active)
            {
                var builder = app.MapPost("", async ([FromBody]TModel entity, [FromServices]TDbContext context) =>
                {
                    context.Set<TModel>().Add(entity);
                    await context.SaveChangesAsync();
                    return Results.Created($"/api/{ControllerName}/{entity.Id?.ToString()}", entity);
                });

                ExecuteOptions(builder, _controllerOptions.ConfigPost);
            }
        }

        private void MapPut(IEndpointRouteBuilder app)
        {
            if (_controllerOptions.ConfigPut.Active)
            {
                var builder = app.MapPut($"/{{id}}", async ([FromRoute]TIdentity id, [FromBody]TModel updatedEntity, [FromServices]TDbContext context) =>
                {
                    var entity = await context.Set<TModel>().FindAsync(id);
                    if (entity is null)
                        return Results.NotFound();
                    
                    context.Entry(entity).CurrentValues.SetValues(updatedEntity);
                    await context.SaveChangesAsync();
                    return Results.NoContent();
                });

                ExecuteOptions(builder, _controllerOptions.ConfigPut);
            }
        }

        private void MapDelete(IEndpointRouteBuilder app)
        {
            if (_controllerOptions.ConfigDelete.Active)
            {
                var builder = app.MapDelete($"/{{id}}", async ([FromRoute]TIdentity id, [FromServices]TDbContext context) =>
                {
                    var entity = await context.Set<TModel>().FindAsync(id);
                    if (entity is null)
                    {
                        return Results.NotFound();
                    }

                    context.Set<TModel>().Remove(entity);
                    await context.SaveChangesAsync();
                    return Results.NoContent();
                });

                ExecuteOptions(builder, _controllerOptions.ConfigDelete);
            }
        }

        protected void IncludeNamespace<T>() where T : IFastEndpoint
        {
            var ns = typeof(T).Namespace;
            // Escaneamos el ensamblado buscando clases que:
            // 1. Estén en el namespace indicado.
            // 2. Implementen IFastEndpoint.
            var targetAssembly = typeof(T).Assembly;
            var types = targetAssembly
                .GetTypes()
                .Where(p => typeof(IFastEndpoint).IsAssignableFrom(p)
                            && p.IsClass
                            && p.Namespace?.StartsWith(ns ?? string.Empty) == true
                            && !p.IsAbstract);

                // Guardamos el tipo para que el motor de FastSharp lo ejecute
                // al momento de construir las Minimal APIs
                _controllerEndpoints.AddRange(types);
        }

        protected void Include<TEndpoint>()
        {
            _controllerEndpoints.Add(typeof(TEndpoint));
        }
    }
}