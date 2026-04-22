using FastSharp.Modules;
using Microsoft.AspNetCore.Mvc;

namespace Api.Modules.Products.Endpoints
{
    public class CheckProductStock : IEndpoint
    {
        public void Map(RouteGroupBuilder app)
        {
            app.MapGet("/{id}/stock", async ([FromRoute] int id) =>
            {
                return Results.Ok($"Checking stock for product {id}");
            })
            .WithTags("prueba");
        }
    }
}