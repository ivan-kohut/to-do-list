using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Xunit;

namespace Controllers.Tests.Fixtures
{
  [CollectionDefinition(nameof(IntegrationTestCollection))]
  public class IntegrationTestCollection : ICollectionFixture<TestServerFixture> { }

  public class TestServerFixture : IDisposable
  {
    public TestServer Server { get; }
    public HttpClient Client { get; }
    public int UserId { get; }

    public TestServerFixture()
    {
      string projectRootPath = Path.Combine(
        Directory.GetCurrentDirectory(),
        "..", "..", "..", "..", "..", "src", "TodoList.API"
      );

      IWebHostBuilder webHostBuilder = WebHost.CreateDefaultBuilder()
        .UseContentRoot(projectRootPath)
        .UseStartup<TestStartup>();

      Server = new TestServer(webHostBuilder);
      Client = Server.CreateClient();

      object user = new
      {
        email = "admin@admin.admin",
        password = "abcABC123."
      };

      using HttpContent httpContent = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");
      
      string userToken = JsonConvert.DeserializeObject<string>(
        Client.PostAsync("/api/v1/users/login", httpContent).Result.Content.ReadAsStringAsync().Result
      );

      UserId = int.Parse(
        (new JwtSecurityTokenHandler().ReadToken(userToken) as JwtSecurityToken)!
          .Claims
          .Single(c => c.Type == ClaimTypes.NameIdentifier)
          .Value
      );

      Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);
    }

    public void Dispose()
    {
      Client?.Dispose();
      Server?.Dispose();
    }
  }
}
