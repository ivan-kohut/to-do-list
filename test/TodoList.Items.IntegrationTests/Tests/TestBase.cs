using Microsoft.AspNetCore.TestHost;
using System.Net.Http;
using TodoList.Items.IntegrationTests.Fixtures;

namespace TodoList.Items.IntegrationTests.Tests
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
