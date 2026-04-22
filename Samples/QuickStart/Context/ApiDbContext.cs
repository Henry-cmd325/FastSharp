using Api.Context.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Context;

public partial class ApiDbContext(DbContextOptions<ApiDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
}