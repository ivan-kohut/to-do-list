using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using System;
using System.IO;
using System.Net.Http;
using WebApplication;
using EnvironmentName = WebApplication.EnvironmentName;

namespace Controllers.Tests.Fixtures
{
  public class TestServerFixture : IDisposable
  {
    private readonly TestServer server;

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

      this.server = new TestServer(webHostBuilder);

      Client = server.CreateClient();
    }

    public void Dispose()
    {
      Client?.Dispose();
      server?.Dispose();
    }
  }
}
