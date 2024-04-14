using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using TodoList.Identity.API.Data.Entities;

namespace TodoList.Identity.API.Pages.Account.Manage
{
    [Authorize(Roles = "user")]
    public class Disable2faPageModel(UserManager<User> userManager) : PageModel
    {
        public async Task<IActionResult> OnGetAsync()
        {
            User? user = await userManager.GetUserAsync(User);

            if (user is null)
            {
                return NotFound();
            }
            else if (!user.TwoFactorEnabled)
            {
                return RedirectToPage("Index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            User? user = await userManager.GetUserAsync(User);

            if (user is null)
            {
                return NotFound();
            }
            else if (user.TwoFactorEnabled)
            {
                await userManager.SetTwoFactorEnabledAsync(user, enabled: false);
                await userManager.ResetAuthenticatorKeyAsync(user);
            }

            return RedirectToPage("Index");
        }
    }
}
