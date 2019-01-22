using Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Repositories;
using Services;
using StackExchange.Profiling;
using System.Text;

namespace WebApplication
{
  public class Startup
  {
    private const string secret = "peRhtr7oth7nh98rtx78Tdy0g98graKfYrovjhaz5dX75h56trOdKvnghruGYdxm";

    protected virtual string ConnectionStringName { get; } = "DefaultConnection";
    private SecurityKey SecurityKey { get; } = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret));

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
        .AddIdentity<User, IdentityRole<int>>()
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();
      services
        .AddMiniProfiler(options => options.PopupRenderPosition = RenderPosition.Right)
        .AddEntityFramework();

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
            IssuerSigningKey = SecurityKey
          };
        });

      services.AddMvc();

      services.Configure<JwtOptions>(o => o.SecurityKey = SecurityKey);

      services.AddScoped<IItemRepository, ItemRepository>();

      services.AddScoped<IItemService, ItemService>();
      services.AddSingleton<ISelectListService, SelectListService>();
      services.AddSingleton<IJwtTokenService, JwtTokenService>();
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
