using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace TodoList.Client
{
  public class Program
  {
    public static async Task Main(string[] args)
    {
      WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);

      builder.Services
        .AddBlazoredLocalStorage()
        .AddSingleton(new HttpClient { BaseAddress = new Uri(builder.Configuration["ItemsUrl"]) })
        .AddScoped<AppState>()
        .AddScoped<IAppHttpClient, AppHttpClient>();

      builder.RootComponents.Add<App>("app");

      await builder
        .Build()
        .RunAsync();
    }
  }
}
