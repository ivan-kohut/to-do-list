using Duende.IdentityServer.EntityFramework.DbContexts;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace TodoList.Identity.API.Data.Seed
{
  public static class PersistedGrantDbContextSeed
  {
    public static async Task InitializeAsync(this PersistedGrantDbContext context)
    {
      await context
        .Database
        .MigrateAsync();
    }
  }
}
