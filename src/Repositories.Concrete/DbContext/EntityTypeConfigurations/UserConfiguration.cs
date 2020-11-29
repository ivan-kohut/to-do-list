using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repositories
{
  public class UserConfiguration : IEntityTypeConfiguration<User>
  {
    public void Configure(EntityTypeBuilder<User> builder)
    {
      builder.HasKey(e => e.Id);

      builder.HasIndex(e => e.Id);
      builder.HasIndex(e => e.IdentityId).IsUnique();

      builder.ToTable("Users");
    }
  }
}
