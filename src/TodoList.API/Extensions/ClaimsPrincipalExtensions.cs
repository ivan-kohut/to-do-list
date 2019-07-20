using System.Security.Claims;

namespace Extensions
{
  public static class ClaimsPrincipalExtensions
  {
    public static int GetAuthorizedUserId(this ClaimsPrincipal claimsPrincipal)
    {
      return int.Parse(claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier).Value);
    }
  }
}
