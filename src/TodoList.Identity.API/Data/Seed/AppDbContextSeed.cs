using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using TodoList.Identity.API.Data.Entities;

namespace TodoList.Identity.API.Data.Seed
{
  public static class AppDbContextSeed
  {
    public static async Task InitializeAsync(this AppDbContext context)
    {
      await context
        .Database
        .MigrateAsync();

      if (!await context.Roles.AnyAsync())
      {
        IEnumerable<Role> roles = Config.Roles;

        foreach (Role role in roles)
        {
          context.Roles.Add(role);
        }

        await context.SaveChangesAsync();

        if (!await context.Users.AnyAsync())
        {
          User admin = Config.Admin;

          context.Users.Add(admin);

          await context.SaveChangesAsync();

          foreach (Role role in roles)
          {
            context.UserRoles.Add(new UserRole
            {
              UserId = admin.Id,
              RoleId = role.Id
            });
          }

          await context.SaveChangesAsync();
        }
      }
    }
  }
}
