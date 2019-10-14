using API.Models;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TodoList.Client.Components
{
  public class UsersComponent : ComponentBase
  {
    [Inject]
    private IAppHttpClient AppHttpClient { get; set; } = null!;

    protected IList<UserListApiModel> Users { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
      ApiCallResult<IList<UserListApiModel>> apiCallResult = await AppHttpClient.GetAsync<IList<UserListApiModel>>(ApiUrls.GetUsersList);

      Users = apiCallResult.IsSuccess
        ? apiCallResult.Value!
        : throw new Exception("API call is not successful");
    }

    protected async Task DeleteUserAsync(UserListApiModel user)
    {
      Users.Remove(user);

      await AppHttpClient.DeleteAsync(ApiUrls.DeleteUser.Replace(Urls.DeleteUser, user.Id.ToString()));
    }
  }
}
