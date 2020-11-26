using Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Repositories
{
  public class AppDbContext : DbContext
  {
    public DbSet<Item> Items { get; set; } = null!;

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
  }
}
