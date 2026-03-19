namespace Api.Modules.Products.Dtos
{
    public class ProductRequest
    {
        public string Name { get; set; } = null!;

        public double Price { get; set; }

        public string Description { get; set; } = null!;
    }
}
