using FastSharp.Models;

namespace Api.Context.Models;

public partial class Product : IModel<int>
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public double Price { get; set; }

    public string Description { get; set; } = null!;

    public virtual Stock? Stock { get; set; }
}
