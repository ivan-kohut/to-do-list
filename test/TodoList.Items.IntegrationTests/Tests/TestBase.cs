using Microsoft.AspNetCore.TestHost;
using System.Net.Http;
using TodoList.Items.IntegrationTests.Infrastructure;

namespace TodoList.Items.IntegrationTests.Tests
{
    public abstract class TestBase(ItemsWebApplicationFactory applicationFactory)
    {
        protected TestServer Server { get; } = applicationFactory.Server;

        protected HttpClient Client { get; } = applicationFactory.CreateClient();
    }
}
