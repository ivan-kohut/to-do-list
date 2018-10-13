using Microsoft.AspNetCore.Mvc;

namespace WebApplication.Controllers
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
