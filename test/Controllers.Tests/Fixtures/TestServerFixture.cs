using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Repositories;
using System;
using System.IO;
using System.Net.Http;
using WebApplication;
using EnvironmentName = WebApplication.EnvironmentName;

namespace Controllers.Tests.Fixtures
{
  public class TestServerFixture : IDisposable
  {
    public TestServer Server { get; }
    public HttpClient Client { get; }

    public TestServerFixture()
    {
      string projectRootPath = Path.Combine(
        Directory.GetCurrentDirectory(),
        "..", "..", "..", "..", "..", "src", "WebApplication"
      );

      IWebHostBuilder webHostBuilder = WebHost.CreateDefaultBuilder()
        .UseEnvironment(EnvironmentName.Testing)
        .UseContentRoot(projectRootPath)
        .UseStartup<Startup>();

      Server = new TestServer(webHostBuilder);
      Client = Server.CreateClient();
    }

    public void Dispose()
    {
      Server.Host.Services.GetRequiredService<AppDbContext>().Dispose();
      Client?.Dispose();
      Server?.Dispose();
    }
  }
}
