using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace TodoList.Identity.API.Pages
{
    public class ErrorPageModel : PageModel
    {
        private readonly IIdentityServerInteractionService interaction;

        public ErrorPageModel(IIdentityServerInteractionService interaction)
        {
            this.interaction = interaction;
        }

        public ErrorMessage? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(string? errorId)
        {
            ErrorMessage = await interaction.GetErrorContextAsync(errorId);

            return Page();
        }
    }
}
