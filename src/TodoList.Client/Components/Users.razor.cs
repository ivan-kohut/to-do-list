using API.Models;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TodoList.Client.Components
{
  public class UsersComponent : ComponentBase
  {
    [Inject]
    private IAppHttpClient AppHttpClient { get; set; }

    protected IList<UserListApiModel> Users { get; set; }

    protected override async Task OnInitAsync()
    {
      Users = (await AppHttpClient.GetAsync<IList<UserListApiModel>>(ApiUrls.GetUsersList)).Value;
    }

    protected async Task DeleteUserAsync(UserListApiModel user)
    {
      Users.Remove(user);

      await AppHttpClient.DeleteAsync(ApiUrls.DeleteUser.Replace(Urls.DeleteUser, user.Id.ToString()));
    }
  }
}
