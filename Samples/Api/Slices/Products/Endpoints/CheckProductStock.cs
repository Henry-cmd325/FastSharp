using FastSharp.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Api.Slices.Products.Endpoints
{
    public class CheckProductStock : IFastEndpoint
    {
        public void Map(RouteGroupBuilder app)
        {
            app.MapGet("/api/products/{id}/stock", async ([FromRoute] int id) =>
            {
                Results.Ok($"Checking stock for product {id}");
            })
            .WithTags("prueba");
        }
    }
}
