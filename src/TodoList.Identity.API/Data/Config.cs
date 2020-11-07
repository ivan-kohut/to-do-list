using IdentityServer4;
using IdentityServer4.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using TodoList.Identity.API.Data.Entities;

namespace TodoList.Identity.API.Data
{
  public static class Config
  {
    public static User Admin =>
      new User
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

    public static IEnumerable<Role> Roles =>
      new[]
      {
        new Role { Name = "admin", NormalizedName = "ADMIN" },
        new Role { Name = "user", NormalizedName = "USER" }
      };

    public static IEnumerable<ApiScope> ApiScopes =>
      new[]
      {
        new ApiScope(name: "items", displayName: "Items Service")
      };

    public static IEnumerable<ApiResource> ApiResources =>
      new[]
      {
          new ApiResource(name: "items", displayName: "Items Service", userClaims: new[] { "role" })
          {
              Scopes = { "items" }
          }
      };

    public static IEnumerable<IdentityResource> IdentityResources =>
      new IdentityResource[]
      {
        new IdentityResources.OpenId(),
        new IdentityResource(name: "profile", userClaims: new[] { "name", "role" }, displayName: "User profile")
      };

    public static IEnumerable<Client> Clients(IConfiguration configuration)
    {
      string blazorWasmClientUrl = configuration["BlazorWasmClientUrl"];

      return new[]
      {
        new Client
        {
          ClientId = "wasm",
          ClientName = "Blazor WebAssembly Client",
          AllowedGrantTypes = GrantTypes.Code,
          RequireClientSecret = false,

          RedirectUris =           { $"{blazorWasmClientUrl}/authentication/login-callback" },
          PostLogoutRedirectUris = { $"{blazorWasmClientUrl}/" },
          AllowedCorsOrigins =     { blazorWasmClientUrl },

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
}
