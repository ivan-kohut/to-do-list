using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using System;
using System.IO;
using System.Net.Http;
using Xunit;

namespace Controllers.Tests.Fixtures
{
  [CollectionDefinition(nameof(IntegrationTestCollection))]
  public class IntegrationTestCollection : ICollectionFixture<TestServerFixture> { }

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
        .UseContentRoot(projectRootPath)
        .UseStartup<TestStartup>();

      Server = new TestServer(webHostBuilder);
      Client = Server.CreateClient();
    }

    public void Dispose()
    {
      Client?.Dispose();
      Server?.Dispose();
    }
  }
}
