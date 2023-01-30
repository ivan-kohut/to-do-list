using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Polly.Retry;
using System;
using System.IO;
using System.Linq;
using TodoList.Items.API;
using TodoList.Items.API.BackgroundServices;
using TodoList.Items.Domain.Aggregates.UserAggregate;
using TodoList.Items.Infrastructure;

namespace TodoList.Items.IntegrationTests.Infrastructure
{
    public class ItemsWebApplicationFactory : WebApplicationFactory<Startup>
    {
        public User User { get; set; } = null!;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder
                .ConfigureAppConfiguration(configureBuilder =>
                {
                    configureBuilder.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.IntegrationTests.json"));
                    configureBuilder.AddEnvironmentVariables();
                })
                .ConfigureTestServices(services =>
                {
                    ServiceDescriptor? eventBusHostedService = services
                        .FirstOrDefault(s => s.ImplementationType == typeof(EventBusHostedService));

                    if (eventBusHostedService is not null)
                    {
                        services.Remove(eventBusHostedService);
                    }

                    services
                        .AddAuthentication(options =>
                        {
                            options.DefaultScheme = StubAuthenticationHandler.AuthenticationScheme;
                            options.DefaultAuthenticateScheme = StubAuthenticationHandler.AuthenticationScheme;
                        })
                        .AddScheme<AuthenticationSchemeOptions, StubAuthenticationHandler>(StubAuthenticationHandler.AuthenticationScheme, options => { });
                });
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            IHost host = base.CreateHost(builder);

            AsyncRetryPolicy retryPolicy = Policy
                .Handle<SqlException>()
                .WaitAndRetryAsync(
                    3,
                    retryNumber => TimeSpan.FromSeconds(retryNumber * 2),
                    (exception, sleepDuration) => Console.WriteLine($"SQL Server connection retry, sleep duration: {sleepDuration}"));

            User? user = null;

            retryPolicy.ExecuteAsync(async () =>
            {
                using IServiceScope scope = host.Services.CreateScope();

                ItemsDbContext itemsDbContext = scope.ServiceProvider.GetRequiredService<ItemsDbContext>();

                await itemsDbContext.Database.MigrateAsync();

                user = await itemsDbContext.Users
                    .AsNoTracking()
                    .SingleAsync(user => user.IdentityId == 1);

            }).GetAwaiter().GetResult();

            User = user!;

            return host;
        }
    }
}
