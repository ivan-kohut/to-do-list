using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        IEnumerable<IdentityRole<int>> roles = Config.Roles;

        foreach (IdentityRole<int> role in roles)
        {
          context.Roles.Add(role);
        }

        await context.SaveChangesAsync();

        if (!await context.Users.AnyAsync())
        {
          IdentityUser<int> admin = Config.Admin;

          context.Users.Add(admin);

          await context.SaveChangesAsync();

          foreach (IdentityRole<int> role in roles)
          {
            context.UserRoles.Add(new IdentityUserRole<int>
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
