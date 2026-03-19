using Api.Context.Models;

namespace Api.Modules.Products.Dtos
{
    public class ProductDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public double Price { get; set; }

        public string Description { get; set; } = null!;
    }
}
