using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repositories
{
  public class UserClaimConfiguration : IEntityTypeConfiguration<UserClaim>
  {
    public void Configure(EntityTypeBuilder<UserClaim> builder)
    {
      builder.HasKey(e => e.Id);

      builder
        .HasOne(e => e.User)
        .WithMany(e => e.UserClaims)
        .HasForeignKey(e => e.UserId);
    }
  }
}
