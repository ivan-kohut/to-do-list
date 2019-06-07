using Microsoft.AspNetCore.Components.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace TodoList.Client
{
  public class Startup
  {
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddScoped<IAppState, AppState>();
      services.AddScoped<IAppHttpClient, AppHttpClient >();
    }

    public void Configure(IComponentsApplicationBuilder app)
    {
      app.AddComponent<App>("app");
    }
  }
}
