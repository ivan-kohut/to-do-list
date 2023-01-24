using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TodoList.Identity.API.Data.Entities;

namespace TodoList.Identity.API.Data.EntityTypeConfigurations
{
    public class RoleClaimConfiguration : IEntityTypeConfiguration<RoleClaim>
    {
        public void Configure(EntityTypeBuilder<RoleClaim> builder)
        {
            builder.HasKey(e => e.Id);

            builder
                .HasOne(e => e.Role)
                .WithMany(e => e!.RoleClaims)
                .HasForeignKey(e => e.RoleId);
        }
    }
}
