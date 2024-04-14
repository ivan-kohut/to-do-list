using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using TodoList.Identity.API.Data.Entities;

namespace TodoList.Identity.API.Pages.Account.Manage
{
    [Authorize(Roles = "user")]
    public class IndexPageModel(UserManager<User> userManager) : PageModel
    {
        public bool IsTwoFactorAuthenticationEnabled { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            User? user = await userManager.GetUserAsync(User);

            if (user is null)
            {
                return NotFound();
            }

            IsTwoFactorAuthenticationEnabled = user.TwoFactorEnabled;

            return Page();
        }
    }
}
