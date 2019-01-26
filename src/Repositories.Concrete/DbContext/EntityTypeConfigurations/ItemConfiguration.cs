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

      builder.Property(e => e.UserId)
        .IsRequired();

      builder
        .HasOne(e => e.User)
        .WithMany(e => e.Items);
      
      builder.Property(e => e.Status)
        .IsRequired();

      builder.Property(e => e.Text)
        .IsRequired()
        .HasMaxLength(255);

      builder.Property(e => e.Priority)
        .IsRequired();
    }
  }
}
