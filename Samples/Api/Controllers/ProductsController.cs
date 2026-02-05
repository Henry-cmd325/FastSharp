using Api.Models;
using FastSharp.Controllers;

namespace Api.Controllers
{
    public class ProductsController : FastController<Product, int> 
    {
        public ProductsController()
        {
            ConfigureGetList(opt => opt.Active = false);

            ConfigureDelete(opt => opt.Builder = b =>
                b.WithDescription("Deletes a product by its unique identifier.")
                    .WithTags("Delete")
            );
        }
    }
}
