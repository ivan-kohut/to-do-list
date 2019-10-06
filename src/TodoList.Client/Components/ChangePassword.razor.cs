using API.Models;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TodoList.Client.Components
{
  public class ChangePasswordComponent : LoadingSpinnerComponentBase
  {
    [Inject]
    private IAppHttpClient AppHttpClient { get; set; }

    [Inject]
    private NavigationManager NavigationManager { get; set; }

    protected UserChangePasswordModel UserChangePasswordModel;
    protected IEnumerable<string> Errors { get; set; }

    protected override void OnInitialized()
    {
      UserChangePasswordModel = new UserChangePasswordModel();
    }

    protected override async Task HandleEventAsync()
    {
      ApiCallResult changePasswordCallResult = await AppHttpClient.PutAsync(ApiUrls.ChangePassword, UserChangePasswordModel);

      if (changePasswordCallResult.IsSuccess)
      {
        NavigationManager.NavigateTo(string.Empty);
      }
      else
      {
        Errors = changePasswordCallResult.Errors;
      }
    }
  }
}
