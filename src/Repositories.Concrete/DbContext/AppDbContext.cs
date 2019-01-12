using Entities;
using Microsoft.EntityFrameworkCore;

namespace Repositories
{
  public class AppDbContext : DbContext
  {
    public DbSet<Item> Items { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      base.OnModelCreating(modelBuilder);

      modelBuilder.ApplyConfiguration(new ItemConfiguration());
    }
  }
}
