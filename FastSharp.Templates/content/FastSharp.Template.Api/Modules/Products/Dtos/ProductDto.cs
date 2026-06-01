namespace FastSharpApi.Modules.Products.Dtos;

public record ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public double Price { get; set; }
    public string Description { get; set; } = null!;
    public int Quantity { get; set; }
}
