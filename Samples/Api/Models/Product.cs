using FastSharp.Models;

namespace Api.Models
{
    public class Product : IModel<int>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
