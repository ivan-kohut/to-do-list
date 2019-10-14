using Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Repositories
{
  public class AppDbContext : IdentityDbContext<User, Role, int, UserClaim, UserRole, UserLogin, RoleClaim, UserToken>
  {
    public DbSet<Item> Items { get; set; } = null!;

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      base.OnModelCreating(modelBuilder);

      modelBuilder.ApplyConfiguration(new ItemConfiguration());
      modelBuilder.ApplyConfiguration(new UserRoleConfiguration());
      modelBuilder.ApplyConfiguration(new UserLoginConfiguration());
      modelBuilder.ApplyConfiguration(new UserTokenConfiguration());
      modelBuilder.ApplyConfiguration(new UserClaimConfiguration());
      modelBuilder.ApplyConfiguration(new RoleClaimConfiguration());
    }
  }
}
