using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
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
        await scope.ServiceProvider
          .GetRequiredService<AppDbContext>()
          .InitializeAsync();

        await scope.ServiceProvider
          .GetRequiredService<ConfigurationDbContext>()
          .InitializeAsync(scope.ServiceProvider.GetRequiredService<IConfiguration>());

        await scope.ServiceProvider
          .GetRequiredService<PersistedGrantDbContext>()
          .InitializeAsync();
      }

      await host.RunAsync();
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
