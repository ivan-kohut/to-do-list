using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Services;

namespace WebApplication
{
  public class Startup
  {
    private readonly IConfiguration configuration;

    public Startup(IConfiguration configuration)
    {
      this.configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
      services.AddDbContext<AppDbContext>(
        options => options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
      );

      services.AddMvc();
      services.AddScoped<IItemService, ItemService>();
    }

    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app.UseBrowserLink();
        app.UseDeveloperExceptionPage();
      }

      app.UseStaticFiles();

      app.UseMvc(routes =>
      {
        routes.MapRoute(
          name: "default",
          template: "{controller=Main}/{action=Index}/{id?}");
      });
    }
  }
}
