using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TodoList.Identity.API.Pages.Account
{
    public class ResetPasswordSuccessPageModel : PageModel
    {
        public string? ReturnUrl { get; set; }

        public IActionResult OnGet(string? returnUrl)
        {
            ReturnUrl = returnUrl;

            return Page();
        }
    }
}
