using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using System;
using System.IO;
using System.Net.Http;
using TodoList.Items.Domain.Aggregates.UserAggregate;
using TodoList.Items.Infrastructure;
using TodoList.Items.IntegrationTests.Extensions;
using Xunit;

namespace TodoList.Items.IntegrationTests.Fixtures
{
    [CollectionDefinition(nameof(IntegrationTestCollection))]
    public class IntegrationTestCollection : ICollectionFixture<TestServerFixture> { }

    public class TestServerFixture : IDisposable
    {
        public TestServer Server { get; }

        public HttpClient Client { get; }

        public User User { get; }

        public TestServerFixture()
        {
            string projectRootPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "..", "..", "..", "..", "..", "src", "services", "Items", "TodoList.Items.API");

            IWebHostBuilder webHostBuilder = WebHost.CreateDefaultBuilder()
                .UseContentRoot(projectRootPath)
                .UseEnvironment(Environments.Development)
                .UseStartup<TestStartup>();

            Server = new TestServer(webHostBuilder);
            Client = Server.CreateClient();

            AsyncRetryPolicy retryPolicy = Policy
                .Handle<SqlException>()
                .WaitAndRetryAsync(
                    3,
                    retryNumber => TimeSpan.FromSeconds(retryNumber * 2),
                    (exception, sleepDuration) => Console.WriteLine($"SQL Server connection retry, sleep duration: {sleepDuration}"));

            User? user = null;

            retryPolicy.ExecuteAsync(async () =>
            {
                using IServiceScope scope = Server.CreateScope();

                ItemsDbContext itemsDbContext = scope.ServiceProvider.GetRequiredService<ItemsDbContext>();

                await itemsDbContext.Database.MigrateAsync();

                user = await itemsDbContext.Users
                    .AsNoTracking()
                    .SingleAsync(u => u.IdentityId == 1);

            }).GetAwaiter().GetResult();

            User = user!;

            Server.Services.GetRequiredService<IOptions<RouteOptions>>().Value.SuppressCheckForUnhandledSecurityMetadata = true;
        }

        public void Dispose()
        {
            Client?.Dispose();
            Server?.Dispose();
        }
    }
}
