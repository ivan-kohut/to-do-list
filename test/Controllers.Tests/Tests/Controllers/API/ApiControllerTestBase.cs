using Controllers.Tests.Extensions;
using Controllers.Tests.Fixtures;
using Entities;
using API.Models;
using Newtonsoft.Json;
using Repositories;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Controllers.Tests
{
  public abstract class ApiControllerTestBase : ControllerTestBase
  {
    protected int UserId { get; }

    protected ApiControllerTestBase(TestServerFixture testServerFixture) : base(testServerFixture)
    {
      this.UserId = testServerFixture.UserId;
    }

    protected override HttpContent CreateHttpContent(object requestBody)
    {
      return new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
    }

    protected async Task<T> DeserializeResponseBodyAsync<T>(HttpResponseMessage response)
    {
      return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
    }

    protected async Task<ItemApiModel> SaveItemAsync(Item itemToSave)
    {
      using (AppDbContext appDbContext = Server.GetService<AppDbContext>())
      {
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

    protected async Task<UserListApiModel> SaveUserAsync(User userToSave)
    {
      using (AppDbContext appDbContext = Server.GetService<AppDbContext>())
      {
        appDbContext.Add(userToSave);

        await appDbContext.SaveChangesAsync();

        return new UserListApiModel
        {
          Id = userToSave.Id,
          Name = userToSave.UserName,
          Email = userToSave.Email,
          IsEmailConfirmed = userToSave.EmailConfirmed
        };
      }
    }
  }
}
