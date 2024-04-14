using Duende.IdentityServer;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TodoList.Identity.API.Data;
using TodoList.Identity.API.Data.Entities;
using TodoList.Identity.API.Events;
using TodoList.Identity.API.Services;

namespace TodoList.Identity.API.Pages.Account
{
    public class ExternalLoginPageModel(
        AppDbContext dbContext,
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IEventBusService eventBusService,
        IAuthenticationSchemeProvider schemeProvider,
        IIdentityServerInteractionService interaction) : PageModel
    {
        public async Task<IActionResult> OnGetAsync(string? scheme, string? returnUrl)
        {
            if (!interaction.IsValidReturnUrl(returnUrl)
                || string.IsNullOrWhiteSpace(scheme)
                || !(await schemeProvider.GetAllSchemesAsync()).Any(s => s.Name == scheme))
            {
                return Redirect("/");
            }

            return Challenge(new AuthenticationProperties
            {
                RedirectUri = Url.Page("ExternalLogin", "Callback"),
                Items =
                {
                    { "returnUrl", returnUrl }
                }
            }, scheme);
        }

        public async Task<IActionResult> OnGetCallbackAsync()
        {
            AuthenticateResult authResult = await HttpContext.AuthenticateAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);

            if (!authResult.Succeeded || authResult.Principal is null)
            {
                return Redirect("/");
            }

            string? externalId = authResult.Principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            string? externalLoginProvider = authResult.Principal.Identity?.AuthenticationType;
            string? userEmail = authResult.Principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            if (new[] { externalId, externalLoginProvider, userEmail }.Any(string.IsNullOrWhiteSpace))
            {
                return Redirect("/");
            }

            User? user = await userManager.FindByEmailAsync(userEmail!);

            if (user == null)
            {
                user = new User { UserName = userEmail, Email = userEmail };

                await userManager.CreateAsync(user);
                await userManager.AddToRoleAsync(user, "user");
                await userManager.AddLoginAsync(user, new UserLoginInfo(externalLoginProvider!, externalId!, externalLoginProvider));

                eventBusService.Publish(new UserCreatedIntegrationEvent(user.Id));
            }
            else if (!dbContext.UserLogins.Any(ul => ul.LoginProvider == externalLoginProvider && ul.ProviderKey == externalId))
            {
                await userManager.AddLoginAsync(user, new UserLoginInfo(externalLoginProvider!, externalId!, externalLoginProvider));
            }

            await signInManager.SignInWithClaimsAsync(user, isPersistent: false, [new Claim("amr", "pwd")]);
            await HttpContext.SignOutAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);

            string? returnUrl = authResult.Properties?.Items["returnUrl"];

            return Redirect(interaction.IsValidReturnUrl(returnUrl) ? returnUrl! : "/");
        }
    }
}
