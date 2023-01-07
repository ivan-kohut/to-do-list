using Microsoft.AspNetCore.Mvc;

namespace TodoList.Items.API.Controllers
{
    [Route("/")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class HomeController : Controller
    {
        [HttpGet]
        public IActionResult Index() => Redirect("/swagger");
    }
}
