using Microsoft.EntityFrameworkCore;

namespace FastSharp.Tests.Context
{
    public sealed class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
    {
        public DbSet<TestModel> Models => Set<TestModel>();
        public DbSet<TestModelWitoutInterface> ModelsWithoutInterface => Set<TestModelWitoutInterface>();
    }
}
