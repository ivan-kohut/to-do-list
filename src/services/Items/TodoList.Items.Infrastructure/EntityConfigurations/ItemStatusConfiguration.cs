using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TodoList.Items.Domain.Aggregates.ItemAggregate;

namespace TodoList.Items.Infrastructure.EntityConfigurations
{
  public class ItemStatusConfiguration : IEntityTypeConfiguration<ItemStatus>
  {
    public void Configure(EntityTypeBuilder<ItemStatus> builder)
    {
      builder.HasKey(e => e.Id);

      builder.HasIndex(e => e.Id);

      builder
        .Property(e => e.Name)
        .HasMaxLength(50);
    }
  }
}
