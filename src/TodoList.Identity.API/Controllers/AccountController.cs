using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using TodoList.Identity.API.Data.Entities;
using TodoList.Identity.API.Services;
using TodoList.Identity.API.ViewModels;

namespace TodoList.Identity.API.Controllers
{
  public class AccountController : Controller
  {
    private readonly UserManager<User> userManager;
    private readonly SignInManager<User> signInManager;
    private readonly IEmailService emailService;
    private readonly IIdentityServerInteractionService interaction;

    public AccountController(
      UserManager<User> userManager,
      SignInManager<User> signInManager,
      IEmailService emailService,
      IIdentityServerInteractionService interaction)
    {
      this.userManager = userManager;
      this.signInManager = signInManager;
      this.emailService = emailService;
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

          return View(new RegisterViewModel { ReturnUrl = model.ReturnUrl });
        }
        else if (user == null)
        {
          user = new User { UserName = model.Name, Email = model.Email };

          IdentityResult identityCreateResult = await userManager.CreateAsync(user, model.Password);

          if (!identityCreateResult.Succeeded)
          {
            return ReturnViewWithErrors(identityCreateResult, model.ReturnUrl);
          }

          await userManager.AddToRoleAsync(user, "user");
        }
        else
        {
          user.UserName = model.Name;

          foreach (IUserValidator<User> userValidator in userManager.UserValidators)
          {
            IdentityResult identityValidateResult = await userValidator.ValidateAsync(userManager, user);

            if (!identityValidateResult.Succeeded)
            {
              return ReturnViewWithErrors(identityValidateResult, model.ReturnUrl);
            }
          }

          IdentityResult identityUpdateResult = await userManager.AddPasswordAsync(user, model.Password);

          if (!identityUpdateResult.Succeeded)
          {
            return ReturnViewWithErrors(identityUpdateResult, model.ReturnUrl);
          }
        }

        await emailService.SendEmailAsync(user.Email!, "Confirm your email", await GenerateEmailConfirmationMessageAsync(user, model.ReturnUrl));

        return Redirect("/Account/RegistrationSuccess");
      }

      return View(new RegisterViewModel { ReturnUrl = model.ReturnUrl });
    }

    [HttpGet]
    public IActionResult RegistrationSuccess()
    {
      return View();
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(int? id, string? code, string? returnUrl)
    {
      return View();
    }

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

    private IActionResult ReturnViewWithErrors(IdentityResult identityResult, string? returnUrl)
    {
      identityResult
        .Errors
        .Select(e => e.Description)
        .ToList()
        .ForEach(e => ModelState.AddModelError(string.Empty, e));

      return View(new RegisterViewModel { ReturnUrl = returnUrl });
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
  }
}
