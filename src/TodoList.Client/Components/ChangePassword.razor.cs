using API.Models;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TodoList.Client.Components
{
  public class ChangePasswordComponent : ComponentBase
  {
    [Inject]
    private IAppHttpClient AppHttpClient { get; set; }

    [Inject]
    private IUriHelper UriHelper { get; set; }

    protected UserChangePasswordModel UserChangePasswordModel;
    protected IEnumerable<string> Errors { get; set; }

    protected override void OnInit()
    {
      UserChangePasswordModel = new UserChangePasswordModel();
    }

    protected async Task OnChangePasswordAsync()
    {
      ApiCallResult changePasswordCallResult = await AppHttpClient.PutAsync(ApiUrls.ChangePassword, UserChangePasswordModel);

      if (changePasswordCallResult.IsSuccess)
      {
        UriHelper.NavigateTo(string.Empty);
      }
      else
      {
        Errors = changePasswordCallResult.Errors;
      }
    }
  }
}
