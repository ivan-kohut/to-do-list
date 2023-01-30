using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TodoList.Items.API.Models;
using TodoList.Items.Domain.Aggregates.ItemAggregate;
using TodoList.Items.Infrastructure;
using TodoList.Items.IntegrationTests.Infrastructure;

namespace TodoList.Items.IntegrationTests.Tests.Controllers
{
    public abstract class ControllerTestBase : TestBase
    {
        protected int UserId { get; }

        protected int IdentityId { get; }

        protected ControllerTestBase(ItemsWebApplicationFactory applicationFactory) : base(applicationFactory)
        {
            this.UserId = applicationFactory.User.Id;
            this.IdentityId = applicationFactory.User.IdentityId;
        }

        protected async Task<HttpResponseMessage> GetAsync(string url)
        {
            return await Client.GetAsync(url);
        }

        protected async Task<HttpResponseMessage> PostAsync(string url, object requestBody)
        {
            using HttpContent httpContent = CreateHttpContent(requestBody);
            return await Client.PostAsync(url, httpContent);
        }

        protected async Task<HttpResponseMessage> PutAsync(string url, object? requestBody)
        {
            using HttpContent httpContent = CreateHttpContent(requestBody);
            return await Client.PutAsync(url, httpContent);
        }

        protected async Task<HttpResponseMessage> DeleteAsync(string url)
        {
            return await Client.DeleteAsync(url);
        }

        protected async Task<ItemApiModel> SaveItemAsync(Item itemToSave)
        {
            using IServiceScope scope = Server.Services.CreateScope();

            ItemsDbContext itemsDbContext = scope.ServiceProvider.GetRequiredService<ItemsDbContext>();

            itemsDbContext.Add(itemToSave);

            await itemsDbContext.SaveChangesAsync();

            return new ItemApiModel
            {
                Id = itemToSave.Id,
                IsDone = itemToSave.IsDone,
                Text = itemToSave.Text,
                Priority = itemToSave.Priority
            };
        }

        protected static async Task<T> DeserializeResponseBodyAsync<T>(HttpResponseMessage response)
        {
            return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync()) ?? throw new Exception("The deserialized object is null");
        }

        private static HttpContent CreateHttpContent(object? requestBody)
        {
            return new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
        }
    }
}
