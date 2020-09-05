using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Repositories;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;

namespace WebApplication
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

        IWebHostEnvironment webHostEnvironment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

        if (webHostEnvironment.IsDevelopment())
        {
          await scope.ServiceProvider
            .GetRequiredService<AppDbContext>()
            .Database
            .MigrateAsync();
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
