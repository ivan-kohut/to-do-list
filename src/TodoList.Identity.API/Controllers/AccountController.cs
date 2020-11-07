using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TodoList.Identity.API.Data.Entities;
using TodoList.Identity.API.ViewModels;

namespace TodoList.Identity.API.Controllers
{
  public class AccountController : Controller
  {
    private readonly UserManager<User> userManager;
    private readonly SignInManager<User> signInManager;
    private readonly IIdentityServerInteractionService interaction;

    public AccountController(
      UserManager<User> userManager,
      SignInManager<User> signInManager,
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
        User user = await userManager.FindByEmailAsync(model.Email);

        if (user == null)
        {
          ModelState.AddModelError(nameof(LoginViewModel.Email), "The User is not found.");
        }
        else if (!(await signInManager.PasswordSignInAsync(user, model.Password, isPersistent: false, lockoutOnFailure: true)).Succeeded)
        {
          ModelState.AddModelError(nameof(LoginViewModel.Password), "The Password is invalid.");
        }
        else
        {
          return interaction.IsValidReturnUrl(model.ReturnUrl)
            ? Redirect(model.ReturnUrl)
            : Redirect("/");
        }
      }

      return View(new LoginViewModel { ReturnUrl = model.ReturnUrl });
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
