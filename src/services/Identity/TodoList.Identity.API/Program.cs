using Duende.IdentityServer.EntityFramework.DbContexts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Polly.Retry;
using Serilog;
using System;
using System.Threading.Tasks;
using TodoList.Identity.API.Data;
using TodoList.Identity.API.Data.Seed;

namespace TodoList.Identity.API
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            IHost host = CreateHostBuilder(args).Build();

            using IServiceScope scope = host.Services.CreateScope();

            if (scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>().IsDevelopment())
            {
                AsyncRetryPolicy retryPolicy = Policy
                    .Handle<SqlException>()
                    .WaitAndRetryAsync(
                        retryCount: 3,
                        retryNumber => TimeSpan.FromSeconds(retryNumber * 2),
                        (exception, sleepDuration) => Console.WriteLine($"SQL Server connection retry, sleep duration: {sleepDuration}"));

                await retryPolicy.ExecuteAsync(async () =>
                {
                    await scope.ServiceProvider
                        .GetRequiredService<AppDbContext>()
                        .InitializeAsync();

                    await scope.ServiceProvider
                        .GetRequiredService<ConfigurationDbContext>()
                        .InitializeAsync(scope.ServiceProvider.GetRequiredService<IConfiguration>());

                    await scope.ServiceProvider
                        .GetRequiredService<PersistedGrantDbContext>()
                        .InitializeAsync();
                });
            }

            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host
                .CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
