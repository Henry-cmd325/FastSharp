using Api.Context;
using Api.Context.Models;
using Api.Modules.Products.Dtos;
using Api.Modules.Products.Endpoints;
using FastSharp.Modules.Configuration;
using FastSharp.Modules.Core;

namespace Api.Modules.Products;

public class ProductsModule : Module<ApiDbContext>
{
    public ProductsModule()
    {
        ConfigureModule("/api", opt => opt
            .WithTags("Productos")
            .WithDescription("Endpoints of products module")
        );

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

        // You can also include custom endpoints in the module.
        Include<CheckProductStock>();
        Include<UpdateProductsStock>();
    }
}
