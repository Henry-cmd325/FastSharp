using Api.Context;
using Api.Context.Models;
using Api.Slices.Products.Endpoints;
using FastSharp.Controllers;
using FastSharp.Controllers.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Api.Slices.Products
{
    public class ProductsController : FastController<ApiDbContext, Product, int> 
    {
        public ProductsController()
        {
            ConfigureGroup(opt =>
            {
                opt.WithTags("ProductosPrueba")
                .WithDescription("Endpoints for managing products in the inventory");
            });

            ConfigureCRUD(opt =>
            {
                opt.DisableEndpoint(GenericEndpoint.GetList);

                opt.ConfigureEndpoint(GenericEndpoint.Delete, (endpoint) =>
                    endpoint.WithDescription("Deletes a product by its unique identifier")
                    .WithTags("Delete"));
            });

            IncludeNamespace<CheckProductStock>();
        }
    }
}