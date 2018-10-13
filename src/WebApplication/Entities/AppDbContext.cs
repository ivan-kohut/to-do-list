using Microsoft.EntityFrameworkCore;

namespace WebApplication.Entities
{
  public class AppDbContext : DbContext
  {
    public AppDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Item> Items { get; set; }
  }
}
