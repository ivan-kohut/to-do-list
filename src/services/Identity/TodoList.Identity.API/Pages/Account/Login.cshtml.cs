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
    public class LoginPageModel(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IIdentityServerInteractionService interaction) : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; } = null!;

        public string? ReturnUrl { get; set; }

        public IActionResult OnGet(string? returnUrl)
        {
            ReturnUrl = returnUrl;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl)
        {
            if (ModelState.IsValid)
            {
                User? user = await userManager.FindByEmailAsync(Input.Email!);

                if (user == null)
                {
                    ModelState.AddModelError($"{nameof(Input)}.{nameof(Input.Email)}", "The User is not found.");
                }
                else if (!await userManager.CheckPasswordAsync(user, Input.Password!))
                {
                    ModelState.AddModelError($"{nameof(Input)}.{nameof(Input.Password)}", "The Password is invalid.");
                }
                else if (!user.EmailConfirmed)
                {
                    ModelState.AddModelError($"{nameof(Input)}.{nameof(Input.Email)}", "The Email is not confirmed.");
                }
                else
                {
                    SignInResult signInResult = await signInManager.PasswordSignInAsync(user, Input.Password!, isPersistent: false, lockoutOnFailure: true);

                    if (signInResult.Succeeded)
                    {
                        return Redirect(interaction.IsValidReturnUrl(returnUrl) ? returnUrl! : "/");
                    }
                    else if (signInResult.RequiresTwoFactor)
                    {
                        return RedirectToPage("LoginWith2fa", new { returnUrl });
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    }
                }
            }

            ReturnUrl = returnUrl;

            return Page();
        }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string? Email { get; set; }

            [Required]
            public string? Password { get; set; }
        }
    }
}
