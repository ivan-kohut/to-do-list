using Controllers.Tests.Fixtures;
using Microsoft.AspNetCore.TestHost;
using System.Net.Http;

namespace Controllers.Tests
{
  public abstract class TestBase
  {
    protected TestServer Server { get; }
    protected HttpClient Client { get; }

    protected TestBase(TestServerFixture testServerFixture)
    {
      this.Server = testServerFixture.Server;
      this.Client = testServerFixture.Client;
    }
  }
}
