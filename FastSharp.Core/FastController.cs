using FastSharp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastSharp.Controllers
{
    public abstract class FastController<TModel, TIdentity> : IFastEndpoint where TModel : class, IModel<TIdentity>
    {
        private readonly string typeName = typeof(TModel).Name.ToLower();

        private Action<EndpointOptions>? _configGetList;
        private Action<EndpointOptions>? _configGetById;
        private Action<EndpointOptions>? _configPost;
        private Action<EndpointOptions>? _configPut;
        private Action<EndpointOptions>? _configDelete;
        private Action<EndpointOptions>? _configAll;

        protected void ConfigureGetList(Action<EndpointOptions> configuration) => _configGetList = configuration;
        protected void ConfigureGetById(Action<EndpointOptions> configuration) => _configGetById = configuration;
        protected void ConfigurePost(Action<EndpointOptions> configuration) => _configPost = configuration;
        protected void ConfigurePut(Action<EndpointOptions> configuration) => _configPut = configuration;
        protected void ConfigureDelete(Action<EndpointOptions> configuration) => _configDelete = configuration;
        protected void ConfigureAll(Action<EndpointOptions> configuration) => _configAll = configuration;

        public void Map(IEndpointRouteBuilder app)
        {
            MapGetList(app);
            MapGetById(app);
            MapPost(app);
            MapPut(app);
            MapDelete(app);

            MapEndpoints(app);
        }

        private EndpointOptions BuildOptions(Action<EndpointOptions>? specific)
        {
            var opt = new EndpointOptions();
            _configAll?.Invoke(opt);     // común
            specific?.Invoke(opt);       // específico
            return opt;
        }

        private void MapGetList(IEndpointRouteBuilder app)
        {
            var opt = BuildOptions(_configGetList); //El usuario llena las opciones

            if (opt.Active)
            {
                var builder = app.MapGet($"/api/{typeName}s", async ([FromServices] DbContext context) =>
                {
                    return await context.Set<TModel>().ToListAsync();
                })
                .WithTags(typeName);

                opt.Builder?.Invoke(builder); // Aplicamos la configuración que haya puesto el usuario.
            }
        }

        private void MapGetById(IEndpointRouteBuilder app)
        {
            var opt = BuildOptions(_configGetById);

            if (opt.Active)
            {
                var builder = app.MapGet($"/api/{typeName}s/{{id}}", async ([FromRoute] TIdentity id, [FromServices] DbContext context) =>
                {
                    return await context.Set<TModel>().FindAsync(id);
                })
                .WithTags(typeName);

                opt.Builder?.Invoke(builder);
            }
        }

        private void MapPost(IEndpointRouteBuilder app)
        {
            var opt = BuildOptions(_configPost);

            if (opt.Active)
            {
                var builder = app.MapPost($"/api/{typeName}", async ([FromBody] TModel entity, [FromServices] DbContext context) =>
                {
                    context.Set<TModel>().Add(entity);
                    await context.SaveChangesAsync();
                    return Results.Created($"/api/{typeName}/{entity.Id?.ToString()}", entity);
                })
                .WithTags(typeName);

                opt.Builder?.Invoke(builder);
            }
        }

        private void MapPut(IEndpointRouteBuilder app)
        {
            var opt = BuildOptions(_configPut);

            if (opt.Active)
            {
                var builder = app.MapPut($"/api/{typeName}/{{id}}", async ([FromRoute] TIdentity id, [FromBody] TModel updatedEntity, [FromServices] DbContext context) =>
                {
                    var entity = await context.Set<TModel>().FindAsync(id);
                    if (entity is null)
                    {
                        return Results.NotFound();
                    }
                    context.Entry(entity).CurrentValues.SetValues(updatedEntity);
                    await context.SaveChangesAsync();
                    return Results.NoContent();
                })
                .WithTags(typeName);

                opt.Builder?.Invoke(builder);
            }
        }

        private void MapDelete(IEndpointRouteBuilder app)
        {
            var opt = BuildOptions(_configDelete);

            if (opt.Active)
            {
                var builder = app.MapDelete($"/api/{typeName}/{{id}}", async ([FromRoute] TIdentity id, [FromServices] DbContext context) =>
                {
                    var entity = await context.Set<TModel>().FindAsync(id);
                    if (entity is null)
                    {
                        return Results.NotFound();
                    }

                    context.Set<TModel>().Remove(entity);
                    await context.SaveChangesAsync();
                    return Results.NoContent();
                })
                .WithTags(typeName);

                opt.Builder?.Invoke(builder);
            }
        }

        public virtual void MapEndpoints(IEndpointRouteBuilder app) { }
    }
}
