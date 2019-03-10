using Controllers.Tests.Extensions;
using Controllers.Tests.Fixtures;
using Entities;
using Newtonsoft.Json;
using Repositories;
using Services;
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

    protected T DeserializeResponseBody<T>(HttpResponseMessage response)
    {
      return JsonConvert.DeserializeObject<T>(response.Content.ReadAsStringAsync().Result);
    }

    protected async Task<ItemDTO> SaveItemAsync(Item itemToSave)
    {
      using (AppDbContext appDbContext = Server.GetService<AppDbContext>())
      {
        appDbContext.Add(itemToSave);

        await appDbContext.SaveChangesAsync();

        return new ItemDTO
        {
          Id = itemToSave.Id,
          Text = itemToSave.Text,
          StatusId = (int)itemToSave.Status,
          Priority = itemToSave.Priority
        };
      }
    }

    protected async Task<UserDTO> SaveUserAsync(User userToSave)
    {
      using (AppDbContext appDbContext = Server.GetService<AppDbContext>())
      {
        appDbContext.Add(userToSave);

        await appDbContext.SaveChangesAsync();

        return new UserDTO
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
