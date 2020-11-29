using System;
using System.Security.Claims;

namespace Extensions
{
  public static class ClaimsPrincipalExtensions
  {
    public static int GetIdentityId(this ClaimsPrincipal claimsPrincipal) =>
      claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier) is Claim claim
        ? int.Parse(claim.Value)
        : throw new Exception($"Invalid {nameof(claimsPrincipal)}");
  }
}
