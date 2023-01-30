using Microsoft.AspNetCore.TestHost;
using System.Net.Http;
using TodoList.Items.IntegrationTests.Infrastructure;

namespace TodoList.Items.IntegrationTests.Tests
{
    public abstract class TestBase
    {
        protected TestServer Server { get; }

        protected HttpClient Client { get; }

        protected TestBase(ItemsWebApplicationFactory applicationFactory)
        {
            this.Server = applicationFactory.Server;
            this.Client = applicationFactory.CreateClient();
        }
    }
}
