using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace TodoList.Client.Server
{
  public class Startup
  {
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
        app.UseWebAssemblyDebugging();
      }

      app.UseStaticFiles();
      app.UseBlazorFrameworkFiles();

      app.UseRouting();

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapFallbackToFile("index.html");
      });
    }
  }
}
