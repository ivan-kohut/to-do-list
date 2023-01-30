using HealthChecks.UI.Client;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using StackExchange.Profiling;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using TodoList.Items.API.Application.Commands;
using TodoList.Items.API.Application.Models;
using TodoList.Items.API.BackgroundServices;
using TodoList.Items.API.Extensions;
using TodoList.Items.API.Filters;
using TodoList.Items.API.Options;
using TodoList.Items.API.Swagger;
using TodoList.Items.Domain.Aggregates.ItemAggregate;
using TodoList.Items.Domain.Aggregates.UserAggregate;
using TodoList.Items.Domain.Shared;
using TodoList.Items.Infrastructure;
using TodoList.Items.Infrastructure.Repositories;

namespace TodoList.Items.API
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
            string connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new Exception("Connection string is null");

            services
                .AddDbContext<ItemsDbContext>(o => o.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(Startup).GetTypeInfo().Assembly.GetName().Name)));

            if (webHostEnvironment.IsDevelopment())
            {
                services
                    .AddMiniProfiler(o => o.RouteBasePath = "/profiler")
                    .AddEntityFramework();
            }

            services
                .Configure<ApiBehaviorOptions>(o => o.SuppressModelStateInvalidFilter = true);

            string identityUrl = configuration["IdentityUrl"]
                ?? throw new Exception("Identity URL is null");

            services
                .AddAuthentication(o =>
                {
                    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(o =>
                {
                    o.Authority = identityUrl;
                    o.Audience = "items";
                    o.RequireHttpsMetadata = false;
                });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Todo List",
                    Description = "Simple Todo List API developed using C#, ASP.NET Core 7.0 and EF Core 7.0"
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

            services.AddMediatR(Assembly.GetExecutingAssembly());

            services.AddTransient<IRequestHandler<RemoveCachedItemsCommand<CreateItemCommand, ItemDTO>, ItemDTO>, RemoveCachedItemsCommandHandler<CreateItemCommand, ItemDTO>>();
            services.AddTransient<IRequestHandler<RemoveCachedItemsCommand<UpdateItemCommand, Unit>, Unit>, RemoveCachedItemsCommandHandler<UpdateItemCommand, Unit>>();
            services.AddTransient<IRequestHandler<RemoveCachedItemsCommand<DeleteItemCommand, Unit>, Unit>, RemoveCachedItemsCommandHandler<DeleteItemCommand, Unit>>();

            services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<ItemsDbContext>());

            services.AddScoped<IItemRepository, ItemRepository>();
            services.AddScoped<IUserRepository, UserRepository>();

            services.AddHostedService<EventBusHostedService>();

            services.Configure<EventBusOptions>(configuration.GetSection("EventBus"));

            services.AddHealthChecks()
                .AddCheck("self", () => HealthCheckResult.Healthy())
                .AddSqlServer(connectionString, name: "db")
                .AddRabbitMQ("amqp://" + configuration["EventBus:Connection"], name: "rabbitmq")
                .AddUrlGroup(new Uri(identityUrl + "/health"), name: "identity-api");
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
            app.UseCors(b => b.WithOrigins(configuration["Cors:Origins"]?.Split(",").Select(o => o.Trim()).ToArray() ?? Array.Empty<string>()).AllowAnyHeader().AllowAnyMethod());
            app.UseAppExceptionHandler();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
            });
        }
    }
}
