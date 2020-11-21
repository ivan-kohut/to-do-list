using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using TodoList.Identity.API.Data.Entities;
using TodoList.Identity.API.ViewModels;

namespace TodoList.Identity.API.Controllers
{
  [Authorize(Roles = "user")]
  public class SettingsController : Controller
  {
    private readonly UserManager<User> userManager;

    public SettingsController(UserManager<User> userManager)
    {
      this.userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Manage() => View(new ManageViewModel { IsTwoFactorAuthenticationEnabled = (await userManager.GetUserAsync(User)).TwoFactorEnabled });

    [HttpGet]
    public async Task<IActionResult> Enable2fa()
    {
      User user = await userManager.GetUserAsync(User);

      string authenticatorKey = await userManager.GetAuthenticatorKeyAsync(user);

      if (string.IsNullOrWhiteSpace(authenticatorKey))
      {
        await userManager.ResetAuthenticatorKeyAsync(user);

        authenticatorKey = await userManager.GetAuthenticatorKeyAsync(user);
      }

      return View(new Enable2faViewModel { AuthenticatorUri = $"otpauth://totp/ToDoList:{user.UserName}?secret={authenticatorKey}&issuer=ToDoList&digits=6" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enable2fa(Enable2faViewModel model)
    {
      if (ModelState.IsValid)
      {
        User user = await userManager.GetUserAsync(User);

        if (user.TwoFactorEnabled)
        {
          return RedirectToAction(nameof(Manage));
        }
        else if (await userManager.VerifyTwoFactorTokenAsync(user, userManager.Options.Tokens.AuthenticatorTokenProvider, model.Code))
        {
          await userManager.SetTwoFactorEnabledAsync(user, true);

          return RedirectToAction(nameof(Manage));
        }
        else
        {
          ModelState.AddModelError(nameof(Enable2faViewModel.Code), "Invalid authenticator code.");
        }
      }

      return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Disable2fa()
    {
      User user = await userManager.GetUserAsync(User);

      if (user.TwoFactorEnabled)
      {
        await userManager.SetTwoFactorEnabledAsync(user, false);
        await userManager.ResetAuthenticatorKeyAsync(user);
      }

      return RedirectToAction(nameof(Manage));
    }

    [HttpGet]
    public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
      if (ModelState.IsValid)
      {
        User user = await userManager.GetUserAsync(User);

        IdentityResult identityChangePasswordResult = await userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

        if (identityChangePasswordResult.Succeeded)
        {
          return RedirectToAction(nameof(Manage));
        }
        else
        {
          identityChangePasswordResult.Errors
            .Select(e => e.Description)
            .ToList()
            .ForEach(d => ModelState.AddModelError(string.Empty, d));
        }
      }

      return View(model);
    }
  }
}
