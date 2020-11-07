using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TodoList.Identity.API.ViewModels;

namespace TodoList.Identity.API.Controllers
{
  public class AccountController : Controller
  {
    private readonly UserManager<IdentityUser<int>> userManager;
    private readonly SignInManager<IdentityUser<int>> signInManager;
    private readonly IIdentityServerInteractionService interaction;

    public AccountController(
      UserManager<IdentityUser<int>> userManager,
      SignInManager<IdentityUser<int>> signInManager,
      IIdentityServerInteractionService interaction)
    {
      this.userManager = userManager;
      this.signInManager = signInManager;
      this.interaction = interaction;
    }

    [HttpGet]
    public IActionResult Login(string returnUrl) => View(new LoginViewModel { ReturnUrl = returnUrl });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
      if (ModelState.IsValid)
      {
        IdentityUser<int> user = await userManager.FindByEmailAsync(model.Email);

        if (user != null && (await signInManager.PasswordSignInAsync(user, model.Password, isPersistent: false, lockoutOnFailure: true)).Succeeded)
        {
          return Url.IsLocalUrl(model.ReturnUrl)
            ? Redirect(model.ReturnUrl)
            : Redirect("/");
        }
      }

      return View(new LoginViewModel { Email = model.Email, ReturnUrl = model.ReturnUrl });
    }

    [HttpGet]
    public IActionResult Logout(string logoutId) => View(new LogoutViewModel { LogoutId = logoutId });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(LogoutViewModel model)
    {
      if (User.Identity != null && User.Identity.IsAuthenticated)
      {
        await signInManager.SignOutAsync();
      }

      return Redirect((await interaction.GetLogoutContextAsync(model.LogoutId))?.PostLogoutRedirectUri ?? "/");
    }
  }
}
