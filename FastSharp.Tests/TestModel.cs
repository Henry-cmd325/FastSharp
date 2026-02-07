using FastSharp.Models;

namespace FastSharp.Tests
{
    public sealed class TestModel : IModel<int>
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }
}
