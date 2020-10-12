using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TodoList.Client.Server.Options;

namespace TodoList.Client.Server.Controllers
{
  [ApiController]
  [Route("[controller]")]
  public class ConfigurationController : ControllerBase
  {
    private readonly AppOptions appOptions;

    public ConfigurationController(IOptions<AppOptions> appOptions)
    {
      this.appOptions = appOptions.Value;
    }

    [HttpGet]
    public IActionResult GetAppOptions() => Ok(appOptions);
  }
}
