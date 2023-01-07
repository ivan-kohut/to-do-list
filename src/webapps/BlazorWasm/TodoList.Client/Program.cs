using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace TodoList.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);

            builder.Services
                .AddOidcAuthentication(options =>
                {
                    options.ProviderOptions.Authority = builder.Configuration["IdentityUrl"];
                    options.ProviderOptions.ClientId = "wasm";
                    options.ProviderOptions.ResponseType = "code";
                    options.ProviderOptions.PostLogoutRedirectUri = builder.HostEnvironment.BaseAddress;
                    options.ProviderOptions.DefaultScopes.Add("items");
                    options.UserOptions.RoleClaim = "role";
                })
                .AddAccountClaimsPrincipalFactory<ArrayClaimsPrincipalFactory<RemoteUserAccount>>();

            string itemsUrl = builder.Configuration["ItemsUrl"] ?? throw new ApplicationException("Items URL is null");

            builder.Services
                .AddHttpClient("items-api", c => c.BaseAddress = new Uri(itemsUrl))
                .AddHttpMessageHandler(s =>
                    s
                        .GetRequiredService<AuthorizationMessageHandler>()
                        .ConfigureHandler(authorizedUrls: new[] { itemsUrl }, scopes: new[] { "items" }));

            builder.Services
                .AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("items-api"))
                .AddScoped<IAppHttpClient, AppHttpClient>();

            builder.RootComponents.Add<App>("#app");

            await builder
                .Build()
                .RunAsync();
        }
    }
}
