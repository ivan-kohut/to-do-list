using Entities;
using Microsoft.EntityFrameworkCore;

namespace Repositories
{
  public class AppDbContext : DbContext
  {
    public DbSet<Item> Items { get; set; }

    public AppDbContext(DbContextOptions options) : base(options)
    {
    }
  }
}
