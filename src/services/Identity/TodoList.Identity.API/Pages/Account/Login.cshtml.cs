using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using TodoList.Identity.API.Data.Entities;

using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace TodoList.Identity.API.Pages.Account
{
    public class LoginPageModel : PageModel
    {
        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signInManager;
        private readonly IIdentityServerInteractionService interaction;

        public LoginPageModel(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IIdentityServerInteractionService interaction)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.interaction = interaction;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IActionResult OnGet(string? returnUrl)
        {
            Input = new InputModel { ReturnUrl = returnUrl };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                User? user = await userManager.FindByEmailAsync(Input.Email!);

                if (user == null)
                {
                    // TO DO
                    ModelState.AddModelError("Input." + nameof(InputModel.Email), "The User is not found.");
                }
                else if (!await userManager.CheckPasswordAsync(user, Input.Password!))
                {
                    ModelState.AddModelError(nameof(InputModel.Password), "The Password is invalid.");
                }
                else if (!user.EmailConfirmed)
                {
                    ModelState.AddModelError(nameof(InputModel.Email), "The Email is not confirmed.");
                }
                else
                {
                    SignInResult signInResult = await signInManager.PasswordSignInAsync(user, Input.Password!, isPersistent: false, lockoutOnFailure: true);

                    if (signInResult.Succeeded)
                    {
                        return Redirect(interaction.IsValidReturnUrl(Input.ReturnUrl) ? Input.ReturnUrl! : "/");
                    }
                    else if (signInResult.RequiresTwoFactor)
                    {
                        // TO DO
                        return RedirectToPage("LoginWith2fa", new { Input.ReturnUrl });
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    }
                }
            }

            return Page();
        }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string? Email { get; set; }

            [Required]
            public string? Password { get; set; }

            public string? ReturnUrl { get; set; }
        }
    }
}
