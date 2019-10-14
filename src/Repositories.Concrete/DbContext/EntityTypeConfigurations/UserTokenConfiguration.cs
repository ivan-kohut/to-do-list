using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repositories
{
  public class UserTokenConfiguration : IEntityTypeConfiguration<UserToken>
  {
    public void Configure(EntityTypeBuilder<UserToken> builder)
    {
      builder.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });

      builder
        .HasOne(e => e.User)
        .WithMany(e => e.UserTokens)
        .HasForeignKey(e => e.UserId);
    }
  }
}
