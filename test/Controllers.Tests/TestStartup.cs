using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using WebApplication;

namespace Controllers.Tests
{
  public class TestStartup : Startup
  {
    protected override string ConnectionStringName { get; } = "DefaultTestConnection";

    public TestStartup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment) : base(configuration, webHostEnvironment)
    {
    }
  }
}
