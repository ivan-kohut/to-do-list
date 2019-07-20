using Controllers.Services;
using Delegates;
using Entities;
using Extensions;
using Filters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Options;
using Repositories;
using Services;
using StackExchange.Profiling;
using Swagger;
using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace WebApplication
{
  public class Startup
  {
    protected virtual string ConnectionStringName { get; } = "DefaultConnection";

    private readonly IConfiguration configuration;

    public Startup(IConfiguration configuration)
    {
      this.configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
      services
        .AddDbContext<AppDbContext>(o => o.UseSqlServer(configuration.GetConnectionString(ConnectionStringName)));

      services
        .AddIdentity<User, Role>()
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

      services
        .AddMiniProfiler(o => o.RouteBasePath = "/profiler")
        .AddEntityFramework();

      services
        .AddHttpClient();

      services
        .Configure<ApiBehaviorOptions>(o => o.SuppressModelStateInvalidFilter = true);

      SecurityKey securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration["Jwt:Secret"]));

      services
        .AddAuthentication(o =>
        {
          o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
          o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(o =>
        {
          o.TokenValidationParameters = new TokenValidationParameters
          {
            ValidateAudience = false,
            ValidateIssuer = false,
            IssuerSigningKey = securityKey
          };
        });

      services.AddSwaggerGen(c =>
      {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
          Version = "v1",
          Title = "Todo List",
          Description = "Simple Todo List API developed using C#, ASP.NET Core 2.2 and EF Core 2.2"
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

      services.AddMvc(o => o.Filters.Add(typeof(ModelStateInvalidFilter)));

      services.Configure<JwtOptions>(o => o.SecurityKey = securityKey);
      services.Configure<FacebookOptions>(configuration.GetSection("Facebook"));
      services.Configure<GoogleOptions>(configuration.GetSection("Google"));
      services.Configure<GithubOptions>(configuration.GetSection("Github"));
      services.Configure<LinkedInOptions>(configuration.GetSection("LinkedIn"));

      services.AddScoped<IItemRepository, ItemRepository>();
      services.AddScoped<IUserRepository, UserRepository>();
      services.AddScoped<IRoleRepository, RoleRepository>();
      services.AddScoped<IUserRoleRepository, UserRoleRepository>();
      services.AddScoped<IUserLoginRepository, UserLoginRepository>();
      services.AddScoped<ITransactionManager, TransactionManager>();

      services.AddScoped<ItemService>();
      services.AddScoped<CachedItemService>();

      services.AddScoped<ItemServiceResolver>(serviceProvider => serviceKey =>
      {
        switch (serviceKey)
        {
          case "main":
            return serviceProvider.GetService<ItemService>();
          case "cached":
            return serviceProvider.GetService<CachedItemService>();
          default:
            return null;
        }
      });

      services.AddScoped<IUserService, UserService>();
      services.AddScoped<IUserRoleService, UserRoleService>();
      services.AddScoped<IUserLoginService, UserLoginService>();
      services.AddSingleton<IEmailService>(p => new EmailService(configuration["SendGrid:ApiKey"]));
    }

    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
      app.UseHttpsRedirection();
      app.UseMiniProfiler();
      app.UseSwagger(c => c.RouteTemplate = "api-docs/{documentName}/swagger.json");

      app.UseSwaggerUI(c =>
      {
        c.SwaggerEndpoint("/api-docs/v1/swagger.json", "Todo List v1");
        c.IndexStream = () => File.OpenRead("Swagger/index.html");
      });

      app.UseCors(b => b.WithOrigins("http://localhost:5000", "https://localhost:44328").AllowAnyHeader().AllowAnyMethod());
      app.UseAppExceptionHandler();
      app.UseAuthentication();
      app.UseMvc();
    }
  }
}
