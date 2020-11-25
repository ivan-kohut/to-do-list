using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TodoList.Client.Server.Options;

namespace TodoList.Client.Server.Controllers
{
  [ApiController]
  [Route("[controller]")]
  public class ConfigurationController : ControllerBase
  {
    [HttpGet]
    public IActionResult GetAppOptions([FromServices] IOptions<AppOptions> appOptions) => Ok(appOptions.Value);
  }
}
