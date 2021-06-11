using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace TodoList.Items.IntegrationTests.Extensions
{
  public static class ServerServiceExtensions
  {
    public static IServiceScope CreateScope(this TestServer server)
    {
      return server.Host.Services.CreateScope();
    }
  }
}
