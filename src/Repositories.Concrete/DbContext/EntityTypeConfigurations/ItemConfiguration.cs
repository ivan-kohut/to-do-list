using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repositories
{
  public class ItemConfiguration : IEntityTypeConfiguration<Item>
  {
    public void Configure(EntityTypeBuilder<Item> builder)
    {
      builder.HasKey(e => e.Id);

      builder.HasIndex(e => e.Id);

      builder.Property(e => e.UserId);

      builder
        .HasOne(e => e.User)
        .WithMany(e => e.Items);

      builder.Property(e => e.Status);

      builder
        .Property(e => e.Text)
        .HasMaxLength(255);

      builder.Property(e => e.Priority);

      builder.ToTable("Items");
    }
  }
}
