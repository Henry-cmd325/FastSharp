using Api.Context;
using Api.Context.Models;
using Api.Modules.Products.Dtos;
using Api.Modules.Products.Endpoints;
using FastSharp.Modules;
using FastSharp.Modules.Configuration;

namespace Api.Modules.Products
{
    public class ProductsModule : Module<ApiDbContext>
    {
        public ProductsModule()
        {
            ConfigureModule("api/products", opt =>
            {
                opt.WithTags("Productos")
                .WithDescription("Endpoints for managing products in the inventory");
            });
        
            AddCRUD<Product, int>("products", crud =>
            {
                crud.DisableEndpoint(GenericEndpoint.GetList);

                crud.GetPaged<ProductDto>((endpoint) =>
                {
                    endpoint.WithDescription("Retrieves a product by its unique identifier").WithTags("Get");
                });

                crud.Create<ProductRequest, ProductDto>((endpoint) =>
                {
                    endpoint.WithDescription("Creates a new product").WithTags("Create");
                });
            });

            AddCRUD<Product, int>("products/alternative");

            IncludeNamespace<CheckProductStock>();
        }
    }
}