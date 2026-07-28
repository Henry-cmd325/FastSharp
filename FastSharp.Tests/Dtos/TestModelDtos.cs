namespace FastSharp.Tests.Dtos
{
    public sealed class TestModelIdDto
    {
        public int Id { get; set; }
    }

    public sealed class TestModelRequestDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    public sealed class TestModelResponseDto
    {
        public int Id { get; set; }
    }

    public sealed class TestModelValidatedDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }
}
