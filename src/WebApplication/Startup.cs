using Entities;
using Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Options;
using Repositories;
using Services;
using StackExchange.Profiling;
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
        .AddDbContext<AppDbContext>(
          options => options.UseSqlServer(configuration.GetConnectionString(ConnectionStringName))
        );

      services
        .AddIdentity<User, Role>()
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();
      services
        .AddMiniProfiler(options => options.PopupRenderPosition = RenderPosition.Right)
        .AddEntityFramework();

      services
        .AddHttpClient();

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

      services.AddMvc();

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

      services.AddScoped<IItemService, ItemService>();
      services.AddScoped<IUserService, UserService>();
      services.AddScoped<IUserRoleService, UserRoleService>();
      services.AddScoped<IUserLoginService, UserLoginService>();
      services.AddSingleton<IEmailService>(p => new EmailService(configuration["SendGrid:ApiKey"]));
      services.AddSingleton<ISelectListService, SelectListService>();
    }

    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
      app.UseMiniProfiler();
      app.UseStaticFiles();
      app.UseHttpsRedirection();
      app.UseAppExceptionHandler();
      app.UseAuthentication();
      app.UseMvc();
    }
  }
}
