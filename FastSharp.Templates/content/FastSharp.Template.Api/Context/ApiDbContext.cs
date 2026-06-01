using FastSharpApi.Context.Models;
using Microsoft.EntityFrameworkCore;

namespace FastSharpApi.Context;

public partial class ApiDbContext(DbContextOptions<ApiDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
}
