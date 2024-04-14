using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using TodoList.Identity.API.Data.Entities;

using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace TodoList.Identity.API.Pages.Account
{
    [Authorize(AuthenticationSchemes = "Identity.TwoFactorUserId")]
    public class LoginWith2faPageModel(
        SignInManager<User> signInManager,
        IIdentityServerInteractionService interaction) : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; } = null!;

        public async Task<IActionResult> OnPostAsync(string? returnUrl)
        {
            if (ModelState.IsValid)
            {
                SignInResult signInResult = await signInManager.TwoFactorAuthenticatorSignInAsync(
                    Input.TwoFactorToken!,
                    isPersistent: false,
                    rememberClient: false);

                if (signInResult.Succeeded)
                {
                    return Redirect(interaction.IsValidReturnUrl(returnUrl) ? returnUrl! : "/");
                }

                ModelState.AddModelError($"{nameof(Input)}.{nameof(Input.TwoFactorToken)}", "Invalid authenticator code.");
            }

            return Page();
        }

        public class InputModel
        {
            [Required]
            [RegularExpression("[0-9]{6}")]
            public string? TwoFactorToken { get; set; }
        }
    }
}
