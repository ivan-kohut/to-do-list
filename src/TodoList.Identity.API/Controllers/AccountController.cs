using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TodoList.Identity.API.ViewModels;

namespace TodoList.Identity.API.Controllers
{
  public class AccountController : Controller
  {
    private readonly SignInManager<IdentityUser<int>> signInManager;
    private readonly IIdentityServerInteractionService interaction;

    public AccountController(SignInManager<IdentityUser<int>> signInManager, IIdentityServerInteractionService interaction)
    {
      this.signInManager = signInManager;
      this.interaction = interaction;
    }

    [HttpGet]
    public IActionResult Login(string returnUrl) => View(new LoginViewModel { ReturnUrl = returnUrl });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
      if (ModelState.IsValid && (await signInManager.PasswordSignInAsync(model.Username, model.Password, false, lockoutOnFailure: true)).Succeeded)
      {
        return Url.IsLocalUrl(model.ReturnUrl)
          ? Redirect(model.ReturnUrl)
          : Redirect("/");
      }

      return View(new LoginViewModel { Username = model.Username, ReturnUrl = model.ReturnUrl });
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
