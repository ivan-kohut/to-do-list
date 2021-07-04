using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication.Internal;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace TodoList.Client
{
  public class ArrayClaimsPrincipalFactory<TAccount> : AccountClaimsPrincipalFactory<TAccount> where TAccount : RemoteUserAccount
  {
    public ArrayClaimsPrincipalFactory(IAccessTokenProviderAccessor accessor) : base(accessor)
    {
    }

    public async override ValueTask<ClaimsPrincipal> CreateUserAsync(TAccount account, RemoteAuthenticationUserOptions options)
    {
      ClaimsPrincipal user = await base.CreateUserAsync(account, options);

      if (account != null)
      {
        ClaimsIdentity? claimsIdentity = (ClaimsIdentity?)user.Identity;

        if (claimsIdentity != null)
        {
          foreach (KeyValuePair<string, object> kvp in account.AdditionalProperties)
          {
            if (kvp.Value is JsonElement element && element.ValueKind == JsonValueKind.Array)
            {
              claimsIdentity.RemoveClaim(claimsIdentity.FindFirst(kvp.Key));

              IEnumerable<Claim> claims = element
                .EnumerateArray()
                .Select(e => e.ToString())
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Select(v => new Claim(kvp.Key, v!))
                .ToList();

              claimsIdentity.AddClaims(claims);
            }
          }
        }
      }

      return user;
    }
  }
}
