using Controllers.Tests.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebApplication;

namespace Controllers.Tests
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

    protected override void AddHostedServices(IServiceCollection services)
    {
      // Hosted services shouldn't be added for tests
    }
  }
}
