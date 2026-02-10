using Api.Context;
using Api.Context.Models;
using Api.Modules.Products.Endpoints;
using FastSharp.Modules;
using FastSharp.Modules.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Api.Modules.Products
{
    public class ProductsModule : Module<ApiDbContext>
    {
        public ProductsModule()
        {
            ConfigureGroup(opt =>
             {
                 opt.WithTags("Productos")
                 .WithDescription("Endpoints for managing products in the inventory");
             });
        
            AddCRUD<Product, int>("/products", opt =>
            {
                opt.DisableEndpoint(GenericEndpoint.GetList);

                opt.ConfigureEndpoint(GenericEndpoint.Delete, (endpoint) =>
                    endpoint.WithDescription("Deletes a product by its unique identifier")
                    .WithTags("Delete"));
            });

            AddCRUD<Product, int>("/products/alternative");

            IncludeNamespace<CheckProductStock>();
        }
    }
}