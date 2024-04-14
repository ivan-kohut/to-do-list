using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace TodoList.Items.IntegrationTests.Infrastructure
{
    public class StubAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        public const string AuthenticationScheme = "IntegrationTestsScheme";

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            IEnumerable<Claim> claims =
            [
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "user")
            ];

            IIdentity identity = new ClaimsIdentity(claims, AuthenticationScheme);
            ClaimsPrincipal principal = new(identity);

            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, AuthenticationScheme)));
        }
    }
}
