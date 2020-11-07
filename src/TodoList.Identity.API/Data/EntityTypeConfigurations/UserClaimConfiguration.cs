using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TodoList.Identity.API.Data.Entities;

namespace TodoList.Identity.API.Data.EntityTypeConfigurations
{
  public class UserClaimConfiguration : IEntityTypeConfiguration<UserClaim>
  {
    public void Configure(EntityTypeBuilder<UserClaim> builder)
    {
      builder.HasKey(e => e.Id);

      builder
        .HasOne(e => e.User)
        .WithMany(e => e!.UserClaims)
        .HasForeignKey(e => e.UserId);
    }
  }
}
