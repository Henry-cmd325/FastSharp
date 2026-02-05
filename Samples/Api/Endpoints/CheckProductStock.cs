using FastSharp.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints
{
    public class CheckProductStock : IFastEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/products/{id}/stock", async ([FromRoute]int id) =>
            {
                Results.Ok($"Checking stock for product {id}");
            })
            .WithTags("prueba");
        }
    }
}
