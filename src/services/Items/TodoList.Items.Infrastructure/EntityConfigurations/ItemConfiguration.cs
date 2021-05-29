using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TodoList.Items.Domain.Aggregates.ItemAggregate;

namespace TodoList.Items.Infrastructure.EntityConfigurations
{
  public class ItemConfiguration : IEntityTypeConfiguration<Item>
  {
    public void Configure(EntityTypeBuilder<Item> builder)
    {
      builder.HasKey(e => e.Id);

      builder.HasIndex(e => e.Id);

      builder.Property(e => e.UserId);

      builder.Property(e => e.Status);

      builder
        .Property(e => e.Text)
        .HasMaxLength(255);

      builder.Property(e => e.Priority);

      builder.ToTable("Items");
    }
  }
}
