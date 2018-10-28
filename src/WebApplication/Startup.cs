using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Repositories;
using Services;
using System.Reflection;

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

      services.AddMvc()
        .AddApplicationPart(Assembly.Load(new AssemblyName("Controllers")));

      services.AddScoped<IItemRepository, ItemRepository>();

      services.AddScoped<IItemService, ItemService>();
      services.AddScoped<IDbTransactionManager, DbTransactionManager>();
    }

    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app.UseBrowserLink();
        app.UseDeveloperExceptionPage();
      }

      app.UseStaticFiles();
      app.UseMvc();
    }
  }
}
