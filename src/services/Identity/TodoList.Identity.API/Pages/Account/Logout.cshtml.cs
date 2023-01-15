using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using TodoList.Identity.API.Data.Entities;

namespace TodoList.Identity.API.Pages.Account
{
    public class LogoutPageModel : PageModel
    {
        private readonly SignInManager<User> signInManager;
        private readonly IIdentityServerInteractionService interaction;

        public LogoutPageModel(SignInManager<User> signInManager, IIdentityServerInteractionService interaction)
        {
            this.signInManager = signInManager;
            this.interaction = interaction;
        }

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
