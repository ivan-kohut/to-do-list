using Entities;
using Microsoft.EntityFrameworkCore;

namespace Repositories
{
  public class AppDbContext : DbContext
  {
    public AppDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Item> Items { get; set; }
  }
}
