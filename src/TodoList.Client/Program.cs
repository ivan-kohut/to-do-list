using Blazored.LocalStorage;
using Microsoft.AspNetCore.Blazor.Hosting;
using Microsoft.Extensions.DependencyInjection;
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
        .AddScoped<AppState>()
        .AddScoped<IAppHttpClient, AppHttpClient>();

      builder.RootComponents.Add<App>("app");

      await builder
        .Build()
        .RunAsync();
    }
  }
}
