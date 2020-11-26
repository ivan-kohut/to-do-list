using Controllers.Services;
using Delegates;
using Extensions;
using Filters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Repositories;
using Services;
using StackExchange.Profiling;
using Swagger;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace WebApplication
{
  public class Startup
  {
    protected virtual string ConnectionStringName { get; } = "DefaultConnection";

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
        .AddDbContext<AppDbContext>(o => o.UseSqlServer(configuration.GetConnectionString(ConnectionStringName)));

      if (webHostEnvironment.IsDevelopment())
      {
        services
          .AddMiniProfiler(o => o.RouteBasePath = "/profiler")
          .AddEntityFramework();
      }

      services
        .Configure<ApiBehaviorOptions>(o => o.SuppressModelStateInvalidFilter = true);

      services
        .AddAuthentication(o =>
        {
          o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
          o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(o =>
        {
          o.Authority = configuration["IdentityUrl"];
          o.Audience = "items";
          o.RequireHttpsMetadata = false;
        });

      services.AddSwaggerGen(c =>
      {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
          Version = "v1",
          Title = "Todo List",
          Description = "Simple Todo List API developed using C#, ASP.NET Core 5.0 and EF Core 5.0"
        });

        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
          Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
          Name = "Authorization",
          In = ParameterLocation.Header,
          Type = SecuritySchemeType.ApiKey
        });

        c.OperationFilter<AuthResponsesOperationFilter>();

        c.DocumentFilter<LowercaseDocumentFilter>();

        c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));
      });

      services.AddCors();

      services.AddMemoryCache();

      services
        .AddControllers(o => o.Filters.Add(typeof(ModelStateInvalidFilter)))
        .AddApplicationPart(typeof(Startup).Assembly);

      services.AddScoped<IItemRepository, ItemRepository>();
      services.AddScoped<ITransactionManager, TransactionManager>();

      services.AddScoped<ItemService>();
      services.AddScoped<CachedItemService>();

      services.AddScoped<ItemServiceResolver>(serviceProvider => serviceKey =>
      {
        return serviceKey switch
        {
          "main" => serviceProvider.GetService<ItemService>(),
          "cached" => serviceProvider.GetService<CachedItemService>(),
          _ => null
        };
      });
    }

    public void Configure(IApplicationBuilder app)
    {
      app.UseHttpsRedirection();

      if (webHostEnvironment.IsDevelopment())
      {
        app.UseMiniProfiler();
      }

      app.UseSwagger(c => c.RouteTemplate = "api-docs/{documentName}/swagger.json");

      app.UseSwaggerUI(c =>
      {
        c.SwaggerEndpoint("/api-docs/v1/swagger.json", "Todo List v1");
        c.IndexStream = () => File.OpenRead(Path.Combine(AppContext.BaseDirectory, "Swagger/index.html"));
      });

      app.UseRouting();
      app.UseCors(b => b.WithOrigins(configuration["Cors:Origins"]?.Split(",").Select(o => o.Trim()).ToArray() ?? new string[] { }).AllowAnyHeader().AllowAnyMethod());
      app.UseAppExceptionHandler();
      app.UseAuthentication();
      app.UseAuthorization();
      app.UseEndpoints(endpoints => endpoints.MapControllers());
    }
  }
}
