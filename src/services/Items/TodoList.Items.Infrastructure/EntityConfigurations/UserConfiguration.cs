using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TodoList.Items.Domain.Aggregates.UserAggregate;

namespace TodoList.Items.Infrastructure.EntityConfigurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(e => e.Id);

            builder.HasIndex(e => e.Id);

            builder.Property(e => e.IdentityId);

            builder.HasIndex(e => e.IdentityId).IsUnique();

            builder.ToTable("Users");
        }
    }
}
