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
    public class ChangePasswordPageModel(UserManager<User> userManager) : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; } = null!;

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                User? user = await userManager.GetUserAsync(User);

                if (user is null)
                {
                    return NotFound();
                }

                IdentityResult changePasswordResult = await userManager.ChangePasswordAsync(user, Input.OldPassword!, Input.NewPassword!);

                if (changePasswordResult.Succeeded)
                {
                    return RedirectToPage("Index");
                }

                foreach (IdentityError error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return Page();
        }

        public class InputModel
        {
            [Required]
            public string? OldPassword { get; set; }

            [Required]
            public string? NewPassword { get; set; }

            [Required]
            [Compare(nameof(NewPassword))]
            public string? ConfirmNewPassword { get; set; }
        }
    }
}
