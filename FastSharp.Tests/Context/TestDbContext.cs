using Microsoft.EntityFrameworkCore;

namespace FastSharp.Tests
{
    public sealed class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
    {
        public DbSet<TestModel> Models => Set<TestModel>();
    }
}
