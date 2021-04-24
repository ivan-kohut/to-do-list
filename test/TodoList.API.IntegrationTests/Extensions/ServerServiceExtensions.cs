using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Controllers.Tests.Extensions
{
  public static class ServerServiceExtensions
  {
    public static IServiceScope CreateScope(this TestServer server)
    {
      return server.Host.Services.CreateScope();
    }
  }
}
