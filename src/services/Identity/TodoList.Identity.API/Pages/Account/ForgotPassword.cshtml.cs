using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using TodoList.Identity.API.Data.Entities;
using TodoList.Identity.API.Services;

namespace TodoList.Identity.API.Pages.Account
{
    public class ForgotPasswordPageModel(UserManager<User> userManager, IEmailService emailService) : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; } = null!;

        public async Task<IActionResult> OnPostAsync(string? returnUrl)
        {
            if (ModelState.IsValid)
            {
                User? user = await userManager.FindByEmailAsync(Input.Email!);

                if (user != null && user.EmailConfirmed)
                {
                    await emailService.SendEmailAsync(user.Email!, "Reset password", await GeneratePasswordResetMessageAsync(user, returnUrl));
                }

                return RedirectToPage("ForgotPasswordSuccess");
            }

            return Page();
        }

        private async Task<string> GeneratePasswordResetMessageAsync(User user, string? returnUrl)
        {
            string callbackUrl = Url.Page(
                "ResetPassword",
                pageHandler: null,
                values: new { id = user.Id, code = await userManager.GeneratePasswordResetTokenAsync(user), returnUrl },
                protocol: Request.Scheme)!;

            return $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.";
        }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string? Email { get; set; }
        }
    }
}
