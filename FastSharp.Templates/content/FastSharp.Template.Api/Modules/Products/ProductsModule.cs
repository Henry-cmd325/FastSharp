using FastSharpApi.Context;
using FastSharpApi.Context.Models;
using FastSharpApi.Modules.Products.Dtos;
using FastSharpApi.Modules.Products.Endpoints;
using FastSharp.Modules.Configuration;
using FastSharp.Modules.Core;

namespace FastSharpApi.Modules.Products;

public class ProductsModule : Module<ApiDbContext>
{
    public ProductsModule()
    {
        ConfigureModule("/api", opt => opt
            .WithTags("Productos")
            .WithDescription("Endpoints of products module")
        );

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

        // Custom endpoints inside the module
        Include<CheckProductStock>();
        Include<UpdateProductsStock>();
    }
}
