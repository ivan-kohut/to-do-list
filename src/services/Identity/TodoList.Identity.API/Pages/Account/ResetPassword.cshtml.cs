using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using TodoList.Identity.API.Data.Entities;

namespace TodoList.Identity.API.Pages.Account
{
    public class ResetPasswordPageModel : PageModel
    {
        private readonly UserManager<User> userManager;

        public ResetPasswordPageModel(UserManager<User> userManager)
        {
            this.userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = null!;

        public async Task<IActionResult> OnPostAsync(int? id, string? code, string? returnUrl)
        {
            if (ModelState.IsValid)
            {
                if (!id.HasValue || string.IsNullOrWhiteSpace(code))
                {
                    return Redirect("/");
                }

                User? user = await userManager.FindByIdAsync(id.Value.ToString());

                if (user == null)
                {
                    return Redirect("/");
                }

                IdentityResult identityResetPasswordResult = await userManager.ResetPasswordAsync(user, code, Input.Password!);

                if (identityResetPasswordResult.Succeeded)
                {
                    return RedirectToPage("ResetPasswordSuccess", new { returnUrl });
                }

                AddModelErrors(identityResetPasswordResult);
            }

            return Page();
        }

        private void AddModelErrors(IdentityResult identityResult)
        {
            foreach (IdentityError error in identityResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        public class InputModel
        {
            [Required]
            public string? Password { get; set; }

            [Required]
            [Compare(nameof(Password))]
            public string? ConfirmPassword { get; set; }
        }
    }
}
