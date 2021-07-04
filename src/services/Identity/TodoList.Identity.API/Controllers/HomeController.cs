using IdentityServer4.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TodoList.Identity.API.ViewModels;

namespace TodoList.Identity.API.Controllers
{
  public class HomeController : Controller
  {
    private readonly IIdentityServerInteractionService interaction;

    public HomeController(IIdentityServerInteractionService interaction)
    {
      this.interaction = interaction;
    }

    [HttpGet]
    public IActionResult Index() => Redirect("/.well-known/openid-configuration");

    public async Task<IActionResult> Error(string errorId) =>
      View(new ErrorViewModel { Error = await interaction.GetErrorContextAsync(errorId) });
  }
}
