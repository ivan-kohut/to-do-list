using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using TodoList.Identity.API.Data.Entities;

namespace TodoList.Identity.API.Pages.Account
{
    public class ConfirmEmailPageModel(UserManager<User> userManager) : PageModel
    {
        public string? ReturnUrl { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id, string? code, string? returnUrl)
        {
            if (!id.HasValue || string.IsNullOrWhiteSpace(code))
            {
                return Redirect("/");
            }

            User? user = await userManager.FindByIdAsync(id.Value.ToString());

            if (user == null || user.EmailConfirmed || !(await userManager.ConfirmEmailAsync(user, code)).Succeeded)
            {
                return Redirect("/");
            }

            ReturnUrl = returnUrl;

            return Page();
        }
    }
}
