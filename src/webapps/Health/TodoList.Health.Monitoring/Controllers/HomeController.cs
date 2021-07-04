using Microsoft.AspNetCore.Mvc;

namespace TodoList.Health.Monitoring.Controllers
{
  public class HomeController : ControllerBase
  {
    [HttpGet]
    public IActionResult Index() => Redirect("/health-monitoring");
  }
}
