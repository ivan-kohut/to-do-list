using API.Models;
using Controllers.Tests.Extensions;
using Controllers.Tests.Fixtures;
using Entities;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Repositories;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Controllers.Tests
{
  public abstract class ApiControllerTestBase : ControllerTestBase
  {
    protected ApiControllerTestBase(TestServerFixture testServerFixture) : base(testServerFixture)
    {
    }

    protected override HttpContent CreateHttpContent(object? requestBody)
    {
      return new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
    }

    protected async Task<T> DeserializeResponseBodyAsync<T>(HttpResponseMessage response)
    {
      return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
    }

    protected async Task<ItemApiModel> SaveItemAsync(Item itemToSave)
    {
      using IServiceScope scope = Server.CreateScope();

      AppDbContext appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

      appDbContext.Add(itemToSave);

      await appDbContext.SaveChangesAsync();

      return new ItemApiModel
      {
        Id = itemToSave.Id,
        IsDone = itemToSave.Status == ItemStatus.Done,
        Text = itemToSave.Text,
        Priority = itemToSave.Priority
      };
    }
  }
}
