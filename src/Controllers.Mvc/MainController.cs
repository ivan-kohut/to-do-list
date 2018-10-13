using Microsoft.AspNetCore.Mvc;

namespace Controllers
{
  public class MainController : Controller
  {
    [HttpGet]
    public IActionResult Index()
    {
      return this.View();
    }
  }
}
