using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Polly.Retry;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;
using TodoList.Items.Infrastructure;

namespace TodoList.Items.API
{
  public class Program
  {
    static async Task Main(string[] args)
    {
      Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Warning()
        .WriteTo.Console()
        .WriteTo.Async(c => c.File(Path.Combine("Logs", "log-.txt"), rollingInterval: RollingInterval.Day))
        .CreateLogger();

      try
      {
        IHost host = CreateHostBuilder(args).Build();

        using IServiceScope scope = host.Services.CreateScope();

        if (scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>().IsDevelopment())
        {
          AsyncRetryPolicy retryPolicy = Policy
            .Handle<SqlException>()
            .WaitAndRetryAsync(3, retryNumber => TimeSpan.FromSeconds(retryNumber * 2), (exception, sleepDuration) => Console.WriteLine($"SQL Server connection retry, sleep duration: {sleepDuration}"));

          await retryPolicy.ExecuteAsync(async () =>
          {
            await scope.ServiceProvider
              .GetRequiredService<ItemsDbContext>()
              .Database
              .MigrateAsync();
          });
        }

        await host.RunAsync();
      }
      catch (Exception ex)
      {
        Log.Fatal(ex, "Host terminated unexpectedly");
      }
      finally
      {
        Log.CloseAndFlush();
      }
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host
          .CreateDefaultBuilder(args)
          .ConfigureWebHostDefaults(webBuilder =>
          {
            webBuilder
              .UseStartup<Startup>()
              .UseSerilog();
          });
  }
}
