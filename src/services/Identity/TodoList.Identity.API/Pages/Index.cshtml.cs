using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TodoList.Identity.API.Pages
{
    public class IndexPageModel : PageModel
    {
        public IActionResult OnGet()
        {
            return Redirect("/.well-known/openid-configuration");
        }
    }
}
