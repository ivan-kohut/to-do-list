using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using TodoList.Identity.API.Data.Entities;

namespace TodoList.Identity.API.Pages.Account
{
    public class LogoutPageModel(SignInManager<User> signInManager, IIdentityServerInteractionService interaction) : PageModel
    {
        public async Task<IActionResult> OnPostAsync(string? logoutId)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                await signInManager.SignOutAsync();
            }

            return Redirect((await interaction.GetLogoutContextAsync(logoutId)).PostLogoutRedirectUri ?? "/");
        }
    }
}
