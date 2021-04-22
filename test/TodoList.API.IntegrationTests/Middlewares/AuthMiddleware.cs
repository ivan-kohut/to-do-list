using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Controllers.Tests.Middlewares
{
  public class AuthMiddleware
  {
    private readonly RequestDelegate next;

    public AuthMiddleware(RequestDelegate next)
    {
      this.next = next;
    }

    public async Task Invoke(HttpContext httpContext)
    {
      ClaimsIdentity identity = new ClaimsIdentity("test-auth-type");

      identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, 1.ToString()));

      httpContext.User.AddIdentity(identity);

      await next.Invoke(httpContext);
    }
  }
}
