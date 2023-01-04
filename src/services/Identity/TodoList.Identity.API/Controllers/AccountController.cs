using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using TodoList.Identity.API.Data.Entities;
using TodoList.Identity.API.Events;
using TodoList.Identity.API.Services;
using TodoList.Identity.API.ViewModels;

using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace TodoList.Identity.API.Controllers
{
  public class AccountController : Controller
  {
    private readonly UserManager<User> userManager;
    private readonly SignInManager<User> signInManager;
    private readonly IEmailService emailService;
    private readonly IEventBusService eventBusService;
    private readonly IIdentityServerInteractionService interaction;

    public AccountController(
      UserManager<User> userManager,
      SignInManager<User> signInManager,
      IEmailService emailService,
      IEventBusService eventBusService,
      IIdentityServerInteractionService interaction)
    {
      this.userManager = userManager;
      this.signInManager = signInManager;
      this.emailService = emailService;
      this.eventBusService = eventBusService;
      this.interaction = interaction;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl) => View(new LoginViewModel { ReturnUrl = returnUrl });

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
        else if (!await userManager.CheckPasswordAsync(user, model.Password))
        {
          ModelState.AddModelError(nameof(LoginViewModel.Password), "The Password is invalid.");
        }
        else if (!user.EmailConfirmed)
        {
          ModelState.AddModelError(nameof(LoginViewModel.Email), "The Email is not confirmed.");
        }
        else
        {
          SignInResult signInResult = await signInManager.PasswordSignInAsync(user, model.Password, isPersistent: false, lockoutOnFailure: true);

          if (signInResult.Succeeded)
          {
            return RedirectTo(model.ReturnUrl);
          }
          else if (signInResult.RequiresTwoFactor)
          {
            return RedirectToAction(nameof(LoginWith2fa), new { model.ReturnUrl });
          }
          else
          {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
          }
        }
      }

      return View(model);
    }

    [HttpGet]
    public IActionResult Register(string? returnUrl) => View(new RegisterViewModel { ReturnUrl = returnUrl });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
      if (ModelState.IsValid)
      {
        User user = await userManager.FindByEmailAsync(model.Email);

        if (user != null && user.PasswordHash != null)
        {
          ModelState.AddModelError(nameof(LoginViewModel.Email), $"The User with the email already exists");

          return View(model);
        }
        else if (user == null)
        {
          user = new User { UserName = model.Name, Email = model.Email };

          IdentityResult identityCreateResult = await userManager.CreateAsync(user, model.Password);

          if (!identityCreateResult.Succeeded)
          {
            return ViewWithErrors(identityCreateResult, model);
          }

          await userManager.AddToRoleAsync(user, "user");

          eventBusService.Publish(new UserCreatedIntegrationEvent(user.Id));
        }
        else
        {
          user.UserName = model.Name;

          foreach (IUserValidator<User> userValidator in userManager.UserValidators)
          {
            IdentityResult identityValidateResult = await userValidator.ValidateAsync(userManager, user);

            if (!identityValidateResult.Succeeded)
            {
              return ViewWithErrors(identityValidateResult, model);
            }
          }

          IdentityResult identityUpdateResult = await userManager.AddPasswordAsync(user, model.Password);

          if (!identityUpdateResult.Succeeded)
          {
            return ViewWithErrors(identityUpdateResult, model);
          }
        }

        await emailService.SendEmailAsync(user.Email!, "Confirm your email", await GenerateEmailConfirmationMessageAsync(user, model.ReturnUrl));

        return RedirectToAction(nameof(RegisterSuccess));
      }

      return View(model);
    }

    [HttpGet]
    public IActionResult RegisterSuccess() => View();

    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(int? id, string? code, string? returnUrl)
    {
      if (!id.HasValue || string.IsNullOrWhiteSpace(code))
      {
        return Redirect("/");
      }

      User user = await userManager.FindByIdAsync(id.ToString());

      return user == null || user.EmailConfirmed || !(await userManager.ConfirmEmailAsync(user, code)).Succeeded
        ? Redirect("/")
        : RedirectToAction(nameof(ConfirmEmailSuccess), new { returnUrl });
    }

    [HttpGet]
    public IActionResult ConfirmEmailSuccess(string? returnUrl) => View(model: returnUrl);

    [HttpGet]
    [Authorize(AuthenticationSchemes = "Identity.TwoFactorUserId")]
    public IActionResult LoginWith2fa(string? returnUrl) => View(new LoginWith2faViewModel { ReturnUrl = returnUrl });

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(AuthenticationSchemes = "Identity.TwoFactorUserId")]
    public async Task<IActionResult> LoginWith2fa(LoginWith2faViewModel model)
    {
      if (ModelState.IsValid)
      {
        if (!(await signInManager.TwoFactorAuthenticatorSignInAsync(model.TwoFactorToken, isPersistent: false, rememberClient: false)).Succeeded)
        {
          ModelState.AddModelError(nameof(LoginWith2faViewModel.TwoFactorToken), "Invalid authenticator code.");
        }
        else
        {
          return RedirectTo(model.ReturnUrl);
        }
      }

      return View(model);
    }

    [HttpGet]
    public IActionResult ForgotPassword(string? returnUrl) => View(new ForgotPasswordViewModel { ReturnUrl = returnUrl });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
      if (ModelState.IsValid)
      {
        User user = await userManager.FindByEmailAsync(model.Email);

        if (user != null && user.EmailConfirmed)
        {
          await emailService.SendEmailAsync(user.Email, "Reset password", await GeneratePasswordResetMessageAsync(user, model.ReturnUrl));
        }

        return RedirectToAction(nameof(ForgotPasswordSuccess));
      }

      return View(model);
    }

    [HttpGet]
    public IActionResult ForgotPasswordSuccess() => View();

    [HttpGet]
    public IActionResult ResetPassword(int? id, string? code, string? returnUrl) => View(new ResetPasswordViewModel { Id = id, Code = code, ReturnUrl = returnUrl });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
      if (ModelState.IsValid)
      {
        if (!model.Id.HasValue || string.IsNullOrWhiteSpace(model.Code))
        {
          return Redirect("/");
        }

        User user = await userManager.FindByIdAsync(model.Id.ToString());

        if (user == null)
        {
          return Redirect("/");
        }

        IdentityResult identityResetPasswordResult = await userManager.ResetPasswordAsync(user, model.Code, model.Password);

        return identityResetPasswordResult.Succeeded
          ? RedirectToAction(nameof(ResetPasswordSuccess), new { model.ReturnUrl })
          : ViewWithErrors(identityResetPasswordResult, model);
      }

      return View(model);
    }

    [HttpGet]
    public IActionResult ResetPasswordSuccess(string? returnUrl) => View(model: returnUrl);

    [HttpGet]
    public IActionResult Logout(string? logoutId) => View(new LogoutViewModel { LogoutId = logoutId });

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

    private IActionResult ViewWithErrors<T>(IdentityResult identityResult, T model)
    {
      identityResult
        .Errors
        .Select(e => e.Description)
        .ToList()
        .ForEach(d => ModelState.AddModelError(string.Empty, d));

      return View(model);
    }

    private async Task<string> GenerateEmailConfirmationMessageAsync(User user, string? returnUrl)
    {
      string callbackUrl = Url.Action(
        nameof(ConfirmEmail),
        "Account",
        new { id = user.Id, code = await userManager.GenerateEmailConfirmationTokenAsync(user), returnUrl },
        protocol: HttpContext.Request.Scheme
      );

      return $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.";
    }

    private async Task<string> GeneratePasswordResetMessageAsync(User user, string? returnUrl)
    {
      string callbackUrl = Url.Action(
        nameof(ResetPassword),
        "Account",
        new { id = user.Id, code = await userManager.GeneratePasswordResetTokenAsync(user), returnUrl },
        protocol: HttpContext.Request.Scheme
      );

      return $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.";
    }

    private IActionResult RedirectTo(string? returnUrl) => Redirect(interaction.IsValidReturnUrl(returnUrl) ? returnUrl : "/");
  }
}
