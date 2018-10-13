using Entities;
using Microsoft.EntityFrameworkCore;

namespace Services
{
  public class AppDbContext : DbContext
  {
    public AppDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Item> Items { get; set; }
  }
}
