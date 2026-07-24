using FastSharpApi.Context;
using FastSharpApi.Context.Models;
using FastSharpApi.Modules.Products.Dtos;
using FastSharpApi.Modules.Products.Endpoints;
using FastSharp.Modules.Configuration;
using FastSharp.Modules.Core;

namespace FastSharpApi.Modules.Products;

public class ProductsModule : Module<ApiDbContext>
{
    protected override void Configure(ModuleConfiguration configuration)
    {
        configuration.Prefix = "/api";
        configuration.Conventions = opt => opt
            .WithTags("Productos")
            .WithDescription("Endpoints of products module");
    }

    protected override void AddRoutes(RouteGroupBuilder routes)
    {

        // Simplest way: Automatically maps all standard CRUD operations to ProductDto.
        AddCRUD<Product, int>("/products", crud => crud.ConfigureAll<ProductDto>());

        // Advanced: Full control over each endpoint.
        AddCRUD<Product, int>("/products/alternative", p => p.Id, crud =>
        {
            crud.DisableEndpoint(GenericEndpoint.GetList);

            crud.GetList<ProductDto>((endpoint) => endpoint
                .WithDescription("Retrieves a list of products (with optional ?page and ?pageSize params for pagination)")
                .WithTags("GetList")
            );

            crud.Create<ProductRequest, ProductDto>((endpoint) => endpoint
                .WithDescription("Creates a new product")
                .WithTags("Create")
            );
        });

        // Keep complex behavior in endpoint classes.
        Include<UpdateProductsStock>();

        // Small, module-local routes can use native Minimal API mapping directly.
        routes.MapGet("/{id}/stock", ([Microsoft.AspNetCore.Mvc.FromRoute] int id) =>
            Results.Ok($"Checking stock for product {id}"))
            .WithTags("Custom");
    }
}
