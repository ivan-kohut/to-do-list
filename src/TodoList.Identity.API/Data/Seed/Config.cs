using IdentityServer4;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace TodoList.Identity.API.Data.Seed
{
  public static class Config
  {
    public static IdentityUser<int> Admin =>
      new IdentityUser<int>
      {
        UserName = "admin",
        NormalizedUserName = "ADMIN",
        Email = "admin@admin.admin",
        NormalizedEmail = "ADMIN@ADMIN.ADMIN",
        EmailConfirmed = true,
        PasswordHash = "AQAAAAEAACcQAAAAEPps9h221EpyXTDM/MlkZEt1+BFD6ACavmo1PAopG7Akv0nkO3I93kkCyMFUIj6UXw==",
        SecurityStamp = Guid.NewGuid().ToString("D"),
        LockoutEnabled = true
      };

    public static IEnumerable<IdentityRole<int>> Roles =>
      new[]
      {
        new IdentityRole<int> { Name = "admin", NormalizedName = "ADMIN" },
        new IdentityRole<int> { Name = "user", NormalizedName = "USER" }
      };

    public static IEnumerable<ApiResource> ApiResources =>
      new[]
      {
          new ApiResource("items", "Items Service")
      };

    public static IEnumerable<IdentityResource> IdentityResources =>
      new IdentityResource[]
      {
        new IdentityResources.OpenId(),
        new IdentityResources.Profile()
      };

    public static IEnumerable<Client> Clients =>
      new[]
      {
        new Client
        {
          ClientId = "wasm",
          ClientName = "Blazor WebAssembly Client",
          AllowedGrantTypes = GrantTypes.Code,
          RequireClientSecret = false,

          RedirectUris =           { "https://localhost:44399/callback.html" },
          PostLogoutRedirectUris = { "https://localhost:44399/index.html" },
          AllowedCorsOrigins =     { "https://localhost:44399" },

          AllowedScopes =
          {
              IdentityServerConstants.StandardScopes.OpenId,
              IdentityServerConstants.StandardScopes.Profile,
              "items"
          }
        }
      };
  }
}
