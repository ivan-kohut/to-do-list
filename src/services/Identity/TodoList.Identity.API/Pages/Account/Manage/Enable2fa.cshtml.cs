using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using TodoList.Identity.API.Data.Entities;

namespace TodoList.Identity.API.Pages.Account.Manage
{
    [Authorize(Roles = "user")]
    public class Enable2faPageModel(UserManager<User> userManager) : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync()
        {
            User? user = await userManager.GetUserAsync(User);

            if (user is null)
            {
                return NotFound();
            }
            else if (user.TwoFactorEnabled)
            {
                return RedirectToPage("Index");
            }

            string? authenticatorKey = await userManager.GetAuthenticatorKeyAsync(user);

            if (string.IsNullOrWhiteSpace(authenticatorKey))
            {
                await userManager.ResetAuthenticatorKeyAsync(user);

                authenticatorKey = await userManager.GetAuthenticatorKeyAsync(user);
            }

            Input = new InputModel
            {
                AuthenticatorUri = $"otpauth://totp/ToDoList:{user.UserName}?secret={authenticatorKey}&issuer=ToDoList&digits=6"
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                User? user = await userManager.GetUserAsync(User);

                if (user is null)
                {
                    return NotFound();
                }
                else if (user.TwoFactorEnabled)
                {
                    return RedirectToPage("Index");
                }
                else if (await userManager.VerifyTwoFactorTokenAsync(user, userManager.Options.Tokens.AuthenticatorTokenProvider, Input.Code!))
                {
                    await userManager.SetTwoFactorEnabledAsync(user, enabled: true);

                    return RedirectToPage("Index");
                }
                else
                {
                    ModelState.AddModelError($"{nameof(Input)}.{nameof(Input.Code)}", "Invalid authenticator code.");
                }
            }

            return Page();
        }

        public class InputModel
        {
            [Required]
            [RegularExpression("[0-9]{6}")]
            public string? Code { get; set; }

            [Required]
            public string? AuthenticatorUri { get; set; }
        }
    }
}
