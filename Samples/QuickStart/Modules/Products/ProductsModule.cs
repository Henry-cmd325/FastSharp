using Api.Context;
using Api.Context.Models;
using Api.Modules.Products.Dtos;
using Api.Modules.Products.Endpoints;
using FastSharp.Modules.Configuration;
using FastSharp.Modules.Core;

namespace Api.Modules.Products;

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
        // If your model implements IModel, FastSharp handles the Id selection automatically.
        AddCRUD<Product, int>("/products", crud => crud.ConfigureAll<ProductDto>());

        // Advanced: Full control over each endpoint.
        // You can pass a manual Id selector (p => p.Id) for existing models that don't implement IModel.
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
