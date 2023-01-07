using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TodoList.Items.Domain.Aggregates.ItemAggregate;
using TodoList.Items.Domain.Aggregates.UserAggregate;

namespace TodoList.Items.Infrastructure.EntityConfigurations
{
    public class ItemConfiguration : IEntityTypeConfiguration<Item>
    {
        public void Configure(EntityTypeBuilder<Item> builder)
        {
            builder.HasKey(e => e.Id);

            builder.HasIndex(e => e.Id);

            builder.Property(e => e.UserId);

            builder
                .HasOne<User>()
                .WithMany();

            builder
                .Property<int>("_statusId")
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasColumnName("Status");

            builder
                .HasOne(e => e.Status)
                .WithMany()
                .HasForeignKey("_statusId");

            builder
                .Property(e => e.Text)
                .HasMaxLength(255);

            builder.Property(e => e.Priority);

            builder.ToTable("Items");
        }
    }
}
