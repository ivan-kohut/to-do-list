using Duende.IdentityServer;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using TodoList.Identity.API.Data;
using TodoList.Identity.API.Data.Entities;
using TodoList.Identity.API.Options;
using TodoList.Identity.API.Services;

namespace TodoList.Identity.API
{
  public class Startup
  {
    private readonly IConfiguration configuration;
    private readonly IWebHostEnvironment webHostEnvironment;

    public Startup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
    {
      this.configuration = configuration;
      this.webHostEnvironment = webHostEnvironment;
    }

    public void ConfigureServices(IServiceCollection services)
    {
      services
        .AddControllersWithViews();

      string connectionString = configuration.GetConnectionString("DefaultConnection");

      services
        .AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

      services
        .AddIdentity<User, Role>()
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

      string? migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

      services
        .AddIdentityServer()
        .AddConfigurationStore(options =>
        {
          options.ConfigureDbContext = b => b.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(migrationsAssembly));
        })
        .AddOperationalStore(options =>
        {
          options.ConfigureDbContext = b => b.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(migrationsAssembly));
        })
        .AddAspNetIdentity<User>()
        .AddDeveloperSigningCredential();

      services.AddAuthentication()
        .AddFacebook(o =>
        {
          o.AppId = configuration["Facebook:AppId"];
          o.AppSecret = configuration["Facebook:AppSecret"];
          o.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
        })
        .AddGoogle(o =>
        {
          o.ClientId = configuration["Google:ClientId"];
          o.ClientSecret = configuration["Google:ClientSecret"];
          o.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
        })
        .AddGitHub(o =>
        {
          o.ClientId = configuration["Github:ClientId"];
          o.ClientSecret = configuration["Github:ClientSecret"];
          o.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
        });

      services.Configure<SendGridOptions>(configuration.GetSection("SendGrid"));
      services.Configure<EventBusOptions>(configuration.GetSection("EventBus"));

      services.AddSingleton<IEmailService, EmailService>();
      services.AddSingleton<IEventBusService, RabbitMQEventBusService>();

      services.AddHealthChecks()
        .AddCheck("self", () => HealthCheckResult.Healthy())
        .AddSqlServer(connectionString, name: "db")
        .AddRabbitMQ("amqp://" + configuration["EventBus:Connection"], name: "rabbitmq");

      if (webHostEnvironment.IsDevelopment())
      {
        services
          .AddMiniProfiler(o => o.RouteBasePath = "/profiler")
          .AddEntityFramework();
      }
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      app.UseHttpsRedirection();

      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
        app.UseMiniProfiler();
      }

      app.UseStaticFiles();

      app.UseRouting();
      app.UseIdentityServer();
      app.UseAuthorization();
      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllerRoute(name: "Delete user", pattern: "admin/users/delete/{id}", defaults: new { controller = "Admin", action = "DeleteUser" });
        endpoints.MapDefaultControllerRoute();
        endpoints.MapHealthChecks("/health", new HealthCheckOptions
        {
          Predicate = _ => true,
          ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
      });
    }
  }
}
