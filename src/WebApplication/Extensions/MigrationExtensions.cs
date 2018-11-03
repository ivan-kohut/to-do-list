using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Repositories;

namespace WebApplication
{
  public static class MigrationExtensions
  {
    public static IApplicationBuilder ApplyMigrations(this IApplicationBuilder app)
    {
      using (IServiceScope serviceScope = app.ApplicationServices.CreateScope())
      {
        using (DbContext dbContext = serviceScope.ServiceProvider.GetService<AppDbContext>())
        {
          dbContext.Database.Migrate();
        }
      }

      return app;
    }
  }
}
