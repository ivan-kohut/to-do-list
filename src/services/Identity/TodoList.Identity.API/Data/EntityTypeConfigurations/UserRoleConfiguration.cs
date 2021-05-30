using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TodoList.Identity.API.Data.Entities;

namespace TodoList.Identity.API.Data.EntityTypeConfigurations
{
  public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
  {
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
      builder.HasKey(e => new { e.UserId, e.RoleId });

      builder
        .HasOne(e => e.Role)
        .WithMany(e => e!.UserRoles)
        .HasForeignKey(e => e.RoleId);

      builder
        .HasOne(e => e.User)
        .WithMany(e => e!.UserRoles)
        .HasForeignKey(e => e.UserId);
    }
  }
}
