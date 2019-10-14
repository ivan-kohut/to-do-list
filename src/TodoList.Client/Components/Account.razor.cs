using System;
using System.Threading.Tasks;
using API.Models;
using Microsoft.AspNetCore.Components;

namespace TodoList.Client.Components
{
  public class AccountComponent : ComponentBase
  {
    [Inject]
    private IAppHttpClient AppHttpClient { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    protected bool? IsTwoFactorAuthenticationEnabled { get; set; }

    protected override async Task OnInitializedAsync()
    {
      ApiCallResult<UserTwoFactorAuthEnabledModel> apiCallResult = await AppHttpClient.GetAsync<UserTwoFactorAuthEnabledModel>(ApiUrls.IsTwoFactorAuthenticationEnabled);

      IsTwoFactorAuthenticationEnabled = apiCallResult.IsSuccess
        ? apiCallResult.Value!.IsEnabled
        : throw new Exception("API call is not successful");
    }

    protected async Task OnDisableTwoFactorAuthenticationAsync()
    {
      await AppHttpClient.PutAsync(ApiUrls.DisableTwoFactorAuthentication, null);

      NavigationManager.NavigateTo(string.Empty);
    }
  }
}
