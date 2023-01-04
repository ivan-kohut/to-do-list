using Duende.IdentityServer;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TodoList.Identity.API.Data;
using TodoList.Identity.API.Data.Entities;
using TodoList.Identity.API.Events;
using TodoList.Identity.API.Services;

namespace TodoList.Identity.API.Controllers
{
  public class ExternalController : Controller
  {
    private readonly AppDbContext dbContext;
    private readonly UserManager<User> userManager;
    private readonly SignInManager<User> signInManager;
    private readonly IEventBusService eventBusService;
    private readonly IAuthenticationSchemeProvider schemeProvider;
    private readonly IIdentityServerInteractionService interaction;

    public ExternalController(
      AppDbContext dbContext,
      UserManager<User> userManager,
      SignInManager<User> signInManager,
      IEventBusService eventBusService,
      IAuthenticationSchemeProvider schemeProvider,
      IIdentityServerInteractionService interaction)
    {
      this.dbContext = dbContext;
      this.userManager = userManager;
      this.signInManager = signInManager;
      this.eventBusService = eventBusService;
      this.schemeProvider = schemeProvider;
      this.interaction = interaction;
    }

    [HttpGet]
    public async Task<IActionResult> Challenge(string scheme, string returnUrl)
    {
      if (!interaction.IsValidReturnUrl(returnUrl) || string.IsNullOrWhiteSpace(scheme) || !(await schemeProvider.GetAllSchemesAsync()).Any(s => s.Name == scheme))
      {
        return Redirect("/");
      }

      return Challenge(new AuthenticationProperties
      {
        RedirectUri = Url.Action(nameof(Callback)),
        Items =
        {
          { "returnUrl", returnUrl }
        }
      }, scheme);
    }

    [HttpGet]
    [Authorize(AuthenticationSchemes = IdentityServerConstants.ExternalCookieAuthenticationScheme)]
    public async Task<IActionResult> Callback()
    {
      string? externalId = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
      string? externalLoginProvider = User.Identity?.AuthenticationType;
      string? userEmail = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;

      if (new[] { externalId, externalLoginProvider, userEmail }.Any(v => string.IsNullOrWhiteSpace(v)))
      {
        return Redirect("/");
      }

      User user = await userManager.FindByEmailAsync(userEmail);

      if (user == null)
      {
        user = new User { UserName = userEmail, Email = userEmail };

        await userManager.CreateAsync(user);
        await userManager.AddToRoleAsync(user, "user");
        await userManager.AddLoginAsync(user, new UserLoginInfo(externalLoginProvider, externalId, externalLoginProvider));

        eventBusService.Publish(new UserCreatedIntegrationEvent(user.Id));
      }
      else if (!dbContext.UserLogins.Any(ul => ul.LoginProvider == externalLoginProvider && ul.ProviderKey == externalId))
      {
        await userManager.AddLoginAsync(user, new UserLoginInfo(externalLoginProvider, externalId, externalLoginProvider));
      }

      await signInManager.SignInWithClaimsAsync(user, isPersistent: false, new Claim[] { new Claim("amr", "pwd") });
      await HttpContext.SignOutAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);

      string? returnUrl = (await HttpContext.AuthenticateAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme)).Properties?.Items["returnUrl"];

      return Redirect(interaction.IsValidReturnUrl(returnUrl) ? returnUrl : "/");
    }
  }
}
