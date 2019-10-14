﻿using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repositories
{
  public class UserLoginConfiguration : IEntityTypeConfiguration<UserLogin>
  {
    public void Configure(EntityTypeBuilder<UserLogin> builder)
    {
      builder.HasKey(e => new { e.LoginProvider, e.ProviderKey });

      builder
        .HasOne(e => e.User)
        .WithMany(e => e.UserLogins)
        .HasForeignKey(e => e.UserId);
    }
  }
}
