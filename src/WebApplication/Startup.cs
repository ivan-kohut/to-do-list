using Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Repositories;
using Services;
using StackExchange.Profiling;
using System.Reflection;

namespace WebApplication
{
  public class Startup
  {
    private readonly IConfiguration configuration;

    protected virtual string ConnectionStringName { get; } = "DefaultConnection";

    public Startup(IConfiguration configuration)
    {
      this.configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
      services.AddDbContext<AppDbContext>(
        options => options.UseSqlServer(configuration.GetConnectionString(ConnectionStringName))
      );

      services.AddIdentity<User, IdentityRole<int>>()
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();
      services
        .AddMiniProfiler(options => options.PopupRenderPosition = RenderPosition.Right)
        .AddEntityFramework();

      services.AddMvc()
        .AddRazorPagesOptions(options =>
        {
          options.Conventions.AuthorizePage("/Index");
        })
        .AddApplicationPart(Assembly.Load(new AssemblyName("Controllers")));

      services.AddScoped<IItemRepository, ItemRepository>();

      services.AddScoped<IItemService, ItemService>();
      services.AddSingleton<ISelectListService, SelectListService>();
    }

    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
      app.UseDeveloperExceptionPage();
      app.UseMiniProfiler();
      app.UseStaticFiles();
      app.UseHttpsRedirection();
      app.UseAuthentication();
      app.UseMvc();
    }
  }
}
