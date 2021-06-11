using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using TodoList.Items.API;
using TodoList.Items.IntegrationTests.Middlewares;

namespace TodoList.Items.IntegrationTests
{
  public class TestStartup : Startup
  {
    protected override string ConnectionStringName { get; } = "DefaultTestConnection";

    public TestStartup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment) : base(configuration, webHostEnvironment)
    {
    }

    protected override void ConfigureAuth(IApplicationBuilder app)
    {
      app.UseMiddleware<AuthMiddleware>();
    }
  }
}
