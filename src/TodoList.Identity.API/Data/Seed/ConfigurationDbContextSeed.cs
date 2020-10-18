using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace TodoList.Identity.API.Data.Seed
{
  public static class ConfigurationDbContextSeed
  {
    public static async Task InitializeAsync(this ConfigurationDbContext context)
    {
      await context
        .Database
        .MigrateAsync();

      if (!await context.ApiScopes.AnyAsync())
      {
        foreach (ApiScope apiScope in Config.ApiScopes)
        {
          context.ApiScopes.Add(apiScope.ToEntity());
        }
      }

      if (!await context.ApiResources.AnyAsync())
      {
        foreach (ApiResource apiResource in Config.ApiResources)
        {
          context.ApiResources.Add(apiResource.ToEntity());
        }
      }

      if (!await context.IdentityResources.AnyAsync())
      {
        foreach (IdentityResource identityResource in Config.IdentityResources)
        {
          context.IdentityResources.Add(identityResource.ToEntity());
        }
      }

      if (!await context.Clients.AnyAsync())
      {
        foreach (Client client in Config.Clients)
        {
          context.Clients.Add(client.ToEntity());
        }
      }

      await context.SaveChangesAsync();
    }
  }
}
